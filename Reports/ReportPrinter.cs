// Reports/ReportPrinter.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using MedicalStoreMS.Models;
using MedicalStoreMS.UI.Themes;

namespace MedicalStoreMS.Reports
{
    /// <summary>
    /// Generates a printable invoice using Windows PrintDocument.
    /// No third-party library required — works with the built-in print dialog.
    /// </summary>
    public static class ReportPrinter
    {
        // ── Print Invoice ────────────────────────────────────────
        public static void PrintInvoice(Invoice inv)
        {
            var doc = new PrintDocument();
            doc.PrintPage += (s, e) => DrawInvoice(e.Graphics, inv, e.MarginBounds);

            using var dlg = new PrintPreviewDialog
            {
                Document  = doc,
                Text      = $"Invoice #{inv.InvoiceID}",
                WindowState = FormWindowState.Maximized
            };
            dlg.ShowDialog();
        }

        private static void DrawInvoice(Graphics g, Invoice inv, Rectangle bounds)
        {
            g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int x = bounds.Left, y = bounds.Top, w = bounds.Width;

            // Header stripe
            g.FillRectangle(new SolidBrush(AppTheme.Primary), x, y, w, 60);
            g.DrawString("MediCare Store Management", new Font("Segoe UI", 18, FontStyle.Bold), Brushes.White, x + 12, y + 8);
            g.DrawString("Tax Invoice", new Font("Segoe UI", 11), new SolidBrush(Color.FromArgb(180, 220, 255)), x + 14, y + 36);
            y += 75;

            // Invoice meta
            var metaFont = new Font("Segoe UI", 9);
            g.DrawString($"Invoice #:   {inv.InvoiceID}",           metaFont, Brushes.Black, x, y);
            g.DrawString($"Date:        {inv.InvoiceDate:dd/MM/yyyy HH:mm}", metaFont, Brushes.Black, x, y + 18);
            g.DrawString($"Customer:    {inv.CustomerName ?? "Walk-in"}",     metaFont, Brushes.Black, x, y + 36);
            g.DrawString($"Payment:     {inv.PaymentMode}",          metaFont, Brushes.Black, x + 280, y);
            y += 68;

            // Table header
            g.FillRectangle(new SolidBrush(AppTheme.Primary), x, y, w, 24);
            var hFont = new Font("Segoe UI", 9, FontStyle.Bold);
            g.DrawString("Medicine",   hFont, Brushes.White, x + 4,   y + 4);
            g.DrawString("Qty",        hFont, Brushes.White, x + 260,  y + 4);
            g.DrawString("Unit Price", hFont, Brushes.White, x + 310, y + 4);
            g.DrawString("Sub Total",  hFont, Brushes.White, x + 410, y + 4);
            y += 28;

            var rowFont = new Font("Segoe UI", 9);
            bool alt = false;
            foreach (var d in inv.Details)
            {
                if (alt) g.FillRectangle(new SolidBrush(Color.FromArgb(245, 248, 255)), x, y, w, 22);
                g.DrawString(d.MedicineName,              rowFont, Brushes.Black, x + 4,   y + 3);
                g.DrawString(d.Quantity.ToString(),       rowFont, Brushes.Black, x + 260,  y + 3);
                g.DrawString($"Rs {d.UnitPrice:N2}",     rowFont, Brushes.Black, x + 310, y + 3);
                g.DrawString($"Rs {d.SubTotal:N2}",      rowFont, Brushes.Black, x + 410, y + 3);
                y += 22; alt = !alt;
            }

            // Divider
            y += 6;
            g.DrawLine(new Pen(AppTheme.Border, 1), x, y, x + w, y);
            y += 10;

            // Totals
            var totFont = new Font("Segoe UI", 10, FontStyle.Bold);
            g.DrawString($"Total Amount:   Rs {inv.TotalAmount:N2}", rowFont, Brushes.Black, x + 310, y);   y += 20;
            g.DrawString($"Discount:       Rs {inv.Discount:N2}",    rowFont, Brushes.Black, x + 310, y);   y += 20;
            g.FillRectangle(new SolidBrush(AppTheme.Accent), x + 300, y, w - 300, 26);
            g.DrawString($"Net Amount:  Rs {inv.NetAmount:N2}",      totFont, Brushes.White, x + 310, y + 4); y += 40;

            // Footer
            g.DrawLine(new Pen(AppTheme.Border), x, y, x + w, y); y += 10;
            g.DrawString("Thank you for your purchase! — MediCare Store Management System",
                new Font("Segoe UI", 8, FontStyle.Italic), new SolidBrush(AppTheme.TextSecondary), x, y);
        }

        // ── Print Inventory ──────────────────────────────────────
        public static void PrintInventory(List<Medicine> medicines)
        {
            int page = 0;
            var doc = new PrintDocument();
            doc.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                var bounds = e.MarginBounds;
                int x = bounds.Left, y = bounds.Top, w = bounds.Width;

                // Header
                g.FillRectangle(new SolidBrush(AppTheme.Primary), x, y, w, 50);
                g.DrawString("MediCare — Inventory Report", new Font("Segoe UI", 16, FontStyle.Bold), Brushes.White, x + 12, y + 6);
                g.DrawString($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", new Font("Segoe UI", 9), new SolidBrush(Color.FromArgb(180, 220, 255)), x + 14, y + 30);
                y += 64;

                // Column headers
                g.FillRectangle(new SolidBrush(AppTheme.PrimaryLight), x, y, w, 22);
                var hf = new Font("Segoe UI", 8, FontStyle.Bold);
                g.DrawString("Medicine",   hf, Brushes.White, x + 4,  y + 4);
                g.DrawString("Batch",      hf, Brushes.White, x + 180, y + 4);
                g.DrawString("Qty",        hf, Brushes.White, x + 270, y + 4);
                g.DrawString("Price",      hf, Brushes.White, x + 320, y + 4);
                g.DrawString("Expiry",     hf, Brushes.White, x + 400, y + 4);
                y += 26;

                var rf  = new Font("Segoe UI", 8);
                bool alt = false;
                foreach (var m in medicines)
                {
                    if (y > bounds.Bottom - 30) { e.HasMorePages = true; break; }
                    if (alt) g.FillRectangle(new SolidBrush(Color.FromArgb(245,248,255)), x, y, w, 20);
                    var fc = m.IsExpired ? AppTheme.Danger : m.IsNearExpiry ? AppTheme.Warning : Color.Black;
                    g.DrawString(m.MedicineName, rf, new SolidBrush(fc), x + 4,  y + 3);
                    g.DrawString(m.BatchNo ?? "-", rf, Brushes.Black,    x + 180, y + 3);
                    g.DrawString(m.Quantity.ToString(), rf, m.IsLowStock ? new SolidBrush(AppTheme.Warning) : Brushes.Black, x + 270, y + 3);
                    g.DrawString($"Rs {m.UnitPrice:N2}", rf, Brushes.Black, x + 320, y + 3);
                    g.DrawString(m.ExpiryDate.ToString("dd/MM/yy"), rf, new SolidBrush(fc), x + 400, y + 3);
                    y += 20; alt = !alt;
                }
            };

            using var dlg = new PrintPreviewDialog { Document = doc, Text = "Inventory Report", WindowState = FormWindowState.Maximized };
            dlg.ShowDialog();
        }
    }
}
