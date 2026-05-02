// DataAccess/InvoiceRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Models;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.DataAccess
{
    public class InvoiceRepository
    {
        private readonly MedicineRepository _medRepo = new MedicineRepository();

        // ── Save invoice + details in a transaction ──────────────
        public int Save(Invoice inv)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var tx   = conn.BeginTransaction();
            try
            {
                // Insert header
                var cmd = new SqlCommand(@"
                    INSERT INTO Invoices(InvoiceDate,CustomerID,TotalAmount,Discount,NetAmount,PaymentMode,CreatedByUserID,Notes)
                    OUTPUT INSERTED.InvoiceID
                    VALUES(@d,@cid,@tot,@disc,@net,@pm,@uid,@n)", conn, tx);

                cmd.Parameters.AddWithValue("@d",    inv.InvoiceDate);
                cmd.Parameters.AddWithValue("@cid",  (object)inv.CustomerID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tot",  inv.TotalAmount);
                cmd.Parameters.AddWithValue("@disc", inv.Discount);
                cmd.Parameters.AddWithValue("@net",  inv.NetAmount);
                cmd.Parameters.AddWithValue("@pm",   inv.PaymentMode ?? "Cash");
                cmd.Parameters.AddWithValue("@uid",  inv.CreatedByUserID);
                cmd.Parameters.AddWithValue("@n",    (object)inv.Notes ?? DBNull.Value);

                int invoiceId = (int)cmd.ExecuteScalar();

                // Insert details + deduct stock
                foreach (var d in inv.Details)
                {
                    var dcmd = new SqlCommand(@"
                        INSERT INTO InvoiceDetails(InvoiceID,MedicineID,Quantity,UnitPrice,SubTotal)
                        VALUES(@iid,@mid,@q,@p,@st)", conn, tx);
                    dcmd.Parameters.AddWithValue("@iid", invoiceId);
                    dcmd.Parameters.AddWithValue("@mid", d.MedicineID);
                    dcmd.Parameters.AddWithValue("@q",   d.Quantity);
                    dcmd.Parameters.AddWithValue("@p",   d.UnitPrice);
                    dcmd.Parameters.AddWithValue("@st",  d.SubTotal);
                    dcmd.ExecuteNonQuery();

                    // Deduct stock
                    _medRepo.UpdateStock(d.MedicineID, -d.Quantity, conn, tx);
                }

                tx.Commit();
                return invoiceId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Invoice> GetAll(DateTime? from = null, DateTime? to = null)
        {
            var sql = @"
                SELECT i.*, c.CustomerName
                FROM   Invoices i
                LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                WHERE 1=1";
            var parms = new List<SqlParameter>();
            if (from.HasValue) { sql += " AND i.InvoiceDate >= @f"; parms.Add(new SqlParameter("@f", from.Value)); }
            if (to.HasValue)   { sql += " AND i.InvoiceDate <= @t"; parms.Add(new SqlParameter("@t", to.Value.AddDays(1))); }
            sql += " ORDER BY i.InvoiceDate DESC";

            var dt   = DatabaseHelper.ExecuteQuery(sql, parms.ToArray());
            var list = new List<Invoice>();
            foreach (DataRow r in dt.Rows) list.Add(MapHeader(r));
            return list;
        }

        public Invoice GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT i.*, c.CustomerName
                FROM   Invoices i
                LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                WHERE  i.InvoiceID = @id",
                new SqlParameter("@id", id));

            if (dt.Rows.Count == 0) return null;
            var inv = MapHeader(dt.Rows[0]);

            var ddt = DatabaseHelper.ExecuteQuery(@"
                SELECT d.*, m.MedicineName
                FROM   InvoiceDetails d
                JOIN   Medicines m ON d.MedicineID = m.MedicineID
                WHERE  d.InvoiceID = @id",
                new SqlParameter("@id", id));

            foreach (DataRow r in ddt.Rows)
                inv.Details.Add(new InvoiceDetail
                {
                    DetailID     = (int)r["DetailID"],
                    InvoiceID    = id,
                    MedicineID   = (int)r["MedicineID"],
                    MedicineName = r["MedicineName"].ToString(),
                    Quantity     = (int)r["Quantity"],
                    UnitPrice    = (decimal)r["UnitPrice"],
                    SubTotal     = (decimal)r["SubTotal"]
                });

            return inv;
        }

        public decimal GetTodaySales()
            => DatabaseHelper.ExecuteScalar<decimal>(
                "SELECT ISNULL(SUM(NetAmount),0) FROM Invoices WHERE CAST(InvoiceDate AS DATE)=CAST(GETDATE() AS DATE)",
                0);

        public decimal GetMonthSales()
            => DatabaseHelper.ExecuteScalar<decimal>(
                "SELECT ISNULL(SUM(NetAmount),0) FROM Invoices WHERE MONTH(InvoiceDate)=MONTH(GETDATE()) AND YEAR(InvoiceDate)=YEAR(GETDATE())",
                0);

        private static Invoice MapHeader(DataRow r) => new Invoice
        {
            InvoiceID   = (int)r["InvoiceID"],
            InvoiceDate = Convert.ToDateTime(r["InvoiceDate"]),
            CustomerID  = r["CustomerID"] == DBNull.Value ? (int?)null : (int)r["CustomerID"],
            CustomerName= r["CustomerName"]?.ToString(),
            TotalAmount = (decimal)r["TotalAmount"],
            Discount    = (decimal)r["Discount"],
            NetAmount   = (decimal)r["NetAmount"],
            PaymentMode = r["PaymentMode"].ToString(),
            CreatedByUserID = (int)r["CreatedByUserID"],
            Notes       = r["Notes"]?.ToString()
        };
    }
}
