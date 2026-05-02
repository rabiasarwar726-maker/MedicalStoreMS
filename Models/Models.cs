// Models/Models.cs  –  all entity models in one file for clarity
using System;
using System.Collections.Generic;

namespace MedicalStoreMS.Models
{
    // ─────────────────────────────────────────────
    //  User
    // ─────────────────────────────────────────────
    public class User
    {
        public int    UserID       { get; set; }
        public string Username     { get; set; }
        public string PasswordHash { get; set; }
        public string FullName     { get; set; }
        public string Role         { get; set; }   // "Admin" | "Staff"
        public bool   IsActive     { get; set; }
        public DateTime CreatedAt  { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Supplier
    // ─────────────────────────────────────────────
    public class Supplier
    {
        public int    SupplierID   { get; set; }
        public string SupplierName { get; set; }
        public string ContactNo    { get; set; }
        public string Address      { get; set; }
        public string Email        { get; set; }
        public bool   IsActive     { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Customer
    // ─────────────────────────────────────────────
    public class Customer
    {
        public int    CustomerID   { get; set; }
        public string CustomerName { get; set; }
        public string ContactNo    { get; set; }
        public string Address      { get; set; }
        public string Email        { get; set; }
        public DateTime CreatedAt  { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Medicine
    // ─────────────────────────────────────────────
    public class Medicine
    {
        public int      MedicineID    { get; set; }
        public string   MedicineName  { get; set; }
        public string   BatchNo       { get; set; }
        public string   Category      { get; set; }
        public string   Description   { get; set; }
        public DateTime ExpiryDate    { get; set; }
        public int      Quantity      { get; set; }
        public decimal  UnitPrice     { get; set; }
        public int?     SupplierID    { get; set; }
        public string   SupplierName  { get; set; }   // joined
        public int      MinStockLevel { get; set; }
        public bool     IsActive      { get; set; }
        public DateTime CreatedAt     { get; set; }

        public bool IsExpired       => ExpiryDate.Date < DateTime.Today;
        public bool IsNearExpiry    => !IsExpired && ExpiryDate.Date <= DateTime.Today.AddDays(30);
        public bool IsLowStock      => Quantity <= MinStockLevel;
    }

    // ─────────────────────────────────────────────
    //  Invoice
    // ─────────────────────────────────────────────
    public class Invoice
    {
        public int      InvoiceID    { get; set; }
        public DateTime InvoiceDate  { get; set; }
        public int?     CustomerID   { get; set; }
        public string   CustomerName { get; set; }
        public decimal  TotalAmount  { get; set; }
        public decimal  Discount     { get; set; }
        public decimal  NetAmount    { get; set; }
        public string   PaymentMode  { get; set; }
        public int      CreatedByUserID { get; set; }
        public string   Notes        { get; set; }
        public List<InvoiceDetail> Details { get; set; } = new();
    }

    public class InvoiceDetail
    {
        public int     DetailID    { get; set; }
        public int     InvoiceID   { get; set; }
        public int     MedicineID  { get; set; }
        public string  MedicineName { get; set; }
        public int     Quantity    { get; set; }
        public decimal UnitPrice   { get; set; }
        public decimal SubTotal    { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Purchase
    // ─────────────────────────────────────────────
    public class Purchase
    {
        public int      PurchaseID  { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int?     SupplierID  { get; set; }
        public string   SupplierName { get; set; }
        public decimal  TotalAmount { get; set; }
        public int      CreatedByUserID { get; set; }
        public string   Notes       { get; set; }
        public List<PurchaseDetail> Details { get; set; } = new();
    }

    public class PurchaseDetail
    {
        public int     DetailID    { get; set; }
        public int     PurchaseID  { get; set; }
        public int     MedicineID  { get; set; }
        public string  MedicineName { get; set; }
        public int     Quantity    { get; set; }
        public decimal UnitCost    { get; set; }
        public decimal SubTotal    { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Dashboard Summary
    // ─────────────────────────────────────────────
    public class DashboardStats
    {
        public int     TotalMedicines     { get; set; }
        public int     LowStockCount      { get; set; }
        public int     ExpiredCount       { get; set; }
        public int     NearExpiryCount    { get; set; }
        public decimal TodaySales         { get; set; }
        public decimal MonthSales         { get; set; }
        public int     TotalCustomers     { get; set; }
        public int     TotalSuppliers     { get; set; }
    }
}
