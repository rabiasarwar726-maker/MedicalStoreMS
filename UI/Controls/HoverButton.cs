// UI/Controls/HoverButton.cs
// A fully custom, hover-animated flat button with rounded corners,
// ripple highlight on press, and smooth colour transitions.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MedicalStoreMS.UI.Controls
{
    public class HoverButton : Control
    {
        // ── Public properties ────────────────────────────────────
        public Color BaseColor   { get; set; } = Color.FromArgb(13, 71, 161);
        public Color HoverColor  { get; set; } = Color.FromArgb(21, 101, 192);
        public Color PressColor  { get; set; } = Color.FromArgb(8,  48, 107);
        public Color TextColor   { get; set; } = Color.White;
        public int   Radius      { get; set; } = 8;
        public bool  ShowShadow  { get; set; } = true;
        public string Icon       { get; set; } = "";          // emoji or text prefix

        // ── State ────────────────────────────────────────────────
        private bool   _hovered, _pressed;
        private float  _hoverAlpha = 0f;          // 0..1 animated
        private Timer  _animTimer;

        public HoverButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Cursor   = Cursors.Hand;
            Size     = new Size(120, 36);
            Font     = new Font("Segoe UI Semibold", 9, FontStyle.Bold);

            _animTimer = new Timer { Interval = 12 };
            _animTimer.Tick += (s, e) =>
            {
                float target = _hovered ? 1f : 0f;
                _hoverAlpha += (target - _hoverAlpha) * 0.25f;
                if (Math.Abs(_hoverAlpha - target) < 0.01f) { _hoverAlpha = target; _animTimer.Stop(); }
                Invalidate();
            };
        }

        // ── Mouse events ─────────────────────────────────────────
        protected override void OnMouseEnter(EventArgs e)
        { base.OnMouseEnter(e); _hovered = true;  _animTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e)
        { base.OnMouseLeave(e); _hovered = false; _animTimer.Start(); }
        protected override void OnMouseDown(MouseEventArgs e)
        { base.OnMouseDown(e); _pressed = true;  Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e)
        { base.OnMouseUp(e);   _pressed = false; Invalidate(); }

        // ── Paint ────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var r = new Rectangle(0, 0, Width - 1, Height - 1);

            // Shadow
            if (ShowShadow && !_pressed)
            {
                using var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
                using var sp = RoundedPath(new Rectangle(2, 4, Width - 3, Height - 3), Radius);
                g.FillPath(shadow, sp);
            }

            // Background — lerp BaseColor → HoverColor
            Color bg = _pressed ? PressColor : Lerp(BaseColor, HoverColor, _hoverAlpha);
            using var bgPath = RoundedPath(r, Radius);
            using var bgBrush = new SolidBrush(bg);
            g.FillPath(bgBrush, bgPath);

            // Subtle top-highlight when hovered
            if (_hoverAlpha > 0.01f)
            {
                var hiRect = new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, r.Height / 2);
                using var hiPath = RoundedPath(hiRect, Radius - 2);
                using var hiBrush = new LinearGradientBrush(hiRect,
                    Color.FromArgb((int)(30 * _hoverAlpha), 255, 255, 255),
                    Color.Transparent,
                    LinearGradientMode.Vertical);
                g.FillPath(hiBrush, hiPath);
            }

            // Text + icon
            string label = string.IsNullOrEmpty(Icon) ? Text : $"{Icon}  {Text}";
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var tf = new SolidBrush(TextColor);
            g.DrawString(label, Font, tf, new RectangleF(0, 0, Width, Height), sf);

            // Press-ripple overlay
            if (_pressed)
            {
                using var ripple = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                g.FillPath(ripple, bgPath);
            }
        }

        // ── Helpers ──────────────────────────────────────────────
        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures();
            return p;
        }

        private static Color Lerp(Color a, Color b, float t)
            => Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));

        protected override void Dispose(bool disposing)
        { _animTimer?.Dispose(); base.Dispose(disposing); }
    }
}
