using System;
using System.Data;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.DataAccess
{
    public static class DatabaseHelper
    {
        private static string ConnStr => AppConfig.ConnectionString;

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConnStr);
            conn.Open();
            return conn;
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            var dt = new DataTable();
            new SqlDataAdapter(cmd).Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static T ExecuteScalar<T>(string sql, T defaultVal, params SqlParameter[] parameters)
        {
            var result = ExecuteScalar(sql, parameters);
            if (result == null || result == DBNull.Value) return defaultVal;
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}