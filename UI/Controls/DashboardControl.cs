// UI/Controls/DashboardControl.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using MedicalStoreMS.BusinessLogic;
using MedicalStoreMS.Models;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.UI.Controls
{
    public class DashboardControl : UserControl
    {
        private readonly DashboardService _svc = new DashboardService();

        public DashboardControl()
        {
            BackColor = AppTheme.Background;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Load += (s, e) => LoadData();
        }

        private void LoadData()
        {
            try { BuildUI(_svc.GetStats()); }
            catch (Exception ex)
            {
                Controls.Add(new Label
                {
                    Text = $"Dashboard error:\n{ex.Message}\n\nCheck database connection.",
                    Font = AppTheme.FontBody,
                    ForeColor = AppTheme.Danger,
                    AutoSize = true,
                    Location = new Point(20, 20)
                });
            }
        }

        private void BuildUI(DashboardStats s)
        {
            Controls.Clear();

            // ── Page title ───────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            Controls.Add(new Label
            {
                Text = $"Good {Greet()}, {Session.CurrentUser?.FullName}  ·  {DateTime.Now:dddd, dd MMMM yyyy}",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 34)
            });

            // ── Stat cards ───────────────────────────────────────
            var cards = new (string title, string val, string icon, Color accent, string sub)[]
            {
                ("Total Medicines",  s.TotalMedicines.ToString(),  "💊", AppTheme.Primary,  "Active items"),
                ("Today's Sales",    $"Rs {s.TodaySales:N0}",      "💰", AppTheme.Success,  DateTime.Now.ToString("dd MMM")),
                ("Month Sales",      $"Rs {s.MonthSales:N0}",      "📈", AppTheme.Accent,   DateTime.Now.ToString("MMMM")),
                ("Low Stock",        s.LowStockCount.ToString(),   "⚠",  AppTheme.Warning,  "Need restock"),
                ("Expired",          s.ExpiredCount.ToString(),    "🚫", AppTheme.Danger,   "Remove now"),
                ("Near Expiry",      s.NearExpiryCount.ToString(), "⏰", Color.FromArgb(130,40,150), "30 days"),
                ("Suppliers",        s.TotalSuppliers.ToString(),  "🏢", AppTheme.Info,     "Active"),
            };

            int cw = 185, ch = 115, gap = 12;
            int cx = 0, cy = 62;

            foreach (var (title, val, icon, accent, sub) in cards)
            {
                var card = MakeStatCard(title, val, icon, accent, sub, cx, cy, cw, ch);
                Controls.Add(card);
                cx += cw + gap;
                if (cx + cw > 1100) { cx = 0; cy += ch + gap; }
            }

            int py = cy + ch + 20;

            // ── Alert ────────────────────────────────────────────
            if (s.LowStockCount > 0 || s.ExpiredCount > 0)
            {
                var ap = new Panel
                {
                    BackColor = Color.FromArgb(255, 243, 205),
                    Bounds = new Rectangle(0, py, 900, 44),
                    Padding = new Padding(14, 0, 14, 0)
                };
                ap.Controls.Add(new Label
                {
                    Text = $"⚠  {s.LowStockCount} low stock  ·  {s.ExpiredCount} expired  ·  {s.NearExpiryCount} near expiry (30d)",
                    Font = AppTheme.FontBody,
                    ForeColor = Color.FromArgb(130, 60, 0),
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                });
                Controls.Add(ap);
                py += 56;
            }

            // ── Quick Actions ────────────────────────────────────
            Controls.Add(new Label
            {
                Text = "Quick Actions",
                Font = AppTheme.FontH2,
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, py)
            });
            py += 30;

            var actions = new (string label, Color color)[]
            {
                ("New Sale",        AppTheme.Success),
                ("Add Medicine",    AppTheme.Primary),
                ("Record Purchase", AppTheme.Info),
                ("View Reports",    AppTheme.Accent),
            };

            int bx = 0;
            foreach (var (label, color) in actions)
            {
                var btn = new Button
                {
                    Text = label,
                    BackColor = color,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                    Size = new Size(148, 40),
                    Location = new Point(bx, py),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                string lbl = label;
                btn.Click += (s, e) => FireQuickAction(lbl);

                Controls.Add(btn);
                bx += 158;
            }
        }

        private void FireQuickAction(string action)
        {
            var mainForm = FindForm();
            if (mainForm == null) return;

            string navTarget = action switch
            {
                "New Sale" => "Billing",
                "Add Medicine" => "Medicines",
                "Record Purchase" => "Purchases",
                "View Reports" => "Reports",
                _ => ""
            };

            if (string.IsNullOrEmpty(navTarget)) return;

            foreach (Control c in mainForm.Controls)
            {
                if (c is Panel panel && panel.BackColor == AppTheme.Sidebar)
                {
                    foreach (Control nc in panel.Controls)
                    {
                        var field = nc.GetType().GetField("_label",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        if (field?.GetValue(nc)?.ToString() == navTarget)
                        {
                            var method = nc.GetType().GetMethod("OnClick",
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
                            method?.Invoke(nc, new object[] { EventArgs.Empty });
                            return;
                        }
                    }
                }
            }
        }

        private static Panel MakeStatCard(string title, string val, string icon,
            Color accent, string sub, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                BackColor = Color.White,
                Bounds = new Rectangle(x, y, w, h),
                Cursor = Cursors.Default
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var accentBrush = new SolidBrush(accent);
                g.FillRectangle(accentBrush, 0, 0, 5, h);
                using var borderPen = new Pen(Color.FromArgb(220, 226, 235), 1);
                g.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);
            };

            card.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 20),
                AutoSize = true,
                Location = new Point(w - 46, 10),
                BackColor = Color.Transparent
            });

            card.Controls.Add(new Label
            {
                Text = val,
                Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(12, 16),
                BackColor = Color.Transparent
            });

            card.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextSecondary,
                AutoSize = false,
                Size = new Size(w - 20, 20),
                Location = new Point(12, 62),
                BackColor = Color.Transparent
            });

            card.Controls.Add(new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(12, 82),
                BackColor = Color.Transparent
            });

            return card;
        }

        private static string Greet()
        {
            int h = DateTime.Now.Hour;
            return h < 12 ? "morning" : h < 17 ? "afternoon" : "evening";
        }
    }
}