# MediCare — Medical Store Management System
### Author: Amina Yousaf | 2022-ag-9156 | University of Agriculture, Faisalabad

---

## 📋 Project Overview

A full-featured **Windows Forms + C# + SQL Server** desktop application for managing a medical/pharmaceutical store. Covers medicines, billing, purchases, suppliers, customers, reports, and user management — all with a modern, hover-animated UI.

---

## 🗂 Folder Structure

```
MedicalStoreMS/
├── Program.cs                        ← App entry point
├── App.config                        ← DB connection string
├── MedicalStoreMS.csproj             ← .NET 6 project file
│
├── Database/
│   └── CreateDatabase.sql            ← Full SQL schema + seed data
│
├── Models/
│   └── Models.cs                     ← All entity classes
│
├── DataAccess/
│   ├── DatabaseHelper.cs             ← ADO.NET base helper
│   ├── UserRepository.cs             ← User CRUD + auth
│   ├── MedicineRepository.cs         ← Medicine CRUD + stock
│   ├── InvoiceRepository.cs          ← Sales invoices (transactional)
│   ├── PurchaseRepository.cs         ← Purchase orders (transactional)
│   ├── SupplierRepository.cs         ← Supplier CRUD
│   ├── CustomerRepository.cs         ← Customer CRUD
│   └── AuditRepository.cs            ← Audit log reader
│
├── BusinessLogic/
│   └── Services.cs                   ← AuthService, MedicineService,
│                                        SalesService, PurchaseService,
│                                        DashboardService, AuditService,
│                                        Session (current user)
│
├── Reports/
│   └── ReportPrinter.cs              ← Print invoice + inventory (GDI+)
│
├── Utils/
│   ├── AppConfig.cs                  ← App-wide config reader
│   └── PasswordHelper.cs             ← SHA-256 password hashing
│
└── UI/
    ├── Themes/
    │   └── AppTheme.cs               ← Full design system (colors, fonts)
    ├── Forms/
    │   ├── LoginForm.cs              ← Animated login screen
    │   └── MainForm.cs               ← Shell + hover sidebar navigation
    └── Controls/
        ├── HoverButton.cs            ← Custom animated button (hover glow)
        ├── StatCard.cs               ← Dashboard hover-lift stat cards
        ├── UIHelper.cs               ← Shared factory: grid, buttons, etc.
        ├── DashboardControl.cs       ← Dashboard with stat cards
        └── AllControls.cs            ← All other screens:
                                         MedicinesControl, MedicineDialog,
                                         BillingControl, PurchasesControl,
                                         SuppliersControl, SupplierDialog,
                                         CustomersControl, ReportsControl,
                                         AuditControl, UsersControl, UserDialog
```

---

## ⚙️ Setup Instructions

### Prerequisites
- **Visual Studio 2022** (Community or higher) with .NET Desktop workload
- **.NET 6.0 SDK** (or .NET 8 — update TargetFramework in .csproj)
- **SQL Server 2019+** or **SQL Server Express**
- **SQL Server Management Studio (SSMS)** (optional but recommended)

### Step 1 — Database Setup
1. Open **SSMS** and connect to your SQL Server instance
2. Open `Database/CreateDatabase.sql`
3. Execute the script — it creates the database, all tables, and seed data
4. Default admin login: **Username:** `admin` | **Password:** `Admin@123`

### Step 2 — Configure Connection String
Edit `App.config`:
```xml
<add name="MedicalStoreDB"
     connectionString="Server=YOUR_SERVER\SQLEXPRESS;Database=MedicalStoreDB;
                       Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```
Replace `YOUR_SERVER\SQLEXPRESS` with your actual SQL Server instance name.

### Step 3 — Build & Run
```bash
# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```
Or open `MedicalStoreMS.csproj` in **Visual Studio** and press **F5**.

---

## 🔐 Default Credentials
| Role  | Username | Password  |
|-------|----------|-----------|
| Admin | admin    | Admin@123 |

---

## ✨ Features

| Module             | Description |
|--------------------|-------------|
| **Dashboard**       | Live stats: total medicines, today's sales, month revenue, low stock, expiry alerts. Hover-animated stat cards. |
| **Medicine Inventory** | Add, edit, delete medicines. Filter by: All / Low Stock / Expired / Near Expiry. Search by name/batch/category. Print inventory. |
| **Billing**         | Point-of-sale invoice creation. Add multiple items, apply discount, process sale, print invoice. Blocked on expired or insufficient stock. |
| **Purchases**       | Record supplier purchases. Auto-updates stock levels via transaction. |
| **Suppliers**       | Full CRUD for suppliers. |
| **Customers**       | Customer directory management. |
| **Reports**         | Sales report, Purchase report, Inventory report, Expired medicines, Low stock report — all filterable by date. |
| **Audit Log**       | Full admin activity trail — every login, sale, addition, and deletion is logged. |
| **User Management** | Admin-only: add users, toggle active/inactive, reset passwords. |

---

## 🎨 UI Design Highlights

- **HoverButton** — custom `Control` with smooth color lerp animation on hover/press, rounded corners, optional drop shadow
- **StatCard** — dashboard cards that lift on hover with animated shadow and accent glow
- **NavButton** — sidebar nav items with animated highlight bar and smooth color fade
- **Animated Login** — pulsing circle decorations on left panel via `System.Windows.Forms.Timer`
- **DataGridView** — row hover highlight, alternating row colors, custom column headers
- **Design System** — single `AppTheme.cs` defines all colors, fonts, and semantic values

---

## 🏗 Architecture

```
UI Layer (WinForms)
    │
    ▼
BusinessLogic (Services + Session)
    │
    ▼
DataAccess (Repositories + DatabaseHelper)
    │
    ▼
SQL Server (ADO.NET — System.Data.SqlClient)
```

- **No ORM** — raw ADO.NET for full control and performance
- **Repository pattern** — each entity has its own repository class
- **Service layer** — business rules and validation sit above repositories
- **Transactional** — sales and purchases use `SqlTransaction` to ensure stock and records are always consistent
- **Audit trail** — every user action is logged to `AuditLog` table

---

## 📦 NuGet Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.Data.SqlClient 5.2.1` | SQL Server connectivity |
| `iTextSharp 5.5.13.3` | PDF export (optional extension) |
| `Microsoft.VisualBasic` | InputBox dialogs |

---

## 🔒 Security

- Passwords stored as **SHA-256** hash with application-level salt (never plaintext)
- Role-based access: `Admin` vs `Staff` enforced at service and UI level
- All DB access through parameterised queries — **SQL injection safe**
- Audit log captures every significant action with timestamp and user

---

*MediCare Medical Store Management System — University of Agriculture Faisalabad, 2024*
