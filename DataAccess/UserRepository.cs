// DataAccess/UserRepository.cs
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using MedicalStoreMS.Models;
using MedicalStoreMS.Utils;

namespace MedicalStoreMS.DataAccess
{
    public class UserRepository
    {
        public User Authenticate(string username, string password)
        {
            var hash = PasswordHelper.HashPassword(password);
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM Users WHERE Username=@u AND PasswordHash=@p AND IsActive=1",
                new SqlParameter("@u", username),
                new SqlParameter("@p", hash));

            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new User
            {
                UserID   = (int)r["UserID"],
                Username = r["Username"].ToString(),
                FullName = r["FullName"].ToString(),
                Role     = r["Role"].ToString(),
                IsActive = (bool)r["IsActive"]
            };
        }

        public List<User> GetAll()
        {
            var list = new List<User>();
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Users ORDER BY FullName");
            foreach (System.Data.DataRow r in dt.Rows)
                list.Add(Map(r));
            return list;
        }

        public bool Create(User u)
        {
            var rows = DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO Users(Username,PasswordHash,FullName,Role,IsActive)
                  VALUES(@u,@p,@fn,@r,1)",
                new SqlParameter("@u",  u.Username),
                new SqlParameter("@p",  PasswordHelper.HashPassword(u.PasswordHash)),
                new SqlParameter("@fn", u.FullName),
                new SqlParameter("@r",  u.Role));
            return rows > 0;
        }

        public bool ChangePassword(int userId, string newPassword)
        {
            var hash = PasswordHelper.HashPassword(newPassword);
            var rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET PasswordHash=@p WHERE UserID=@id",
                new SqlParameter("@p",  hash),
                new SqlParameter("@id", userId));
            return rows > 0;
        }

        public bool ToggleStatus(int userId)
        {
            var rows = DatabaseHelper.ExecuteNonQuery(
                "UPDATE Users SET IsActive = CASE WHEN IsActive=1 THEN 0 ELSE 1 END WHERE UserID=@id",
                new SqlParameter("@id", userId));
            return rows > 0;
        }

        private static User Map(System.Data.DataRow r) => new User
        {
            UserID   = (int)r["UserID"],
            Username = r["Username"].ToString(),
            FullName = r["FullName"].ToString(),
            Role     = r["Role"].ToString(),
            IsActive = (bool)r["IsActive"]
        };
    }
}
