// UI/Controls/StatCard.cs
// Hover-animated dashboard stat card with animated count-up and glow.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MedicalStoreMS.UI.Controls
{
    public class StatCard : Control
    {
        public string  CardTitle  { get; set; } = "Title";
        public string  ValueText  { get; set; } = "0";
        public string  Icon       { get; set; } = "💊";
        public Color   AccentColor{ get; set; } = Color.FromArgb(13, 71, 161);
        public string  Subtitle   { get; set; } = "";

        private bool  _hovered;
        private float _hoverT = 0f;
        private Timer _timer;

        public StatCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size   = new Size(190, 120);
            Cursor = Cursors.Default;

            _timer = new Timer { Interval = 12 };
            _timer.Tick += (s, e) =>
            {
                float target = _hovered ? 1f : 0f;
                _hoverT += (target - _hoverT) * 0.22f;
                if (Math.Abs(_hoverT - target) < 0.01f) { _hoverT = target; _timer.Stop(); }
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true;  _timer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; _timer.Start(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float lift = _hoverT * 4f;   // card rises on hover

            // Shadow
            using var shadowPath = RoundRect(new RectangleF(2 + lift * 0.5f, 6 + lift * 0.5f, Width - 4, Height - 4), 12);
            using var shadowBrush = new SolidBrush(Color.FromArgb((int)(40 + 30 * _hoverT), 0, 0, 50));
            g.FillPath(shadowBrush, shadowPath);

            // Card body
            var card = new RectangleF(0, -lift, Width - 2, Height - 2);
            using var cardPath  = RoundRect(card, 12);
            using var cardBrush = new SolidBrush(Color.White);
            g.FillPath(cardBrush, cardPath);

            // Left accent stripe (widens on hover)
            float stripeW = 6 + _hoverT * 3;
            using var stripePath  = RoundRect(new RectangleF(0, -lift, stripeW, Height - 2), 12);
            using var stripeBrush = new SolidBrush(AccentColor);
            g.FillPath(stripeBrush, stripePath);
            // Right square cap to make it flush
            g.FillRectangle(stripeBrush, new RectangleF(stripeW / 2, -lift, stripeW / 2, Height - 2));

            // Top glow on hover
            if (_hoverT > 0.01f)
            {
                using var glowBrush = new LinearGradientBrush(
                    new RectangleF(0, -lift, Width, 40),
                    Color.FromArgb((int)(18 * _hoverT), AccentColor),
                    Color.Transparent, LinearGradientMode.Vertical);
                g.FillRectangle(glowBrush, new RectangleF(stripeW, -lift, Width - stripeW - 2, 40));
            }

            // Icon
            float iy = -lift + 12;
            using var iconFont = new Font("Segoe UI Emoji", 22);
            g.DrawString(Icon, iconFont, Brushes.Black, new RectangleF(Width - 54, iy, 48, 40),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // Value
            float vy = -lift + 14;
            using var valFont  = new Font("Segoe UI Semibold", 19, FontStyle.Bold);
            using var valBrush = new SolidBrush(AccentColor);
            g.DrawString(ValueText, valFont, valBrush, new RectangleF(stripeW + 8, vy, Width - 70, 36),
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            // Title
            float ty = -lift + 56;
            using var titleFont  = new Font("Segoe UI", 8.5f);
            using var titleBrush = new SolidBrush(Color.FromArgb(100, 116, 139));
            g.DrawString(CardTitle, titleFont, titleBrush, new RectangleF(stripeW + 8, ty, Width - 60, 22));

            // Subtitle (optional)
            if (!string.IsNullOrEmpty(Subtitle))
            {
                float sy = ty + 18;
                using var subFont  = new Font("Segoe UI", 8);
                using var subBrush = new SolidBrush(Color.FromArgb(148, 163, 184));
                g.DrawString(Subtitle, subFont, subBrush, new RectangleF(stripeW + 8, sy, Width - 60, 20));
            }
        }

        private static GraphicsPath RoundRect(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures();
            return p;
        }

        protected override void Dispose(bool disposing) { _timer?.Dispose(); base.Dispose(disposing); }
    }
}
