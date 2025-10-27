// ====================================
// User Repository - إدارة المستخدمين والصلاحيات
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace ClinicManagementSystem.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseHelper _db;

        public UserRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Authentication
        // ====================================
        public User Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            string query = @"
                SELECT * FROM Users 
                WHERE Username = @Username 
                  AND Password = @Password 
                  AND IsActive = 1";

            var parameters = new[]
            {
                new SQLiteParameter("@Username", username),
                new SQLiteParameter("@Password", hashedPassword)
            };

            var table = _db.ExecuteQuery(query, parameters);

            if (table.Rows.Count > 0)
            {
                return MapToUser(table.Rows[0]);
            }

            return null;
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            // التحقق من كلمة المرور القديمة
            string query = "SELECT Password FROM Users WHERE UserID = @UserID";
            var param = new SQLiteParameter("@UserID", userId);
            var result = _db.ExecuteScalar(query, param);

            if (result == null || result.ToString() != HashPassword(oldPassword))
            {
                return false;
            }

            // تغيير كلمة المرور
            string updateQuery = "UPDATE Users SET Password = @Password WHERE UserID = @UserID";
            var parameters = new[]
            {
                new SQLiteParameter("@Password", HashPassword(newPassword)),
                new SQLiteParameter("@UserID", userId)
            };

            int rowsAffected = _db.ExecuteNonQuery(updateQuery, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // CRUD Operations
        // ====================================
        public int AddUser(User user)
        {
            string query = @"
                INSERT INTO Users (
                    Username, Password, FullName, Role, PhoneNumber,
                    Email, Salary, HireDate, IsActive, Permissions, ProfileImagePath
                ) VALUES (
                    @Username, @Password, @FullName, @Role, @PhoneNumber,
                    @Email, @Salary, @HireDate, @IsActive, @Permissions, @ProfileImagePath
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@Username", user.Username),
                new SQLiteParameter("@Password", HashPassword(user.Password)),
                new SQLiteParameter("@FullName", user.FullName),
                new SQLiteParameter("@Role", user.Role),
                new SQLiteParameter("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value),
                new SQLiteParameter("@Email", (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Salary", user.Salary),
                new SQLiteParameter("@HireDate", user.HireDate.HasValue ? (object)user.HireDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SQLiteParameter("@Permissions", (object)user.Permissions ?? DBNull.Value),
                new SQLiteParameter("@ProfileImagePath", (object)user.ProfileImagePath ?? DBNull.Value)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        public User GetUserById(int userId)
        {
            string query = "SELECT * FROM Users WHERE UserID = @UserID";
            var parameter = new SQLiteParameter("@UserID", userId);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToUser(table.Rows[0]);
            }

            return null;
        }

        public List<User> GetAllUsers(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM Users WHERE IsActive = 1 ORDER BY FullName"
                : "SELECT * FROM Users ORDER BY FullName";

            var table = _db.ExecuteQuery(query);
            var users = new List<User>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                users.Add(MapToUser(row));
            }

            return users;
        }

        public List<User> GetUsersByRole(string role)
        {
            string query = "SELECT * FROM Users WHERE Role = @Role AND IsActive = 1 ORDER BY FullName";
            var parameter = new SQLiteParameter("@Role", role);

            var table = _db.ExecuteQuery(query, parameter);
            var users = new List<User>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                users.Add(MapToUser(row));
            }

            return users;
        }

        public bool UpdateUser(User user)
        {
            string query = @"
                UPDATE Users SET
                    FullName = @FullName,
                    Role = @Role,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email,
                    Salary = @Salary,
                    HireDate = @HireDate,
                    IsActive = @IsActive,
                    Permissions = @Permissions,
                    ProfileImagePath = @ProfileImagePath
                WHERE UserID = @UserID";

            var parameters = new[]
            {
                new SQLiteParameter("@FullName", user.FullName),
                new SQLiteParameter("@Role", user.Role),
                new SQLiteParameter("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value),
                new SQLiteParameter("@Email", (object)user.Email ?? DBNull.Value),
                new SQLiteParameter("@Salary", user.Salary),
                new SQLiteParameter("@HireDate", user.HireDate.HasValue ? (object)user.HireDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                new SQLiteParameter("@IsActive", user.IsActive ? 1 : 0),
                new SQLiteParameter("@Permissions", (object)user.Permissions ?? DBNull.Value),
                new SQLiteParameter("@ProfileImagePath", (object)user.ProfileImagePath ?? DBNull.Value),
                new SQLiteParameter("@UserID", user.UserID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool DeleteUser(int userId)
        {
            string query = "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID";
            var parameter = new SQLiteParameter("@UserID", userId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        // ====================================
        // Audit Log
        // ====================================
        public void LogActivity(int userId, string action, string tableName, int recordId, string details)
        {
            string query = @"
                INSERT INTO AuditLog (UserID, Action, TableName, RecordID, Details)
                VALUES (@UserID, @Action, @TableName, @RecordID, @Details)";

            var parameters = new[]
            {
                new SQLiteParameter("@UserID", userId),
                new SQLiteParameter("@Action", action),
                new SQLiteParameter("@TableName", tableName),
                new SQLiteParameter("@RecordID", recordId),
                new SQLiteParameter("@Details", (object)details ?? DBNull.Value)
            };

            _db.ExecuteNonQuery(query, parameters);
        }

        public List<dynamic> GetUserActivities(int userId, int limit = 50)
        {
            string query = @"
                SELECT * FROM AuditLog 
                WHERE UserID = @UserID 
                ORDER BY Timestamp DESC 
                LIMIT @Limit";

            var parameters = new[]
            {
                new SQLiteParameter("@UserID", userId),
                new SQLiteParameter("@Limit", limit)
            };

            var table = _db.ExecuteQuery(query, parameters);
            var activities = new List<dynamic>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                activities.Add(new
                {
                    LogID = Convert.ToInt32(row["LogID"]),
                    Action = row["Action"].ToString(),
                    TableName = row["TableName"].ToString(),
                    RecordID = Convert.ToInt32(row["RecordID"]),
                    Details = row["Details"]?.ToString(),
                    Timestamp = Convert.ToDateTime(row["Timestamp"])
                });
            }

            return activities;
        }

        // ====================================
        // Helper Methods
        // ====================================
        private User MapToUser(System.Data.DataRow row)
        {
            return new User
            {
                UserID = Convert.ToInt32(row["UserID"]),
                Username = row["Username"].ToString(),
                Password = row["Password"].ToString(), // لن نستخدمها خارج النظام
                FullName = row["FullName"].ToString(),
                Role = row["Role"].ToString(),
                PhoneNumber = row["PhoneNumber"]?.ToString(),
                Email = row["Email"]?.ToString(),
                Salary = Convert.ToDecimal(row["Salary"]),
                HireDate = row["HireDate"] != DBNull.Value ? Convert.ToDateTime(row["HireDate"]) : (DateTime?)null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                Permissions = row["Permissions"]?.ToString(),
                ProfileImagePath = row["ProfileImagePath"]?.ToString()
            };
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool IsUsernameAvailable(string username, int? excludeUserId = null)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            if (excludeUserId.HasValue)
            {
                query += " AND UserID != @UserID";
            }

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@Username", username)
            };

            if (excludeUserId.HasValue)
            {
                parameters.Add(new SQLiteParameter("@UserID", excludeUserId.Value));
            }

            var count = Convert.ToInt32(_db.ExecuteScalar(query, parameters.ToArray()));
            return count == 0;
        }
    }
}