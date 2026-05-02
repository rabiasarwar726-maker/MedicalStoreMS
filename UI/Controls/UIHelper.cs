// UI/Controls/UIHelper.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.UI.Controls
{
    public static class UIHelper
    {
        public static Label MakePageTitle(string text, int x, int y) => new Label
        {
            Text = text, Font = AppTheme.FontH1, ForeColor = AppTheme.TextPrimary,
            AutoSize = true, Location = new Point(x, y)
        };

        public static HoverButton MakeButton(string text, Color baseColor, Point location,
            Color? hoverColor = null, string icon = "")
        {
            return new HoverButton
            {
                Text = text, Icon = icon,
                BaseColor  = baseColor,
                HoverColor = hoverColor ?? Lighten(baseColor, 22),
                PressColor = Darken(baseColor, 22),
                Location   = location,
                Size       = new Size(118, 34)
            };
        }

        public static TextBox MakeSearchBox(Point loc, string placeholder, int width = 300)
        {
            var tb = new TextBox
            {
                Location = loc, Size = new Size(width, 32),
                Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White, ForeColor = AppTheme.TextPrimary,
                PlaceholderText = placeholder
            };
            tb.Enter += (s, e) => tb.BackColor = Color.FromArgb(240, 248, 255);
            tb.Leave += (s, e) => tb.BackColor = Color.White;
            return tb;
        }

        public static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(230, 235, 245),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 34 },
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };

            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.Primary, ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
                Padding = new Padding(8, 0, 8, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            g.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = AppTheme.FontBody, ForeColor = AppTheme.TextPrimary,
                BackColor = Color.White,
                SelectionBackColor = AppTheme.PrimaryLight, SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 8, 0)
            };
            g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(246, 249, 254),
                SelectionBackColor = AppTheme.PrimaryLight, SelectionForeColor = Color.White
            };

            // Row hover highlight
            g.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (!g.Rows[e.RowIndex].Selected)
                    foreach (DataGridViewCell c in g.Rows[e.RowIndex].Cells)
                        c.Style.BackColor = Color.FromArgb(232, 240, 255);
            };
            g.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (!g.Rows[e.RowIndex].Selected)
                    foreach (DataGridViewCell c in g.Rows[e.RowIndex].Cells)
                        c.Style.BackColor = e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(246, 249, 254);
            };

            return g;
        }

        public static DataGridViewTextBoxColumn Col(string name, string header, int width) =>
            new DataGridViewTextBoxColumn
            {
                Name = name, HeaderText = header, Width = width,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

        public static Panel MakeCard(int x, int y, int w, int h)
        {
            var card = new Panel
            {
                BackColor = Color.White, BorderStyle = BorderStyle.None,
                Bounds = new Rectangle(x, y, w, h), Padding = new Padding(16)
            };
            card.Paint += (s, e) =>
            {
                var gr = e.Graphics;
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                using var bp = RoundRect(new Rectangle(0, 0, card.Width - 2, card.Height - 2), 10);
                gr.FillPath(Brushes.White, bp);
                using var pen = new Pen(Color.FromArgb(213, 220, 232), 1);
                gr.DrawPath(pen, bp);
            };
            return card;
        }

        public static Color Lighten(Color c, int a) => Color.FromArgb(c.A,
            Math.Min(255, c.R + a), Math.Min(255, c.G + a), Math.Min(255, c.B + a));
        public static Color Darken(Color c, int a) => Color.FromArgb(c.A,
            Math.Max(0, c.R - a), Math.Max(0, c.G - a), Math.Max(0, c.B - a));

        public static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var p = new GraphicsPath(); int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseAllFigures(); return p;
        }
    }
}
