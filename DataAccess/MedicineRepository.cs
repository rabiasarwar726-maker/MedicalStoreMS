// DataAccess/MedicineRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Models;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.DataAccess
{
    public class MedicineRepository
    {
        private const string BASE_SQL = @"
            SELECT m.*, s.SupplierName
            FROM   Medicines m
            LEFT JOIN Suppliers s ON m.SupplierID = s.SupplierID
            WHERE  m.IsActive = 1";

        public List<Medicine> GetAll()
        {
            var dt   = DatabaseHelper.ExecuteQuery(BASE_SQL + " ORDER BY m.MedicineName");
            return MapList(dt);
        }

        public List<Medicine> Search(string keyword)
        {
            var dt = DatabaseHelper.ExecuteQuery(
                BASE_SQL + " AND (m.MedicineName LIKE @k OR m.BatchNo LIKE @k OR m.Category LIKE @k) ORDER BY m.MedicineName",
                new SqlParameter("@k", $"%{keyword}%"));
            return MapList(dt);
        }

        public List<Medicine> GetExpired()
        {
            var dt = DatabaseHelper.ExecuteQuery(
                BASE_SQL + " AND m.ExpiryDate < @today ORDER BY m.ExpiryDate",
                new SqlParameter("@today", DateTime.Today));
            return MapList(dt);
        }

        public List<Medicine> GetLowStock()
        {
            var dt = DatabaseHelper.ExecuteQuery(
                BASE_SQL + " AND m.Quantity <= m.MinStockLevel ORDER BY m.Quantity");
            return MapList(dt);
        }

        public List<Medicine> GetNearExpiry(int days = 30)
        {
            var dt = DatabaseHelper.ExecuteQuery(
                BASE_SQL + @" AND m.ExpiryDate >= @today AND m.ExpiryDate <= @limit ORDER BY m.ExpiryDate",
                new SqlParameter("@today", DateTime.Today),
                new SqlParameter("@limit", DateTime.Today.AddDays(days)));
            return MapList(dt);
        }

        public Medicine GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery(
                BASE_SQL + " AND m.MedicineID = @id",
                new SqlParameter("@id", id));
            return dt.Rows.Count > 0 ? Map(dt.Rows[0]) : null;
        }

        public int Add(Medicine m)
        {
            var id = DatabaseHelper.ExecuteScalar<int>(
                @"INSERT INTO Medicines(MedicineName,BatchNo,Category,Description,ExpiryDate,
                    Quantity,UnitPrice,SupplierID,MinStockLevel,IsActive)
                  OUTPUT INSERTED.MedicineID
                  VALUES(@n,@b,@c,@d,@e,@q,@p,@s,@ml,1)",
                0,
                new SqlParameter("@n",  m.MedicineName),
                new SqlParameter("@b",  (object)m.BatchNo      ?? DBNull.Value),
                new SqlParameter("@c",  (object)m.Category     ?? DBNull.Value),
                new SqlParameter("@d",  (object)m.Description  ?? DBNull.Value),
                new SqlParameter("@e",  m.ExpiryDate),
                new SqlParameter("@q",  m.Quantity),
                new SqlParameter("@p",  m.UnitPrice),
                new SqlParameter("@s",  (object)m.SupplierID   ?? DBNull.Value),
                new SqlParameter("@ml", m.MinStockLevel));
            return id;
        }

        public bool Update(Medicine m)
        {
            var rows = DatabaseHelper.ExecuteNonQuery(
                @"UPDATE Medicines SET MedicineName=@n, BatchNo=@b, Category=@c, Description=@d,
                    ExpiryDate=@e, Quantity=@q, UnitPrice=@p, SupplierID=@s, MinStockLevel=@ml
                  WHERE MedicineID=@id",
                new SqlParameter("@n",  m.MedicineName),
                new SqlParameter("@b",  (object)m.BatchNo     ?? DBNull.Value),
                new SqlParameter("@c",  (object)m.Category    ?? DBNull.Value),
                new SqlParameter("@d",  (object)m.Description ?? DBNull.Value),
                new SqlParameter("@e",  m.ExpiryDate),
                new SqlParameter("@q",  m.Quantity),
                new SqlParameter("@p",  m.UnitPrice),
                new SqlParameter("@s",  (object)m.SupplierID  ?? DBNull.Value),
                new SqlParameter("@ml", m.MinStockLevel),
                new SqlParameter("@id", m.MedicineID));
            return rows > 0;
        }

        public bool Delete(int id)
        {
            var rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Medicines SET IsActive=0 WHERE MedicineID=@id",
                new SqlParameter("@id", id));
            return rows > 0;
        }

        public bool UpdateStock(int medicineId, int quantityChange, SqlConnection conn, SqlTransaction tx)
        {
            using var cmd = new SqlCommand(
                "UPDATE Medicines SET Quantity = Quantity + @q WHERE MedicineID = @id", conn, tx);
            cmd.Parameters.AddWithValue("@q",  quantityChange);
            cmd.Parameters.AddWithValue("@id", medicineId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ── helpers ──────────────────────────────────────────────
        private static List<Medicine> MapList(DataTable dt)
        {
            var list = new List<Medicine>();
            foreach (DataRow r in dt.Rows) list.Add(Map(r));
            return list;
        }

        private static Medicine Map(DataRow r) => new Medicine
        {
            MedicineID    = (int)r["MedicineID"],
            MedicineName  = r["MedicineName"].ToString(),
            BatchNo       = r["BatchNo"]?.ToString(),
            Category      = r["Category"]?.ToString(),
            Description   = r["Description"]?.ToString(),
            ExpiryDate    = Convert.ToDateTime(r["ExpiryDate"]),
            Quantity      = (int)r["Quantity"],
            UnitPrice     = (decimal)r["UnitPrice"],
            SupplierID    = r["SupplierID"] == DBNull.Value ? (int?)null : (int)r["SupplierID"],
            SupplierName  = r["SupplierName"]?.ToString(),
            MinStockLevel = (int)r["MinStockLevel"],
            IsActive      = (bool)r["IsActive"],
            CreatedAt     = Convert.ToDateTime(r["CreatedAt"])
        };
    }
}
