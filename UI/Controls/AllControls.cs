// UI/Controls/AllControls.cs
// Contains: MedicinesControl, MedicineDialog, BillingControl,
//           PurchasesControl, SuppliersControl, SupplierDialog,
//           CustomersControl, ReportsControl, AuditControl,
//           UsersControl, UserDialog

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MedicalStoreMS.BusinessLogic;
using MedicalStoreMS.DataAccess;
using MedicalStoreMS.Models;
using MedicalStoreMS.Reports;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.UI.Controls
{
    // ══════════════════════════════════════════════════════════════
    //  MEDICINES
    // ══════════════════════════════════════════════════════════════
    public class MedicinesControl : UserControl
    {
        private readonly MedicineService _svc = new MedicineService();
        private readonly SupplierRepository _supRep = new SupplierRepository();
        private DataGridView _grid;
        private TextBox _txtSearch;
        private ComboBox _cmbFilter;
        private Label _lblCount;
        private List<Medicine> _medicines;

        public MedicinesControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); LoadMeds(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("💊  Medicine Inventory", 0, 0));

            _txtSearch = UIHelper.MakeSearchBox(new Point(0, 44), "🔍  Search name, batch, category…", 310);
            _txtSearch.TextChanged += (s, e) => Filter();

            _cmbFilter = new ComboBox { Location = new Point(322, 44), Size = new Size(160, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
            _cmbFilter.Items.AddRange(new[] { "All", "Low Stock", "Expired", "Near Expiry (30d)" });
            _cmbFilter.SelectedIndex = 0;
            _cmbFilter.SelectedIndexChanged += (s, e) => LoadMeds();

            var btnAdd = UIHelper.MakeButton("Add", AppTheme.Success, new Point(500, 42), icon: "➕");
            var btnEdit = UIHelper.MakeButton("Edit", AppTheme.Primary, new Point(622, 42), icon: "✏️");
            var btnDel = UIHelper.MakeButton("Delete", AppTheme.Danger, new Point(744, 42), icon: "🗑");
            var btnPrint = UIHelper.MakeButton("Print", AppTheme.Info, new Point(866, 42), icon: "🖨");
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDel.Click += BtnDel_Click;
            btnPrint.Click += (s, e) => { if (_medicines != null) ReportPrinter.PrintInventory(_medicines); };

            _lblCount = new Label { AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, Location = new Point(2, 82) };

            Controls.AddRange(new Control[] { _txtSearch, _cmbFilter, btnAdd, btnEdit, btnDel, btnPrint, _lblCount });

            _grid = UIHelper.MakeGrid();
            _grid.Columns.AddRange(
                UIHelper.Col("MedicineID", "#", 44),
                UIHelper.Col("MedicineName", "Medicine", 190),
                UIHelper.Col("BatchNo", "Batch", 90),
                UIHelper.Col("Category", "Category", 110),
                UIHelper.Col("Quantity", "Stock", 64),
                UIHelper.Col("UnitPrice", "Price (Rs)", 96),
                UIHelper.Col("ExpiryDate", "Expiry", 108),
                UIHelper.Col("SupplierName", "Supplier", 140),
                UIHelper.Col("MinStockLevel", "Min", 52));
            _grid.Bounds = new Rectangle(0, 98, Width - 10, Height - 140);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.CellFormatting += GridFormat;
            Controls.Add(_grid);
        }

        private void LoadMeds()
        {
            _medicines = _cmbFilter.SelectedItem?.ToString() switch
            {
                "Low Stock" => _svc.GetLowStock(),
                "Expired" => _svc.GetExpired(),
                "Near Expiry (30d)" => _svc.GetNearExpiry(30),
                _ => _svc.GetAll()
            };
            BindGrid(_medicines);
        }

        private void Filter()
        {
            if (_medicines == null) return;
            var kw = _txtSearch.Text.ToLower();
            var f = string.IsNullOrWhiteSpace(kw) ? _medicines
                : _medicines.FindAll(m => m.MedicineName.ToLower().Contains(kw) || (m.BatchNo ?? "").ToLower().Contains(kw) || (m.Category ?? "").ToLower().Contains(kw));
            BindGrid(f);
        }

        private void BindGrid(List<Medicine> list)
        {
            _grid.Rows.Clear();
            foreach (var m in list)
                _grid.Rows.Add(m.MedicineID, m.MedicineName, m.BatchNo, m.Category, m.Quantity,
                    m.UnitPrice.ToString("N2"), m.ExpiryDate.ToString("yyyy-MM-dd"), m.SupplierName, m.MinStockLevel);
            _lblCount.Text = $"{list.Count} records";
        }

        private void GridFormat(object s, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= (_medicines?.Count ?? 0)) return;
            var m = _medicines[e.RowIndex];
            var cn = _grid.Columns[e.ColumnIndex].Name;
            if (cn == "ExpiryDate")
            {
                if (m.IsExpired) e.CellStyle.ForeColor = AppTheme.Danger;
                else if (m.IsNearExpiry) e.CellStyle.ForeColor = AppTheme.Warning;
            }
            if (cn == "Quantity" && m.IsLowStock) e.CellStyle.ForeColor = AppTheme.Warning;
        }

        private Medicine Sel() { var i = _grid.CurrentRow?.Index ?? -1; return i >= 0 && i < (_medicines?.Count ?? 0) ? _medicines[i] : null; }

        private void BtnAdd_Click(object s, EventArgs e)
        {
            using var d = new MedicineDialog(_supRep.GetAll());
            if (d.ShowDialog() == DialogResult.OK) { var (ok, msg) = _svc.Add(d.Medicine); MsgBox(msg, ok); if (ok) LoadMeds(); }
        }
        private void BtnEdit_Click(object s, EventArgs e)
        {
            var m = Sel(); if (m == null) { MsgBox("Select a row first.", false); return; }
            using var d = new MedicineDialog(_supRep.GetAll(), m);
            if (d.ShowDialog() == DialogResult.OK) { var (ok, msg) = _svc.Update(d.Medicine); MsgBox(msg, ok); if (ok) LoadMeds(); }
        }
        private void BtnDel_Click(object s, EventArgs e)
        {
            var m = Sel(); if (m == null) return;
            if (Confirm($"Remove '{m.MedicineName}' from inventory?")) { var (ok, msg) = _svc.Delete(m.MedicineID, m.MedicineName); MsgBox(msg, ok); if (ok) LoadMeds(); }
        }

        private static void MsgBox(string msg, bool ok) => MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        private static bool Confirm(string msg) => MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    // ── Medicine Add/Edit Dialog ─────────────────────────────────
    public class MedicineDialog : Form
    {
        public Medicine Medicine { get; private set; }
        private TextBox _name, _batch, _cat, _desc, _price, _qty, _minStock;
        private DateTimePicker _exp;
        private ComboBox _sup;
        private List<Supplier> _suppliers;

        public MedicineDialog(List<Supplier> suppliers, Medicine existing = null)
        {
            _suppliers = suppliers;
            Medicine = existing ?? new Medicine { MinStockLevel = 10, ExpiryDate = DateTime.Today.AddYears(1) };
            Text = Medicine.MedicineID == 0 ? "Add Medicine" : "Edit Medicine";
            Size = new Size(480, 560); MinimumSize = Size; MaximumSize = Size;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = Color.White; Font = AppTheme.FontBody;
            BuildUI();
            if (existing != null) Populate();
        }

        private void BuildUI()
        {
            int y = 16;
            _name = Field("Medicine Name *", ref y);
            _batch = Field("Batch Number", ref y);
            _cat = Field("Category", ref y);

            // Expiry
            Lbl("Expiry Date *", y);
            _exp = new DateTimePicker { Bounds = new Rectangle(20, y + 22, 420, 28), Format = DateTimePickerFormat.Short, Value = Medicine.ExpiryDate };
            Controls.Add(_exp); y += 56;

            _price = Field("Unit Price (Rs) *", ref y);
            _qty = Field("Quantity in Stock", ref y);
            _minStock = Field("Minimum Stock Level", ref y);

            Lbl("Supplier", y);
            _sup = new ComboBox { Bounds = new Rectangle(20, y + 22, 420, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            _sup.Items.Add("— None —");
            foreach (var s in _suppliers) _sup.Items.Add(s.SupplierName);
            _sup.SelectedIndex = 0;
            Controls.Add(_sup); y += 56;

            _desc = Field("Notes / Description", ref y);

            var ok = UIHelper.MakeButton("💾 Save", AppTheme.Success, new Point(220, y + 10));
            var can = UIHelper.MakeButton("✕ Cancel", AppTheme.Danger, new Point(342, y + 10));
            ok.Click += Save_Click;
            can.Click += (_, __) => Close();
            Controls.AddRange(new Control[] { ok, can });
        }

        private void Populate()
        {
            _name.Text = Medicine.MedicineName; _batch.Text = Medicine.BatchNo;
            _cat.Text = Medicine.Category; _desc.Text = Medicine.Description;
            _price.Text = Medicine.UnitPrice.ToString(); _qty.Text = Medicine.Quantity.ToString();
            _minStock.Text = Medicine.MinStockLevel.ToString(); _exp.Value = Medicine.ExpiryDate;
            if (Medicine.SupplierID.HasValue)
            { int i = _suppliers.FindIndex(s => s.SupplierID == Medicine.SupplierID); if (i >= 0) _sup.SelectedIndex = i + 1; }
        }

        private void Save_Click(object s, EventArgs e)
        {
            if (!decimal.TryParse(_price.Text, out decimal price) || price <= 0) { MessageBox.Show("Enter a valid price."); return; }
            Medicine.MedicineName = _name.Text.Trim();
            Medicine.BatchNo = _batch.Text.Trim();
            Medicine.Category = _cat.Text.Trim();
            Medicine.Description = _desc.Text.Trim();
            Medicine.ExpiryDate = _exp.Value.Date;
            Medicine.UnitPrice = price;
            Medicine.Quantity = int.TryParse(_qty.Text, out int q) ? q : 0;
            Medicine.MinStockLevel = int.TryParse(_minStock.Text, out int ms) ? ms : 10;
            if (_sup.SelectedIndex > 0) Medicine.SupplierID = _suppliers[_sup.SelectedIndex - 1].SupplierID;
            DialogResult = DialogResult.OK; Close();
        }

        private TextBox Field(string label, ref int y) { Lbl(label, y); var tb = new TextBox { Bounds = new Rectangle(20, y + 22, 420, 28), BorderStyle = BorderStyle.FixedSingle }; Controls.Add(tb); y += 56; return tb; }
        private void Lbl(string t, int y) => Controls.Add(new Label { Text = t, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(20, y) });
    }

    // ══════════════════════════════════════════════════════════════
    //  BILLING
    // ══════════════════════════════════════════════════════════════
    public class BillingControl : UserControl
    {
        private readonly SalesService _svc = new SalesService();
        private readonly MedicineService _medSvc = new MedicineService();
        private DataGridView _cartGrid, _histGrid;
        private ComboBox _cmbMed;
        private TextBox _txtQty, _txtCust, _txtNotes;
        private NumericUpDown _nudDisc;
        private Label _lblTotal, _lblNet;
        private List<InvoiceDetail> _cart = new();
        private List<Medicine> _meds;

        public BillingControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); LoadMeds(); LoadHistory(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("🧾  Billing & Invoice", 0, 0));

            // — Cart area —
            var pLeft = new Panel { Bounds = new Rectangle(0, 44, 640, Height - 60), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom };

            pLeft.Controls.Add(new Label { Text = "Medicine", Location = new Point(0, 2), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _cmbMed = new ComboBox { Bounds = new Rectangle(0, 22, 370, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };

            pLeft.Controls.Add(new Label { Text = "Qty", Location = new Point(380, 2), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _txtQty = new TextBox { Bounds = new Rectangle(380, 22, 70, 30), Text = "1", BorderStyle = BorderStyle.FixedSingle };

            var btnAdd = UIHelper.MakeButton("Add →", AppTheme.Accent, new Point(458, 20));
            btnAdd.Click += AddItem;
            pLeft.Controls.AddRange(new Control[] { _cmbMed, _txtQty, btnAdd });

            _cartGrid = UIHelper.MakeGrid();
            _cartGrid.Columns.AddRange(UIHelper.Col("M", "Medicine", 230), UIHelper.Col("Q", "Qty", 56), UIHelper.Col("P", "Price", 90), UIHelper.Col("S", "Sub Total", 110));
            _cartGrid.Bounds = new Rectangle(0, 60, 620, 280);
            pLeft.Controls.Add(_cartGrid);

            var btnRem = UIHelper.MakeButton("Remove Item", AppTheme.Danger, new Point(0, 348));
            btnRem.Click += (s, e) => { if (_cartGrid.CurrentRow != null) { _cart.RemoveAt(_cartGrid.CurrentRow.Index); RefreshCart(); } };
            pLeft.Controls.Add(btnRem);
            Controls.Add(pLeft);

            // — Summary panel —
            var pRight = new Panel { BackColor = Color.White, Bounds = new Rectangle(650, 44, 300, 420) };
            pRight.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var p = UIHelper.RoundRect(new Rectangle(0, 0, pRight.Width - 1, pRight.Height - 1), 10);
                e.Graphics.FillPath(System.Drawing.Brushes.White, p);
                using var pen = new System.Drawing.Pen(AppTheme.Border, 1);
                e.Graphics.DrawPath(pen, p);
            };

            int ry = 16;
            void RLbl(string t, int y) => pRight.Controls.Add(new Label { Text = t, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            TextBox RTb(int y, int h = 28) { var tb = new TextBox { Bounds = new Rectangle(16, y, 268, h), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 250, 253) }; pRight.Controls.Add(tb); return tb; }

            RLbl("Customer Name", ry); _txtCust = RTb(ry + 18); _txtCust.PlaceholderText = "Walk-in Customer"; ry += 56;
            RLbl("Discount (Rs)", ry);
            _nudDisc = new NumericUpDown { Bounds = new Rectangle(16, ry + 18, 140, 28), DecimalPlaces = 2, Maximum = 99999, Font = AppTheme.FontBody };
            _nudDisc.ValueChanged += (s, e) => UpdateTotals();
            pRight.Controls.Add(_nudDisc); ry += 56;

            _lblTotal = new Label { Text = "Total:    Rs 0.00", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(16, ry) }; ry += 26;
            _lblNet = new Label { Text = "Net:       Rs 0.00", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = AppTheme.Success, AutoSize = true, Location = new Point(16, ry) }; ry += 40;
            pRight.Controls.AddRange(new Control[] { _lblTotal, _lblNet });

            RLbl("Notes", ry); _txtNotes = RTb(ry + 18, 60); _txtNotes.Multiline = true; ry += 90;

            var btnProc = UIHelper.MakeButton("✔  Process Sale", AppTheme.Success, new Point(16, ry));
            btnProc.Size = new Size(268, 44); btnProc.Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold); btnProc.Click += ProcessSale; ry += 54;
            var btnClr = UIHelper.MakeButton("🗑  Clear Cart", AppTheme.Warning, new Point(16, ry));
            btnClr.Size = new Size(268, 34); btnClr.Click += (_, __) => { _cart.Clear(); RefreshCart(); _nudDisc.Value = 0; };
            pRight.Controls.AddRange(new Control[] { btnProc, btnClr });
            Controls.Add(pRight);

            // — History grid —
            Controls.Add(new Label { Text = "Recent Sales", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, Height - 220) });
            _histGrid = UIHelper.MakeGrid();
            _histGrid.Columns.AddRange(UIHelper.Col("I", "Invoice#", 80), UIHelper.Col("D", "Date", 120), UIHelper.Col("C", "Customer", 140), UIHelper.Col("N", "Net (Rs)", 100), UIHelper.Col("P", "Payment", 90));
            _histGrid.Bounds = new Rectangle(0, Height - 196, 640, 180);
            _histGrid.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(_histGrid);
        }

        private void LoadMeds()
        {
            _meds = _medSvc.GetAll(); _cmbMed.Items.Clear();
            foreach (var m in _meds) _cmbMed.Items.Add($"{m.MedicineName}  (Stock: {m.Quantity})");
            if (_cmbMed.Items.Count > 0) _cmbMed.SelectedIndex = 0;
        }

        private void LoadHistory()
        {
            _histGrid.Rows.Clear();
            foreach (var inv in _svc.GetSales(DateTime.Today.AddDays(-30), DateTime.Today))
                _histGrid.Rows.Add(inv.InvoiceID, inv.InvoiceDate.ToString("dd/MM HH:mm"), inv.CustomerName, inv.NetAmount.ToString("N2"), inv.PaymentMode);
        }

        private void AddItem(object s, EventArgs e)
        {
            if (_cmbMed.SelectedIndex < 0) return;
            var m = _meds[_cmbMed.SelectedIndex];
            if (!int.TryParse(_txtQty.Text, out int qty) || qty <= 0) { MessageBox.Show("Invalid quantity."); return; }
            if (qty > m.Quantity) { MessageBox.Show($"Only {m.Quantity} units available."); return; }
            _cart.Add(new InvoiceDetail { MedicineID = m.MedicineID, MedicineName = m.MedicineName, Quantity = qty, UnitPrice = m.UnitPrice, SubTotal = qty * m.UnitPrice });
            RefreshCart();
        }

        private void RefreshCart()
        {
            _cartGrid.Rows.Clear();
            foreach (var d in _cart) _cartGrid.Rows.Add(d.MedicineName, d.Quantity, $"Rs {d.UnitPrice:N2}", $"Rs {d.SubTotal:N2}");
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal tot = 0; foreach (var d in _cart) tot += d.SubTotal;
            _lblTotal.Text = $"Total:    Rs {tot:N2}";
            _lblNet.Text = $"Net:       Rs {tot - _nudDisc.Value:N2}";
        }

        private void ProcessSale(object s, EventArgs e)
        {
            if (_cart.Count == 0) { MessageBox.Show("Cart is empty."); return; }
            decimal tot = 0; foreach (var d in _cart) tot += d.SubTotal;
            var inv = new Invoice { Details = _cart, TotalAmount = tot, Discount = _nudDisc.Value, NetAmount = tot - _nudDisc.Value, PaymentMode = "Cash", Notes = _txtNotes.Text };
            var (ok, id, msg) = _svc.ProcessSale(inv);
            MessageBox.Show(msg, ok ? "Sale Complete" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (ok)
            {
                if (MessageBox.Show("Print invoice?", "Print", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    ReportPrinter.PrintInvoice(_svc.GetById(id));
                _cart.Clear(); RefreshCart(); _nudDisc.Value = 0; _txtNotes.Clear();
                LoadMeds(); LoadHistory();
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  PURCHASES
    // ══════════════════════════════════════════════════════════════
    public class PurchasesControl : UserControl
    {
        private readonly PurchaseService _svc = new PurchaseService();
        private readonly MedicineService _medSvc = new MedicineService();
        private readonly SupplierRepository _supRep = new SupplierRepository();
        private DataGridView _histGrid, _itemGrid;
        private ComboBox _cmbSup, _cmbMed;
        private TextBox _txtQty, _txtCost;
        private Label _lblTotal;
        private List<PurchaseDetail> _items = new();
        private List<Medicine> _meds;
        private List<Supplier> _sups;
        private decimal _total;

        public PurchasesControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); LoadHistory(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("📦  Purchase Management", 0, 0));
            _sups = _supRep.GetAll(); _meds = _medSvc.GetAll();

            // — History —
            _histGrid = UIHelper.MakeGrid();
            _histGrid.Columns.AddRange(UIHelper.Col("I", "ID", 60), UIHelper.Col("D", "Date", 120), UIHelper.Col("S", "Supplier", 160), UIHelper.Col("T", "Amount", 100));
            _histGrid.Bounds = new Rectangle(680, 44, Width - 700, Height - 60);
            _histGrid.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_histGrid);

            int y = 50;
            void SLbl(string t) { Controls.Add(new Label { Text = t, Location = new Point(0, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary }); }

            SLbl("Supplier");
            _cmbSup = new ComboBox { Bounds = new Rectangle(0, y + 20, 320, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
            _cmbSup.Items.Add("— Select Supplier —"); foreach (var s in _sups) _cmbSup.Items.Add(s.SupplierName);
            _cmbSup.SelectedIndex = 0; Controls.Add(_cmbSup); y += 60;

            SLbl("Medicine");
            _cmbMed = new ComboBox { Bounds = new Rectangle(0, y + 20, 320, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var m in _meds) _cmbMed.Items.Add(m.MedicineName);
            if (_cmbMed.Items.Count > 0) _cmbMed.SelectedIndex = 0;
            Controls.Add(_cmbMed); y += 60;

            SLbl("Qty");
            _txtQty = new TextBox { Bounds = new Rectangle(0, y + 20, 100, 30), Text = "1", BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(new Label { Text = "Unit Cost (Rs)", Location = new Point(116, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _txtCost = new TextBox { Bounds = new Rectangle(116, y + 20, 130, 30), Text = "0.00", BorderStyle = BorderStyle.FixedSingle };
            var btnAdd = UIHelper.MakeButton("➕ Add", AppTheme.Accent, new Point(258, y + 18));
            btnAdd.Click += AddItem;
            Controls.AddRange(new Control[] { _txtQty, _txtCost, btnAdd }); y += 60;

            _itemGrid = UIHelper.MakeGrid();
            _itemGrid.Columns.AddRange(UIHelper.Col("M", "Medicine", 200), UIHelper.Col("Q", "Qty", 60), UIHelper.Col("C", "Cost", 90), UIHelper.Col("S", "Sub Total", 110));
            _itemGrid.Bounds = new Rectangle(0, y, 650, 190);
            Controls.Add(_itemGrid); y += 200;

            _lblTotal = new Label { Text = "Total:  Rs 0.00", Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold), ForeColor = AppTheme.Primary, AutoSize = true, Location = new Point(0, y) }; y += 40;
            Controls.Add(_lblTotal);

            var btnRec = UIHelper.MakeButton("✔ Record Purchase", AppTheme.Success, new Point(0, y));
            btnRec.Size = new Size(200, 44);
            btnRec.Click += RecordPurchase;
            Controls.Add(btnRec);
        }

        private void LoadHistory()
        {
            _histGrid.Rows.Clear();
            foreach (var p in _svc.GetAll(null, null))
                _histGrid.Rows.Add(p.PurchaseID, p.PurchaseDate.ToString("dd/MM/yy HH:mm"), p.SupplierName, $"Rs {p.TotalAmount:N2}");
        }

        private void AddItem(object s, EventArgs e)
        {
            if (_cmbMed.SelectedIndex < 0) return;
            var m = _meds[_cmbMed.SelectedIndex];
            if (!int.TryParse(_txtQty.Text, out int qty) || qty <= 0 || !decimal.TryParse(_txtCost.Text, out decimal cost) || cost < 0) { MessageBox.Show("Enter valid values."); return; }
            _items.Add(new PurchaseDetail { MedicineID = m.MedicineID, MedicineName = m.MedicineName, Quantity = qty, UnitCost = cost, SubTotal = qty * cost });
            RefreshItems();
        }

        private void RefreshItems()
        {
            _itemGrid.Rows.Clear(); _total = 0;
            foreach (var d in _items) { _total += d.SubTotal; _itemGrid.Rows.Add(d.MedicineName, d.Quantity, $"Rs {d.UnitCost:N2}", $"Rs {d.SubTotal:N2}"); }
            _lblTotal.Text = $"Total:  Rs {_total:N2}";
        }

        private void RecordPurchase(object s, EventArgs e)
        {
            if (_items.Count == 0) { MessageBox.Show("No items added."); return; }
            var p = new Purchase { SupplierID = _cmbSup.SelectedIndex > 0 ? _sups[_cmbSup.SelectedIndex - 1].SupplierID : (int?)null, TotalAmount = _total, Details = _items };
            var (ok, id, msg) = _svc.RecordPurchase(p);
            MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (ok) { _items.Clear(); RefreshItems(); LoadHistory(); }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  SUPPLIERS
    // ══════════════════════════════════════════════════════════════
    public class SuppliersControl : UserControl
    {
        private readonly SupplierRepository _repo = new SupplierRepository();
        private DataGridView _grid;
        private List<Supplier> _list;

        public SuppliersControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); Reload(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("🏢  Supplier Management", 0, 0));
            var a = UIHelper.MakeButton("➕ Add", AppTheme.Success, new Point(0, 44));
            var b = UIHelper.MakeButton("✏️ Edit", AppTheme.Primary, new Point(122, 44));
            var c = UIHelper.MakeButton("🗑 Delete", AppTheme.Danger, new Point(244, 44));
            a.Click += (_, __) => { using var d = new SupplierDialog(); if (d.ShowDialog() == DialogResult.OK) { _repo.Add(d.Supplier); Reload(); } };
            b.Click += (_, __) => { var s = Sel(); if (s == null) return; using var d = new SupplierDialog(s); if (d.ShowDialog() == DialogResult.OK) { _repo.Update(d.Supplier); Reload(); } };
            c.Click += (_, __) => { var s = Sel(); if (s == null) return; if (MessageBox.Show($"Delete '{s.SupplierName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _repo.Delete(s.SupplierID); Reload(); } };
            Controls.AddRange(new Control[] { a, b, c });
            _grid = UIHelper.MakeGrid();
            _grid.Columns.AddRange(UIHelper.Col("SupplierID", "#", 44), UIHelper.Col("SupplierName", "Supplier", 180), UIHelper.Col("ContactNo", "Contact", 130), UIHelper.Col("Email", "Email", 190), UIHelper.Col("Address", "Address", 220));
            _grid.Bounds = new Rectangle(0, 84, Width - 10, Height - 100);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_grid);
        }

        private void Reload() { _list = _repo.GetAll(); _grid.Rows.Clear(); foreach (var s in _list) _grid.Rows.Add(s.SupplierID, s.SupplierName, s.ContactNo, s.Email, s.Address); }
        private Supplier Sel() { var i = _grid.CurrentRow?.Index ?? -1; return i >= 0 && i < (_list?.Count ?? 0) ? _list[i] : null; }
    }

    public class SupplierDialog : Form
    {
        public Supplier Supplier { get; private set; }
        private TextBox _name, _contact, _email, _addr;

        public SupplierDialog(Supplier s = null)
        {
            Supplier = s ?? new Supplier();
            Text = s == null ? "Add Supplier" : "Edit Supplier";
            Size = new Size(420, 330); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; BackColor = Color.White;
            int y = 16;
            _name = F("Supplier Name *", ref y); _contact = F("Contact No", ref y);
            _email = F("Email", ref y); _addr = F("Address", ref y);
            if (s != null) { _name.Text = s.SupplierName; _contact.Text = s.ContactNo; _email.Text = s.Email; _addr.Text = s.Address; }
            var ok = UIHelper.MakeButton("💾 Save", AppTheme.Success, new Point(210, y + 8));
            ok.Click += (_, __) => { Supplier.SupplierName = _name.Text; Supplier.ContactNo = _contact.Text; Supplier.Email = _email.Text; Supplier.Address = _addr.Text; DialogResult = DialogResult.OK; Close(); };
            Controls.Add(ok);
        }

        private TextBox F(string l, ref int y) { Controls.Add(new Label { Text = l, Location = new Point(20, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary }); var tb = new TextBox { Bounds = new Rectangle(20, y + 20, 360, 28), BorderStyle = BorderStyle.FixedSingle }; Controls.Add(tb); y += 56; return tb; }
    }

    // ══════════════════════════════════════════════════════════════
    //  CUSTOMERS
    // ══════════════════════════════════════════════════════════════
    public class CustomersControl : UserControl
    {
        private readonly CustomerRepository _repo = new CustomerRepository();
        private DataGridView _grid;
        private List<Customer> _list;

        public CustomersControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); Reload(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("👥  Customer Management", 0, 0));
            var a = UIHelper.MakeButton("➕ Add", AppTheme.Success, new Point(0, 44));
            var b = UIHelper.MakeButton("🗑 Delete", AppTheme.Danger, new Point(122, 44));
            a.Click += (_, __) =>
            {
                string n = Microsoft.VisualBasic.Interaction.InputBox("Customer Name:", "Add Customer", "");
                string c = Microsoft.VisualBasic.Interaction.InputBox("Contact No:", "Add Customer", "");
                if (!string.IsNullOrWhiteSpace(n)) { _repo.Add(new Customer { CustomerName = n, ContactNo = c }); Reload(); }
            };
            b.Click += (_, __) => { var cu = Sel(); if (cu == null) return; if (MessageBox.Show($"Delete '{cu.CustomerName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _repo.Delete(cu.CustomerID); Reload(); } };
            Controls.AddRange(new Control[] { a, b });
            _grid = UIHelper.MakeGrid();
            _grid.Columns.AddRange(UIHelper.Col("CustomerID", "#", 44), UIHelper.Col("CustomerName", "Customer", 190), UIHelper.Col("ContactNo", "Contact", 130), UIHelper.Col("Email", "Email", 180), UIHelper.Col("CreatedAt", "Registered", 130));
            _grid.Bounds = new Rectangle(0, 84, Width - 10, Height - 100);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_grid);
        }

        private void Reload() { _list = _repo.GetAll(); _grid.Rows.Clear(); foreach (var c in _list) _grid.Rows.Add(c.CustomerID, c.CustomerName, c.ContactNo, c.Email, c.CreatedAt.ToString("dd/MM/yyyy")); }
        private Customer Sel() { var i = _grid.CurrentRow?.Index ?? -1; return i >= 0 && i < (_list?.Count ?? 0) ? _list[i] : null; }
    }

    // ══════════════════════════════════════════════════════════════
    //  REPORTS
    // ══════════════════════════════════════════════════════════════
    public class ReportsControl : UserControl
    {
        private readonly SalesService _sales = new SalesService();
        private readonly MedicineService _medSvc = new MedicineService();
        private readonly PurchaseService _purSvc = new PurchaseService();
        private DataGridView _grid;
        private DateTimePicker _dtFrom, _dtTo;
        private ComboBox _cmbRep;
        private Label _lblSummary;

        public ReportsControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => Build(); }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("📈  Reports & Analytics", 0, 0));

            Controls.Add(new Label { Text = "Report Type", Location = new Point(0, 50), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _cmbRep = new ComboBox { Bounds = new Rectangle(0, 70, 210, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbRep.Items.AddRange(new[] { "Sales Report", "Purchase Report", "Inventory Report", "Expired Medicines", "Low Stock Report" });
            _cmbRep.SelectedIndex = 0; Controls.Add(_cmbRep);

            Controls.Add(new Label { Text = "From", Location = new Point(226, 50), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _dtFrom = new DateTimePicker { Bounds = new Rectangle(226, 70, 140, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-1) };
            Controls.Add(_dtFrom);

            Controls.Add(new Label { Text = "To", Location = new Point(378, 50), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _dtTo = new DateTimePicker { Bounds = new Rectangle(378, 70, 140, 30), Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            Controls.Add(_dtTo);

            var btnRun = UIHelper.MakeButton("▶  Generate", AppTheme.Primary, new Point(532, 68)); btnRun.Click += Run;
            var btnPrt = UIHelper.MakeButton("Print", AppTheme.Info, new Point(654, 68)); btnPrt.Click += (s, e) => MessageBox.Show("Print feature coming soon.", "Print");

            _lblSummary = new Label { AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, Location = new Point(0, 106) };
            Controls.Add(_lblSummary);

            _grid = UIHelper.MakeGrid();
            _grid.Bounds = new Rectangle(0, 118, Width - 10, Height - 144);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_grid);
        }

        private void Run(object s, EventArgs e)
        {
            _grid.Columns.Clear(); _grid.Rows.Clear();

            switch (_cmbRep.SelectedItem?.ToString())
            {
                case "Sales Report":
                    _grid.Columns.AddRange(UIHelper.Col("I", "Invoice#", 80), UIHelper.Col("D", "Date", 120), UIHelper.Col("C", "Customer", 150), UIHelper.Col("T", "Total", 100), UIHelper.Col("Disc", "Disc", 80), UIHelper.Col("N", "Net", 110), UIHelper.Col("P", "Payment", 90));
                    decimal salesTotal = 0;
                    foreach (var inv in _sales.GetSales(_dtFrom.Value, _dtTo.Value))
                    { salesTotal += inv.NetAmount; _grid.Rows.Add(inv.InvoiceID, inv.InvoiceDate.ToString("dd/MM HH:mm"), inv.CustomerName, $"Rs {inv.TotalAmount:N2}", $"Rs {inv.Discount:N2}", $"Rs {inv.NetAmount:N2}", inv.PaymentMode); }
                    _lblSummary.Text = $"Total Revenue: Rs {salesTotal:N2}  |  {_grid.Rows.Count} transactions";
                    break;

                case "Purchase Report":
                    _grid.Columns.AddRange(UIHelper.Col("I", "ID", 60), UIHelper.Col("D", "Date", 120), UIHelper.Col("S", "Supplier", 170), UIHelper.Col("T", "Amount", 120));
                    decimal purTotal = 0;
                    foreach (var p in _purSvc.GetAll(_dtFrom.Value, _dtTo.Value))
                    { purTotal += p.TotalAmount; _grid.Rows.Add(p.PurchaseID, p.PurchaseDate.ToString("dd/MM HH:mm"), p.SupplierName, $"Rs {p.TotalAmount:N2}"); }
                    _lblSummary.Text = $"Total Purchased: Rs {purTotal:N2}  |  {_grid.Rows.Count} orders";
                    break;

                case "Inventory Report":
                    _grid.Columns.AddRange(UIHelper.Col("N", "Medicine", 190), UIHelper.Col("C", "Category", 120), UIHelper.Col("Q", "Stock", 70), UIHelper.Col("P", "Price", 90), UIHelper.Col("E", "Expiry", 110), UIHelper.Col("S", "Supplier", 140));
                    var all = _medSvc.GetAll();
                    foreach (var m in all) _grid.Rows.Add(m.MedicineName, m.Category, m.Quantity, $"Rs {m.UnitPrice:N2}", m.ExpiryDate.ToString("dd/MM/yy"), m.SupplierName);
                    _lblSummary.Text = $"{all.Count} medicines  |  Value: Rs {InventoryValue(all):N2}";
                    break;

                case "Expired Medicines":
                    _grid.Columns.AddRange(UIHelper.Col("N", "Medicine", 190), UIHelper.Col("B", "Batch", 100), UIHelper.Col("Q", "Stock", 70), UIHelper.Col("E", "Expired On", 120), UIHelper.Col("S", "Supplier", 140));
                    var exp = _medSvc.GetExpired();
                    foreach (var m in exp) _grid.Rows.Add(m.MedicineName, m.BatchNo, m.Quantity, m.ExpiryDate.ToString("dd/MM/yyyy"), m.SupplierName);
                    _lblSummary.Text = $"{exp.Count} expired medicines found";
                    break;

                case "Low Stock Report":
                    _grid.Columns.AddRange(UIHelper.Col("N", "Medicine", 190), UIHelper.Col("Q", "Current Stock", 120), UIHelper.Col("M", "Minimum Required", 130), UIHelper.Col("S", "Supplier", 150));
                    var low = _medSvc.GetLowStock();
                    foreach (var m in low) _grid.Rows.Add(m.MedicineName, m.Quantity, m.MinStockLevel, m.SupplierName);
                    _lblSummary.Text = $"{low.Count} medicines below minimum stock level";
                    break;
            }
        }

        private static decimal InventoryValue(List<Medicine> meds) { decimal v = 0; foreach (var m in meds) v += m.Quantity * m.UnitPrice; return v; }
    }

    // ══════════════════════════════════════════════════════════════
    //  AUDIT LOG
    // ══════════════════════════════════════════════════════════════
    public class AuditControl : UserControl
    {
        private readonly AuditRepository _repo = new AuditRepository();
        private DataGridView _grid;

        public AuditControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); Reload(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("🔍  Audit Log", 0, 0));
            var btnRef = UIHelper.MakeButton("🔄 Refresh", AppTheme.Info, new Point(0, 44));
            btnRef.Click += (_, __) => Reload();
            Controls.Add(btnRef);
            _grid = UIHelper.MakeGrid();
            _grid.Columns.AddRange(UIHelper.Col("LogID", "#", 44), UIHelper.Col("Username", "User", 110), UIHelper.Col("Action", "Action", 140), UIHelper.Col("Details", "Details", 360), UIHelper.Col("Timestamp", "Timestamp", 150));
            _grid.Bounds = new Rectangle(0, 84, Width - 10, Height - 100);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_grid);
        }

        private void Reload()
        {
            _grid.Rows.Clear();
            var dt = _repo.GetRecent(500);
            foreach (System.Data.DataRow r in dt.Rows)
                _grid.Rows.Add(r["LogID"], r["Username"], r["Action"], r["Details"], Convert.ToDateTime(r["Timestamp"]).ToString("dd/MM/yyyy HH:mm:ss"));
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  USERS (Admin only)
    // ══════════════════════════════════════════════════════════════
    public class UsersControl : UserControl
    {
        private readonly UserRepository _repo = new UserRepository();
        private DataGridView _grid;
        private List<User> _users;

        public UsersControl() { BackColor = AppTheme.Background; Dock = DockStyle.Fill; Load += (s, e) => { Build(); Reload(); }; }

        private void Build()
        {
            Controls.Add(UIHelper.MakePageTitle("👤  User Management  (Admin)", 0, 0));
            var a = UIHelper.MakeButton("➕ Add", AppTheme.Success, new Point(0, 44));
            var b = UIHelper.MakeButton("🔄 Toggle Status", AppTheme.Warning, new Point(122, 44));
            var c = UIHelper.MakeButton("🔑 Reset Password", AppTheme.Info, new Point(254, 44));
            a.Click += (_, __) => { using var d = new UserDialog(); if (d.ShowDialog() == DialogResult.OK) { _repo.Create(d.NewUser); Reload(); } };
            b.Click += (_, __) => { var u = Sel(); if (u == null) return; _repo.ToggleStatus(u.UserID); Reload(); };
            c.Click += (_, __) => { var u = Sel(); if (u == null) return; string pwd = Microsoft.VisualBasic.Interaction.InputBox("New password:", "Reset Password", ""); if (!string.IsNullOrWhiteSpace(pwd)) { _repo.ChangePassword(u.UserID, pwd); MessageBox.Show("Password updated."); } };
            Controls.AddRange(new Control[] { a, b, c });
            _grid = UIHelper.MakeGrid();
            _grid.Columns.AddRange(UIHelper.Col("UserID", "#", 44), UIHelper.Col("Username", "Username", 140), UIHelper.Col("FullName", "Full Name", 180), UIHelper.Col("Role", "Role", 80), UIHelper.Col("IsActive", "Active", 70), UIHelper.Col("CreatedAt", "Created", 130));
            _grid.Bounds = new Rectangle(0, 84, Width - 10, Height - 100);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(_grid);
        }

        private void Reload() { _users = _repo.GetAll(); _grid.Rows.Clear(); foreach (var u in _users) _grid.Rows.Add(u.UserID, u.Username, u.FullName, u.Role, u.IsActive ? "✔ Yes" : "✖ No", u.CreatedAt.ToString("dd/MM/yyyy")); }
        private User Sel() { var i = _grid.CurrentRow?.Index ?? -1; return i >= 0 && i < (_users?.Count ?? 0) ? _users[i] : null; }
    }

    public class UserDialog : Form
    {
        public User NewUser { get; private set; }
        private TextBox _user, _full, _pwd;
        private ComboBox _role;

        public UserDialog()
        {
            Text = "Add New User"; Size = new Size(400, 320); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; BackColor = Color.White;
            int y = 16;
            _user = UF("Username *", ref y); _full = UF("Full Name *", ref y); _pwd = UF("Password *", ref y); _pwd.UseSystemPasswordChar = true;
            Controls.Add(new Label { Text = "Role", Location = new Point(20, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary });
            _role = new ComboBox { Bounds = new Rectangle(20, y + 20, 340, 28), DropDownStyle = ComboBoxStyle.DropDownList }; _role.Items.AddRange(new[] { "Staff", "Admin" }); _role.SelectedIndex = 0; Controls.Add(_role); y += 52;
            var ok = UIHelper.MakeButton("✔ Create User", AppTheme.Success, new Point(180, y + 8));
            ok.Click += (_, __) => { if (string.IsNullOrWhiteSpace(_user.Text) || string.IsNullOrWhiteSpace(_pwd.Text)) { MessageBox.Show("Username and password required."); return; } NewUser = new User { Username = _user.Text, PasswordHash = _pwd.Text, FullName = _full.Text, Role = _role.Text }; DialogResult = DialogResult.OK; Close(); };
            Controls.Add(ok);
        }

        private TextBox UF(string l, ref int y) { Controls.Add(new Label { Text = l, Location = new Point(20, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary }); var tb = new TextBox { Bounds = new Rectangle(20, y + 20, 340, 28), BorderStyle = BorderStyle.FixedSingle }; Controls.Add(tb); y += 56; return tb; }
    }
}