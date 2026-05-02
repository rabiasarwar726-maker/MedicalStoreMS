// UI/Forms/LoginForm.cs  — Animated login with hover effects
using MedicalStoreMS.Utils; // ✅ REQUIRED
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MedicalStoreMS.BusinessLogic;
using MedicalStoreMS.UI.Controls;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        private Panel _leftPanel, _rightPanel;
        private TextBox _txtUsername, _txtPassword;
        private HoverButton _btnLogin;
        private Label _lblError;
        private CheckBox _chkShow;
        private Timer _pulseTimer;
        private float _pulse = 0f;

        public LoginForm()
        {
            MessageBox.Show("LoginForm started");// ✅ ADD THIS

            Text = "MediCare — Secure Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(1000, 650);
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;
            Font = AppTheme.FontBody;

            BuildRight();
            BuildLeft();
            StartPulse();

            this.Load += (s, e) => this.BringToFront();
        }

        // ── Left branded panel ────────────────────────────────────
        private void BuildLeft()
        {
            _leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = AppTheme.Primary
            };
            _leftPanel.Paint += LeftPanel_Paint;

            var brand = new Label
            {
                Text = "MediCare",
                Font = new Font("Segoe UI Semibold", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(44, 200)   // ✅ moved down from 190
            };
            var tag = new Label
            {
                Text = "Clinical Precision\nManagement System",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(170, 210, 245),
                AutoSize = true,
                Location = new Point(44, 280)   // ✅ moved down from 260 — more gap after brand
            };

            var features = new string[] {
                "💊  Medicine Inventory & Tracking",
                "🧾  Billing & Invoice Generation",
                "📦  Purchase & Supplier Management",
                "⏰  Expiry Date Alerts",
                "📈  Reports & Analytics",
                "🔒  Role-Based Access Control"
            };

            int fy = 380;                         // ✅ moved down from 350
            foreach (var f in features)
            {
                _leftPanel.Controls.Add(new Label
                {
                    Text = f,
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(195, 225, 255),
                    AutoSize = true,
                    Location = new Point(44, fy)
                });
                fy += 28;
            }

            _leftPanel.Controls.AddRange(new Control[] { brand, tag });
            Controls.Add(_leftPanel);
        }

        private void LeftPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float p = (float)Math.Sin(_pulse) * 0.5f + 0.5f;

            using var c1 = new SolidBrush(Color.FromArgb((int)(25 + 15 * p), 255, 255, 255));
            g.FillEllipse(c1, -60, -60, 220, 220);

            using var c2 = new SolidBrush(Color.FromArgb((int)(15 + 10 * p), 255, 255, 255));
            g.FillEllipse(c2, 250, 400, 200, 200);

            using var c3 = new SolidBrush(Color.FromArgb((int)(10 + 8 * p), 255, 255, 255));
            g.FillEllipse(c3, 300, -40, 160, 160);

            // ✅ Plus icon box
            int ix = 44, iy = 110;
            using var iconPath = RoundPath(new Rectangle(ix, iy, 70, 70), 18);
            using var iconBrush = new SolidBrush(Color.FromArgb(25, 101, 192));
            g.FillPath(iconBrush, iconPath);
            using var crossPen = new Pen(Color.White, 5) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(crossPen, ix + 35, iy + 18, ix + 35, iy + 52);
            g.DrawLine(crossPen, ix + 18, iy + 35, ix + 52, iy + 35);
        }

        // ── Right login panel ─────────────────────────────────────
        private void BuildRight()
        {
            _rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Inner panel — centred dynamically
            var inner = new Panel
            {
                BackColor = Color.Transparent,
                Size = new Size(480, 430)
            };

            // ✅ Welcome title
            var welcome = new Label
            {
                Text = "Welcome Back 👋",
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // ✅ Subtitle — sits below welcome with enough gap
            var sub = new Label
            {
                Text = "Sign in to your MediCare dashboard",
                Font = AppTheme.FontBody,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 52)     // ✅ 52px below top — clears the large title font
            };

            // ✅ USERNAME field — starts at y=106 giving sub room
            var ulbl = new Label
            {
                Text = "USERNAME",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 106)
            };
            _txtUsername = new TextBox
            {
                Location = new Point(0, 126),
                Size = new Size(480, 36),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 253),
                ForeColor = AppTheme.TextPrimary
            };
            _txtUsername.PlaceholderText = "Enter your username";
            _txtUsername.Enter += (s, e) => _txtUsername.BackColor = Color.FromArgb(236, 246, 255);
            _txtUsername.Leave += (s, e) => _txtUsername.BackColor = Color.FromArgb(248, 250, 253);

            // ✅ PASSWORD field
            var plbl = new Label
            {
                Text = "PASSWORD",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 186)
            };
            _txtPassword = new TextBox
            {
                Location = new Point(0, 206),
                Size = new Size(480, 36),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 253),
                ForeColor = AppTheme.TextPrimary,
                UseSystemPasswordChar = true
            };
            _txtPassword.PlaceholderText = "Enter your password";
            _txtPassword.Enter += (s, e) => _txtPassword.BackColor = Color.FromArgb(236, 246, 255);
            _txtPassword.Leave += (s, e) => _txtPassword.BackColor = Color.FromArgb(248, 250, 253);

            
            var forgot = new Label
            {
                Text = "Forgot password?",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.PrimaryLight,
                AutoSize = true,
                Location = new Point(300, 188), // ✅ back to 188 — aligns with PASSWORD label
                Cursor = Cursors.Hand
            };
            forgot.MouseEnter += (s, e) => forgot.ForeColor = AppTheme.Primary;
            forgot.MouseLeave += (s, e) => forgot.ForeColor = AppTheme.PrimaryLight;

            // ✅ Show password checkbox
            _chkShow = new CheckBox
            {
                Text = "Show password",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Location = new Point(0, 258),
                Cursor = Cursors.Hand
            };
            _chkShow.CheckedChanged += (s, e) =>
                _txtPassword.UseSystemPasswordChar = !_chkShow.Checked;

            // ✅ Error label
            _lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.Danger,
                AutoSize = false,
                Size = new Size(480, 22),
                Location = new Point(0, 284)
            };

            // ✅ Sign in button
            _btnLogin = new HoverButton
            {
                Text = "SIGN IN SECURELY",
                Icon = "🔐",
                BaseColor = AppTheme.Primary,
                HoverColor = AppTheme.PrimaryLight,
                PressColor = AppTheme.PrimaryDark,
                Location = new Point(0, 312),
                Size = new Size(480, 50),
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Radius = 10,
                ShowShadow = true
            };
            _btnLogin.Click += BtnLogin_Click;

            var divider = new Panel
            {
                BackColor = AppTheme.Border,
                Size = new Size(480, 1),
                Location = new Point(0, 376)
            };

            var secure = new Label
            {
                Text = "🔒  Authenticated Enterprise Session — Data Encrypted",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(0, 384)
            };

            inner.Controls.AddRange(new Control[]
            {
                welcome, sub,
                ulbl, _txtUsername,
                plbl, _txtPassword, forgot,
                _chkShow, _lblError, _btnLogin,
                divider, secure
            });

            _rightPanel.Controls.Add(inner);

            // ✅ Keep inner panel perfectly centred on resize
            _rightPanel.Resize += (s, e) =>
            {
                inner.Location = new Point(
                    Math.Max(0, (_rightPanel.Width - inner.Width) / 2),
                    Math.Max(0, (_rightPanel.Height - inner.Height) / 2));
            };

            Controls.Add(_rightPanel);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            _lblError.Text = "";
            _btnLogin.Enabled = false;
            _btnLogin.Text = "Signing in…";
            Application.DoEvents();

            try
            {
                var (ok, msg) = _auth.Login(_txtUsername.Text.Trim(), _txtPassword.Text);
                if (ok)
                {
                    var main = new MainForm();
                    main.Show();
                    Hide();
                    main.FormClosed += (_, __) => Close();
                }
                else
                {
                    _lblError.Text = "⚠  " + msg;
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                }
            }
            finally
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "SIGN IN SECURELY";
            }
        }

        private void StartPulse()
        {
            _pulseTimer = new Timer { Interval = 30 };
            _pulseTimer.Tick += (s, e) => { _pulse += 0.04f; _leftPanel?.Invalidate(); };
            _pulseTimer.Start();
        }

        private static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            var p = new GraphicsPath(); int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures();
            return p;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _pulseTimer?.Stop();
            base.OnFormClosed(e);
        }
    }
}
