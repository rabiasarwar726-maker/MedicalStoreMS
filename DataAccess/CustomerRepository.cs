// DataAccess/CustomerRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Models;

namespace MedicalStoreMS.DataAccess
{
    public class CustomerRepository
    {
        public List<Customer> GetAll()
        {
            var dt   = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers ORDER BY CustomerName");
            var list = new List<Customer>();
            foreach (DataRow r in dt.Rows) list.Add(Map(r));
            return list;
        }

        public Customer GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers WHERE CustomerID=@id",
                new SqlParameter("@id", id));
            return dt.Rows.Count > 0 ? Map(dt.Rows[0]) : null;
        }

        public int Add(Customer c)
            => DatabaseHelper.ExecuteScalar<int>(@"
                INSERT INTO Customers(CustomerName,ContactNo,Address,Email)
                OUTPUT INSERTED.CustomerID VALUES(@n,@c,@a,@e)",
                0,
                new SqlParameter("@n", c.CustomerName),
                new SqlParameter("@c", (object)c.ContactNo ?? DBNull.Value),
                new SqlParameter("@a", (object)c.Address   ?? DBNull.Value),
                new SqlParameter("@e", (object)c.Email     ?? DBNull.Value));

        public bool Update(Customer c)
            => DatabaseHelper.ExecuteNonQuery(@"
                UPDATE Customers SET CustomerName=@n,ContactNo=@c,Address=@a,Email=@e
                WHERE CustomerID=@id",
                new SqlParameter("@n",  c.CustomerName),
                new SqlParameter("@c",  (object)c.ContactNo ?? DBNull.Value),
                new SqlParameter("@a",  (object)c.Address   ?? DBNull.Value),
                new SqlParameter("@e",  (object)c.Email     ?? DBNull.Value),
                new SqlParameter("@id", c.CustomerID)) > 0;

        public bool Delete(int id)
            => DatabaseHelper.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@id",
                new SqlParameter("@id", id)) > 0;

        private static Customer Map(DataRow r) => new Customer
        {
            CustomerID   = (int)r["CustomerID"],
            CustomerName = r["CustomerName"].ToString(),
            ContactNo    = r["ContactNo"]?.ToString(),
            Address      = r["Address"]?.ToString(),
            Email        = r["Email"]?.ToString(),
            CreatedAt    = Convert.ToDateTime(r["CreatedAt"])
        };
    }
}
