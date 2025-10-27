// ====================================
// Inventory Repository - إدارة المخزون
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClinicManagementSystem.Repositories
{
    public class InventoryRepository
    {
        private readonly DatabaseHelper _db;

        public InventoryRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Create Item
        // ====================================
        public int AddItem(InventoryItem item)
        {
            string query = @"
                INSERT INTO Inventory (
                    ItemCode, ItemName, Category, Description, Unit,
                    Quantity, MinimumStock, UnitPrice, SupplierName,
                    SupplierPhone, ExpiryDate, Location, IsActive
                ) VALUES (
                    @ItemCode, @ItemName, @Category, @Description, @Unit,
                    @Quantity, @MinimumStock, @UnitPrice, @SupplierName,
                    @SupplierPhone, @ExpiryDate, @Location, @IsActive
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@ItemCode", item.ItemCode ?? GenerateItemCode()),
                new SQLiteParameter("@ItemName", item.ItemName),
                new SQLiteParameter("@Category", item.Category),
                new SQLiteParameter("@Description", (object)item.Description ?? DBNull.Value),
                new SQLiteParameter("@Unit", (object)item.Unit ?? DBNull.Value),
                new SQLiteParameter("@Quantity", item.Quantity),
                new SQLiteParameter("@MinimumStock", item.MinimumStock),
                new SQLiteParameter("@UnitPrice", item.UnitPrice),
                new SQLiteParameter("@SupplierName", (object)item.SupplierName ?? DBNull.Value),
                new SQLiteParameter("@SupplierPhone", (object)item.SupplierPhone ?? DBNull.Value),
                new SQLiteParameter("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                new SQLiteParameter("@Location", (object)item.Location ?? DBNull.Value),
                new SQLiteParameter("@IsActive", item.IsActive ? 1 : 0)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        // ====================================
        // Read Items
        // ====================================
        public InventoryItem GetItemById(int itemId)
        {
            string query = "SELECT * FROM Inventory WHERE ItemID = @ItemID";
            var parameter = new SQLiteParameter("@ItemID", itemId);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToInventoryItem(table.Rows[0]);
            }

            return null;
        }

        public InventoryItem GetItemByCode(string itemCode)
        {
            string query = "SELECT * FROM Inventory WHERE ItemCode = @ItemCode";
            var parameter = new SQLiteParameter("@ItemCode", itemCode);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToInventoryItem(table.Rows[0]);
            }

            return null;
        }

        public List<InventoryItem> GetAllItems(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM Inventory WHERE IsActive = 1 ORDER BY ItemName"
                : "SELECT * FROM Inventory ORDER BY ItemName";

            var table = _db.ExecuteQuery(query);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        public List<InventoryItem> GetItemsByCategory(string category)
        {
            string query = "SELECT * FROM Inventory WHERE Category = @Category AND IsActive = 1 ORDER BY ItemName";
            var parameter = new SQLiteParameter("@Category", category);

            var table = _db.ExecuteQuery(query, parameter);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        public List<InventoryItem> SearchItems(string searchTerm)
        {
            string query = @"
                SELECT * FROM Inventory 
                WHERE (ItemName LIKE @SearchTerm 
                    OR ItemCode LIKE @SearchTerm 
                    OR Description LIKE @SearchTerm)
                AND IsActive = 1
                ORDER BY ItemName";

            var parameter = new SQLiteParameter("@SearchTerm", $"%{searchTerm}%");
            var table = _db.ExecuteQuery(query, parameter);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        // ====================================
        // Update Item
        // ====================================
        public bool UpdateItem(InventoryItem item)
        {
            string query = @"
                UPDATE Inventory SET
                    ItemName = @ItemName,
                    Category = @Category,
                    Description = @Description,
                    Unit = @Unit,
                    Quantity = @Quantity,
                    MinimumStock = @MinimumStock,
                    UnitPrice = @UnitPrice,
                    SupplierName = @SupplierName,
                    SupplierPhone = @SupplierPhone,
                    ExpiryDate = @ExpiryDate,
                    Location = @Location,
                    IsActive = @IsActive
                WHERE ItemID = @ItemID";

            var parameters = new[]
            {
                new SQLiteParameter("@ItemName", item.ItemName),
                new SQLiteParameter("@Category", item.Category),
                new SQLiteParameter("@Description", (object)item.Description ?? DBNull.Value),
                new SQLiteParameter("@Unit", (object)item.Unit ?? DBNull.Value),
                new SQLiteParameter("@Quantity", item.Quantity),
                new SQLiteParameter("@MinimumStock", item.MinimumStock),
                new SQLiteParameter("@UnitPrice", item.UnitPrice),
                new SQLiteParameter("@SupplierName", (object)item.SupplierName ?? DBNull.Value),
                new SQLiteParameter("@SupplierPhone", (object)item.SupplierPhone ?? DBNull.Value),
                new SQLiteParameter("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value.ToString("yyyy-MM-dd") : DBNull.Value),
                new SQLiteParameter("@Location", (object)item.Location ?? DBNull.Value),
                new SQLiteParameter("@IsActive", item.IsActive ? 1 : 0),
                new SQLiteParameter("@ItemID", item.ItemID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool UpdateQuantity(int itemId, int newQuantity)
        {
            string query = "UPDATE Inventory SET Quantity = @Quantity WHERE ItemID = @ItemID";
            var parameters = new[]
            {
                new SQLiteParameter("@Quantity", newQuantity),
                new SQLiteParameter("@ItemID", itemId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // Delete Item
        // ====================================
        public bool DeleteItem(int itemId)
        {
            string query = "UPDATE Inventory SET IsActive = 0 WHERE ItemID = @ItemID";
            var parameter = new SQLiteParameter("@ItemID", itemId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        // ====================================
        // Transactions (حركات المخزون)
        // ====================================
        public int AddTransaction(int itemId, string transactionType, int quantity, string reference, string notes, int? userId)
        {
            int transactionId = 0;

            _db.ExecuteTransaction((connection, transaction) =>
            {
                // إضافة الحركة
                string transQuery = @"
                    INSERT INTO InventoryTransactions (
                        ItemID, TransactionType, Quantity, TransactionDate,
                        Reference, Notes, UserID
                    ) VALUES (
                        @ItemID, @TransactionType, @Quantity, @TransactionDate,
                        @Reference, @Notes, @UserID
                    );
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(transQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    cmd.Parameters.AddWithValue("@TransactionType", transactionType);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Reference", (object)reference ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object)notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);

                    transactionId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // تحديث الكمية
                string updateQuery = "";
                switch (transactionType)
                {
                    case "إضافة":
                        updateQuery = "UPDATE Inventory SET Quantity = Quantity + @Quantity WHERE ItemID = @ItemID";
                        break;
                    case "استهلاك":
                        updateQuery = "UPDATE Inventory SET Quantity = Quantity - @Quantity WHERE ItemID = @ItemID";
                        break;
                    case "تسوية":
                        updateQuery = "UPDATE Inventory SET Quantity = @Quantity WHERE ItemID = @ItemID";
                        break;
                }

                if (!string.IsNullOrEmpty(updateQuery))
                {
                    using (var cmd = new SQLiteCommand(updateQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@ItemID", itemId);
                        cmd.ExecuteNonQuery();
                    }
                }
            });

            return transactionId;
        }

        public List<dynamic> GetItemTransactions(int itemId)
        {
            string query = @"
                SELECT t.*, u.FullName as UserName
                FROM InventoryTransactions t
                LEFT JOIN Users u ON t.UserID = u.UserID
                WHERE t.ItemID = @ItemID
                ORDER BY t.TransactionDate DESC";

            var parameter = new SQLiteParameter("@ItemID", itemId);
            var table = _db.ExecuteQuery(query, parameter);
            var transactions = new List<dynamic>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                transactions.Add(new
                {
                    TransactionID = Convert.ToInt32(row["TransactionID"]),
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    TransactionType = row["TransactionType"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    TransactionDate = Convert.ToDateTime(row["TransactionDate"]),
                    Reference = row["Reference"]?.ToString(),
                    Notes = row["Notes"]?.ToString(),
                    UserName = row["UserName"]?.ToString()
                });
            }

            return transactions;
        }

        // ====================================
        // Alerts & Reports
        // ====================================
        public List<InventoryItem> GetLowStockItems()
        {
            string query = "SELECT * FROM v_LowStockItems";
            var table = _db.ExecuteQuery(query);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        public List<InventoryItem> GetExpiringSoonItems(int daysAhead = 90)
        {
            string query = @"
                SELECT * FROM Inventory 
                WHERE ExpiryDate IS NOT NULL 
                  AND ExpiryDate <= DATE('now', '+' || @Days || ' days')
                  AND IsActive = 1
                ORDER BY ExpiryDate";

            var parameter = new SQLiteParameter("@Days", daysAhead);
            var table = _db.ExecuteQuery(query, parameter);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        public List<InventoryItem> GetExpiredItems()
        {
            string query = @"
                SELECT * FROM Inventory 
                WHERE ExpiryDate IS NOT NULL 
                  AND ExpiryDate < DATE('now')
                  AND IsActive = 1
                ORDER BY ExpiryDate";

            var table = _db.ExecuteQuery(query);
            var items = new List<InventoryItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(MapToInventoryItem(row));
            }

            return items;
        }

        public decimal GetTotalInventoryValue()
        {
            string query = "SELECT COALESCE(SUM(Quantity * UnitPrice), 0) FROM Inventory WHERE IsActive = 1";
            var result = _db.ExecuteScalar(query);
            return Convert.ToDecimal(result);
        }

        // ====================================
        // Helper Methods
        // ====================================
        private InventoryItem MapToInventoryItem(System.Data.DataRow row)
        {
            return new InventoryItem
            {
                ItemID = Convert.ToInt32(row["ItemID"]),
                ItemCode = row["ItemCode"].ToString(),
                ItemName = row["ItemName"].ToString(),
                Category = row["Category"].ToString(),
                Description = row["Description"]?.ToString(),
                Unit = row["Unit"]?.ToString(),
                Quantity = Convert.ToInt32(row["Quantity"]),
                MinimumStock = Convert.ToInt32(row["MinimumStock"]),
                UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                SupplierName = row["SupplierName"]?.ToString(),
                SupplierPhone = row["SupplierPhone"]?.ToString(),
                ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : (DateTime?)null,
                Location = row["Location"]?.ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }

        private string GenerateItemCode()
        {
            string query = "SELECT COUNT(*) FROM Inventory";
            int count = Convert.ToInt32(_db.ExecuteScalar(query));
            return $"ITM{DateTime.Now:yyyyMMdd}{(count + 1):D4}";
        }
    }
}