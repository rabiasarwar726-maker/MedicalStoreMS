// UI/Forms/MainForm.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MedicalStoreMS.BusinessLogic;
using MedicalStoreMS.UI.Controls;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.UI.Forms
{
    public class MainForm : Form
    {
        private Panel _sidebar, _contentArea, _topBar;
        private Label _userLabel, _clockLabel;
        private Button _btnLogout;
        private NavButton _activeNav;
        private Timer _clock;

        public MainForm()
        {
            Text = $"MediCare — {Session.CurrentUser?.FullName}";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 700);
            BackColor = AppTheme.Background;
            Font = AppTheme.FontBody;

            BuildTopBar();
            BuildContent();
            BuildSidebar();
            StartClock();
            ShowDashboard();
        }

        private void BuildTopBar()
        {
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };
            _topBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 51, _topBar.Width, 51);

            var title = new Label
            {
                Text = "MediCare — Medical Store Management",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = AppTheme.Primary,
                AutoSize = true,
                Location = new Point(220, 15)
            };

            _userLabel = new Label
            {
                Text = $"👤  {Session.CurrentUser?.FullName}  [{Session.CurrentUser?.Role}]",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true
            };

            _clockLabel = new Label
            {
                Text = DateTime.Now.ToString("ddd dd MMM  HH:mm:ss"),
                Font = new Font("Consolas", 9),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true
            };

            _btnLogout = new Button
            {
                Text = "Logout",
                Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AppTheme.Danger,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 28),
                Cursor = Cursors.Hand
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            _btnLogout.Click += (s, e) =>
            {
                new AuthService().Logout();
                new LoginForm().Show();
                Close();
            };

            _topBar.Controls.AddRange(new Control[] { title, _userLabel, _clockLabel, _btnLogout });

            void LayoutTopBar()
            {
                int right = _topBar.Width - 12;
                _btnLogout.Location = new Point(right - 92, 12);
                _userLabel.Location = new Point(right - 92 - 16 - _userLabel.PreferredWidth, 10);
                _clockLabel.Location = new Point(right - 92 - 16 - _clockLabel.PreferredWidth, 28);
            }

            _topBar.Resize += (s, e) => LayoutTopBar();
            _topBar.Layout += (s, e) => LayoutTopBar();

            Controls.Add(_topBar);
        }

        private void BuildContent()
        {
            _contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Background,
                Padding = new Padding(24, 60, 24, 20)
            };
            Controls.Add(_contentArea);
        }

        private void BuildSidebar()
        {
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = AppTheme.Sidebar
            };
            _sidebar.Paint += SidebarPaint;

            _sidebar.Controls.Add(new Label
            {
                Text = "MediCare",
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(52, 13)
            });

            AddSectionLabel("NAVIGATION", 60);

            var items = new (string icon, string label, string section, Action act)[]
            {
                ("📊", "Dashboard",  "",        ShowDashboard),
                ("💊", "Medicines",  "",        ShowMedicines),
                ("🧾", "Billing",    "",        ShowBilling),
                ("📦", "Purchases",  "",        ShowPurchases),
                ("🏢", "Suppliers",  "",        ShowSuppliers),
                ("👥", "Customers",  "REPORTS", ShowCustomers),
                ("📈", "Reports",    "",        ShowReports),
                ("🔍", "Audit Log",  "ADMIN",   ShowAudit),
                ("👤", "Users",      "",        ShowUsers),
            };

            int y = 78;
            foreach (var (icon, label, section, act) in items)
            {
                if (!string.IsNullOrEmpty(section))
                {
                    AddSectionLabel(section, y + 4);
                    y += 26;
                }
                var nav = new NavButton(icon, label, act) { Location = new Point(0, y), Tag = label };
                nav.Click += (s, e) => SetActive(nav);
                _sidebar.Controls.Add(nav);
                y += 46;
            }

            Controls.Add(_sidebar);
        }

        private void AddSectionLabel(string text, int y)
        {
            _sidebar.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 140, 200),
                AutoSize = true,
                Location = new Point(14, y)
            });
        }

        private void SidebarPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var dec = new SolidBrush(Color.FromArgb(15, 255, 255, 255));
            g.FillEllipse(dec, -40, _sidebar.Height - 130, 150, 150);
            using var lb = new SolidBrush(Color.FromArgb(21, 95, 180));
            using var lp = RoundRect(new Rectangle(11, 8, 33, 33), 8);
            g.FillPath(lb, lp);
            using var cp = new Pen(Color.White, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(cp, 28, 17, 28, 33);
            g.DrawLine(cp, 20, 25, 36, 25);
        }

        private void SetActive(NavButton nav)
        {
            if (_activeNav != null) _activeNav.IsActive = false;
            nav.IsActive = true;
            _activeNav = nav;
        }

        public void NavigateTo(string page)
        {
            switch (page)
            {
                case "Billing": ShowBilling(); ActivateNav("Billing"); break;
                case "Medicines": ShowMedicines(); ActivateNav("Medicines"); break;
                case "Purchases": ShowPurchases(); ActivateNav("Purchases"); break;
                case "Reports": ShowReports(); ActivateNav("Reports"); break;
            }
        }

        private void ActivateNav(string label)
        {
            foreach (Control c in _sidebar.Controls)
                if (c is NavButton nb && nb.Tag?.ToString() == label)
                    SetActive(nb);
        }

        private void LoadPage(Control ctrl)
        {
            _contentArea.Controls.Clear();
            ctrl.Dock = DockStyle.Fill;
            _contentArea.Controls.Add(ctrl);
        }

        private void ShowDashboard() => LoadPage(new DashboardControl());
        private void ShowMedicines() => LoadPage(new MedicinesControl());
        private void ShowBilling() => LoadPage(new BillingControl());
        private void ShowPurchases() => LoadPage(new PurchasesControl());
        private void ShowSuppliers() => LoadPage(new SuppliersControl());
        private void ShowCustomers() => LoadPage(new CustomersControl());
        private void ShowReports() => LoadPage(new ReportsControl());
        private void ShowAudit() => LoadPage(new AuditControl());
        private void ShowUsers()
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Admin access required.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadPage(new UsersControl());
        }

        private void StartClock()
        {
            _clock = new Timer { Interval = 1000 };
            _clock.Tick += (s, e) =>
                _clockLabel.Text = DateTime.Now.ToString("ddd dd MMM  HH:mm:ss");
            _clock.Start();
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var p = new GraphicsPath(); int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        { _clock?.Stop(); base.OnFormClosed(e); }
    }

    public class NavButton : Control
    {
        private readonly string _icon, _label;
        private readonly Action _action;
        private bool _hovered;
        private float _t = 0f;
        private Timer _timer;

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        public NavButton(string icon, string label, Action action)
        {
            _icon = icon; _label = label; _action = action;
            Size = new Size(200, 44);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            _timer = new Timer { Interval = 10 };
            _timer.Tick += (s, e) =>
            {
                float target = _hovered || _isActive ? 1f : 0f;
                _t += (target - _t) * 0.22f;
                if (Math.Abs(_t - target) < 0.01f) { _t = target; _timer.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; _timer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; _timer.Start(); }
        protected override void OnClick(EventArgs e) { base.OnClick(e); _action?.Invoke(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_isActive)
            {
                using var ab = new SolidBrush(AppTheme.SidebarActive);
                g.FillRectangle(ab, 0, 0, Width, Height);
                using var bar = new SolidBrush(Color.White);
                g.FillRectangle(bar, 0, 8, 4, Height - 16);
            }
            else if (_t > 0.01f)
            {
                using var hb = new SolidBrush(Color.FromArgb((int)(55 * _t), 255, 255, 255));
                g.FillRectangle(hb, 0, 0, Width, Height);
            }

            Color tc = _isActive ? Color.White : Color.FromArgb((int)(175 + 80 * _t), 200, 235);
            using var tf = new SolidBrush(tc);
            using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            g.DrawString(_icon, new Font("Segoe UI Emoji", 12), tf, new RectangleF(12, 0, 26, Height), sf);
            g.DrawString(_label, _isActive
                ? new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
                : new Font("Segoe UI", 9.5f),
                tf, new RectangleF(44, 0, Width - 48, Height), sf);
        }

        protected override void Dispose(bool disposing) { _timer?.Dispose(); base.Dispose(disposing); }
    }
}