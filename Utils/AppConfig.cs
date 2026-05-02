using System.Configuration;

namespace MedicalStoreMS.Utils
{
    public static class AppConfig
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["MedicalStoreDB"]?.ConnectionString
            ?? "Server=.\\SQLEXPRESS;Database=MedicalStoreDB;Integrated Security=True;TrustServerCertificate=True;";

        public static int LowStockThreshold => 10;
        public static int ExpiryAlertDays => 30;
        public static string AppName => "MediCare Store Management";
        public static string AppVersion => "1.0.0";
    }
}