// DataAccess/PurchaseRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Models;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.DataAccess
{
    public class PurchaseRepository
    {
        private readonly MedicineRepository _medRepo = new MedicineRepository();

        public int Save(Purchase p)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var tx   = conn.BeginTransaction();
            try
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO Purchases(PurchaseDate,SupplierID,TotalAmount,CreatedByUserID,Notes)
                    OUTPUT INSERTED.PurchaseID
                    VALUES(@d,@sid,@tot,@uid,@n)", conn, tx);
                cmd.Parameters.AddWithValue("@d",   p.PurchaseDate);
                cmd.Parameters.AddWithValue("@sid", (object)p.SupplierID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tot", p.TotalAmount);
                cmd.Parameters.AddWithValue("@uid", p.CreatedByUserID);
                cmd.Parameters.AddWithValue("@n",   (object)p.Notes ?? DBNull.Value);
                int purchaseId = (int)cmd.ExecuteScalar();

                foreach (var d in p.Details)
                {
                    var dc = new SqlCommand(@"
                        INSERT INTO PurchaseDetails(PurchaseID,MedicineID,Quantity,UnitCost,SubTotal)
                        VALUES(@pid,@mid,@q,@c,@st)", conn, tx);
                    dc.Parameters.AddWithValue("@pid", purchaseId);
                    dc.Parameters.AddWithValue("@mid", d.MedicineID);
                    dc.Parameters.AddWithValue("@q",   d.Quantity);
                    dc.Parameters.AddWithValue("@c",   d.UnitCost);
                    dc.Parameters.AddWithValue("@st",  d.SubTotal);
                    dc.ExecuteNonQuery();

                    // Increase stock
                    _medRepo.UpdateStock(d.MedicineID, d.Quantity, conn, tx);
                }

                tx.Commit();
                return purchaseId;
            }
            catch { tx.Rollback(); throw; }
        }

        public List<Purchase> GetAll(DateTime? from = null, DateTime? to = null)
        {
            var sql = @"
                SELECT p.*, s.SupplierName
                FROM   Purchases p
                LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                WHERE 1=1";
            var parms = new List<SqlParameter>();
            if (from.HasValue) { sql += " AND p.PurchaseDate >= @f"; parms.Add(new SqlParameter("@f", from.Value)); }
            if (to.HasValue)   { sql += " AND p.PurchaseDate <= @t"; parms.Add(new SqlParameter("@t", to.Value.AddDays(1))); }
            sql += " ORDER BY p.PurchaseDate DESC";

            var dt   = DatabaseHelper.ExecuteQuery(sql, parms.ToArray());
            var list = new List<Purchase>();
            foreach (DataRow r in dt.Rows)
                list.Add(new Purchase
                {
                    PurchaseID   = (int)r["PurchaseID"],
                    PurchaseDate = Convert.ToDateTime(r["PurchaseDate"]),
                    SupplierID   = r["SupplierID"] == DBNull.Value ? (int?)null : (int)r["SupplierID"],
                    SupplierName = r["SupplierName"]?.ToString(),
                    TotalAmount  = (decimal)r["TotalAmount"],
                    Notes        = r["Notes"]?.ToString()
                });
            return list;
        }
    }
}

// ─────────────────────────────────────────────────────────────
// DataAccess/SupplierRepository.cs
// ─────────────────────────────────────────────────────────────
namespace MedicalStoreMS.DataAccess
{
    using MedicalStoreMS.Models;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class SupplierRepository
    {
        public List<Supplier> GetAll()
        {
            var dt   = DatabaseHelper.ExecuteQuery("SELECT * FROM Suppliers WHERE IsActive=1 ORDER BY SupplierName");
            var list = new List<Supplier>();
            foreach (DataRow r in dt.Rows) list.Add(Map(r));
            return list;
        }

        public bool Add(Supplier s)
            => DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Suppliers(SupplierName,ContactNo,Address,Email) VALUES(@n,@c,@a,@e)",
                new SqlParameter("@n", s.SupplierName),
                new SqlParameter("@c", (object)s.ContactNo ?? System.DBNull.Value),
                new SqlParameter("@a", (object)s.Address   ?? System.DBNull.Value),
                new SqlParameter("@e", (object)s.Email     ?? System.DBNull.Value)) > 0;

        public bool Update(Supplier s)
            => DatabaseHelper.ExecuteNonQuery(
                "UPDATE Suppliers SET SupplierName=@n,ContactNo=@c,Address=@a,Email=@e WHERE SupplierID=@id",
                new SqlParameter("@n",  s.SupplierName),
                new SqlParameter("@c",  (object)s.ContactNo ?? System.DBNull.Value),
                new SqlParameter("@a",  (object)s.Address   ?? System.DBNull.Value),
                new SqlParameter("@e",  (object)s.Email     ?? System.DBNull.Value),
                new SqlParameter("@id", s.SupplierID)) > 0;

        public bool Delete(int id)
            => DatabaseHelper.ExecuteNonQuery(
                "UPDATE Suppliers SET IsActive=0 WHERE SupplierID=@id",
                new SqlParameter("@id", id)) > 0;

        private static Supplier Map(DataRow r) => new Supplier
        {
            SupplierID   = (int)r["SupplierID"],
            SupplierName = r["SupplierName"].ToString(),
            ContactNo    = r["ContactNo"]?.ToString(),
            Address      = r["Address"]?.ToString(),
            Email        = r["Email"]?.ToString(),
            IsActive     = (bool)r["IsActive"]
        };
    }
}
