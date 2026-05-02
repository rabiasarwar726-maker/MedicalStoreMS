// BusinessLogic/Services.cs  –  all service classes
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.DataAccess;
using MedicalStoreMS.Models;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.BusinessLogic
{
    // ─────────────────────────────────────────────
    //  Session (currently logged-in user)
    // ─────────────────────────────────────────────
    public static class Session
    {
        public static User CurrentUser { get; set; }
        public static bool IsAdmin     => CurrentUser?.Role == "Admin";
        public static void Clear()     => CurrentUser = null;
    }

    // ─────────────────────────────────────────────
    //  AuthService
    // ─────────────────────────────────────────────
    public class AuthService
    {
        private readonly UserRepository _repo = new UserRepository();

        public (bool success, string message) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password are required.");

            var user = _repo.Authenticate(username.Trim(), password);
            if (user == null)
                return (false, "Invalid credentials. Please try again.");

            Session.CurrentUser = user;
            AuditService.Log("Login", $"User '{username}' logged in.");
            return (true, $"Welcome, {user.FullName}!");
        }

        public void Logout()
        {
            AuditService.Log("Logout", $"User '{Session.CurrentUser?.Username}' logged out.");
            Session.Clear();
        }
    }

    // ─────────────────────────────────────────────
    //  MedicineService
    // ─────────────────────────────────────────────
    public class MedicineService
    {
        private readonly MedicineRepository _repo = new MedicineRepository();

        public List<Medicine> GetAll()            => _repo.GetAll();
        public List<Medicine> Search(string kw)   => _repo.Search(kw);
        public List<Medicine> GetExpired()         => _repo.GetExpired();
        public List<Medicine> GetLowStock()        => _repo.GetLowStock();
        public List<Medicine> GetNearExpiry(int d) => _repo.GetNearExpiry(d);
        public Medicine       GetById(int id)      => _repo.GetById(id);

        public (bool ok, string msg) Add(Medicine m)
        {
            if (string.IsNullOrWhiteSpace(m.MedicineName)) return (false, "Medicine name is required.");
            if (m.UnitPrice <= 0)                          return (false, "Price must be greater than 0.");
            if (m.ExpiryDate <= DateTime.Today)            return (false, "Expiry date must be in the future.");

            int id = _repo.Add(m);
            AuditService.Log("AddMedicine", $"Added: {m.MedicineName}");
            return id > 0 ? (true, "Medicine added successfully.") : (false, "Failed to add medicine.");
        }

        public (bool ok, string msg) Update(Medicine m)
        {
            if (string.IsNullOrWhiteSpace(m.MedicineName)) return (false, "Medicine name is required.");
            bool ok = _repo.Update(m);
            if (ok) AuditService.Log("UpdateMedicine", $"Updated: {m.MedicineName}");
            return ok ? (true, "Updated successfully.") : (false, "Update failed.");
        }

        public (bool ok, string msg) Delete(int id, string name)
        {
            bool ok = _repo.Delete(id);
            if (ok) AuditService.Log("DeleteMedicine", $"Deleted medicine ID {id}: {name}");
            return ok ? (true, "Medicine removed.") : (false, "Delete failed.");
        }
    }

    // ─────────────────────────────────────────────
    //  SalesService
    // ─────────────────────────────────────────────
    public class SalesService
    {
        private readonly InvoiceRepository  _invRepo = new InvoiceRepository();
        private readonly MedicineRepository _medRepo = new MedicineRepository();

        public (bool ok, int invoiceId, string msg) ProcessSale(Invoice inv)
        {
            // Validate
            if (inv.Details == null || inv.Details.Count == 0)
                return (false, 0, "No items in the invoice.");

            foreach (var d in inv.Details)
            {
                var med = _medRepo.GetById(d.MedicineID);
                if (med == null)         return (false, 0, $"Medicine ID {d.MedicineID} not found.");
                if (med.IsExpired)       return (false, 0, $"{med.MedicineName} is expired.");
                if (med.Quantity < d.Quantity)
                    return (false, 0, $"Insufficient stock for {med.MedicineName}. Available: {med.Quantity}");
            }

            inv.InvoiceDate = DateTime.Now;
            inv.CreatedByUserID = Session.CurrentUser.UserID;

            int id = _invRepo.Save(inv);
            AuditService.Log("Sale", $"Invoice #{id} — Rs {inv.NetAmount:N2}");
            return (true, id, $"Sale completed! Invoice #{id}");
        }

        public List<Invoice> GetSales(DateTime? from, DateTime? to) => _invRepo.GetAll(from, to);
        public Invoice        GetById(int id)                        => _invRepo.GetById(id);
        public decimal        TodaySales()                           => _invRepo.GetTodaySales();
        public decimal        MonthSales()                           => _invRepo.GetMonthSales();
    }

    // ─────────────────────────────────────────────
    //  PurchaseService
    // ─────────────────────────────────────────────
    public class PurchaseService
    {
        private readonly PurchaseRepository _repo = new PurchaseRepository();

        public (bool ok, int purchaseId, string msg) RecordPurchase(Purchase p)
        {
            if (p.Details == null || p.Details.Count == 0)
                return (false, 0, "No items in the purchase.");

            p.PurchaseDate      = DateTime.Now;
            p.CreatedByUserID   = Session.CurrentUser.UserID;

            int id = _repo.Save(p);
            AuditService.Log("Purchase", $"Purchase #{id} — Rs {p.TotalAmount:N2}");
            return (true, id, $"Purchase recorded! ID #{id}");
        }

        public List<Purchase> GetAll(DateTime? from, DateTime? to) => _repo.GetAll(from, to);
    }

    // ─────────────────────────────────────────────
    //  DashboardService
    // ─────────────────────────────────────────────
    public class DashboardService
    {
        private readonly MedicineRepository _medRepo = new MedicineRepository();
        private readonly InvoiceRepository  _invRepo = new InvoiceRepository();
        private readonly SupplierRepository _supRepo = new SupplierRepository();

        public DashboardStats GetStats()
        {
            var meds = _medRepo.GetAll();
            return new DashboardStats
            {
                TotalMedicines  = meds.Count,
                LowStockCount   = _medRepo.GetLowStock().Count,
                ExpiredCount    = _medRepo.GetExpired().Count,
                NearExpiryCount = _medRepo.GetNearExpiry(AppConfig.ExpiryAlertDays).Count,
                TodaySales      = _invRepo.GetTodaySales(),
                MonthSales      = _invRepo.GetMonthSales(),
                TotalSuppliers  = _supRepo.GetAll().Count
            };
        }
    }

    // ─────────────────────────────────────────────
    //  AuditService
    // ─────────────────────────────────────────────
    public static class AuditService
    {
        public static void Log(string action, string details = null)
        {
            try
            {
                var parms = new Microsoft.Data.SqlClient.SqlParameter[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@uid", (object)Session.CurrentUser?.UserID ?? DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@a",   action),
                    new Microsoft.Data.SqlClient.SqlParameter("@d",   (object)details ?? DBNull.Value)
                };
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO AuditLog(UserID,Action,Details) VALUES(@uid,@a,@d)",
                    parms);
            }
            catch { }
        }
    }
}
