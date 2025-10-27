// ====================================
// Database Helper - SQLite Operations
// Install: System.Data.SQLite via NuGet
// ====================================

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ClinicManagementSystem.Data
{
    public class DatabaseHelper
    {
        private static DatabaseHelper _instance;
        private static readonly object _lock = new object();
        private string _connectionString;

        public static DatabaseHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        private DatabaseHelper()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClinicManagementSystem",
                "ClinicDB.db"
            );

            // إنشاء المجلد إذا لم يكن موجود
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            _connectionString = $"Data Source={dbPath};Version=3;";

            // إنشاء قاعدة البيانات إذا لم تكن موجودة
            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        private void CreateDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // قراءة السكريبت من ملف أو استخدام السكريبت المدمج
                string createTablesScript = GetDatabaseSchema();

                using (var command = new SQLiteCommand(createTablesScript, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private string GetDatabaseSchema()
        {
            // هنا نضع السكريبت الكامل من ملف SQL السابق
            // يمكن قراءته من ملف منفصل أو تضمينه كـ Resource
            return @"
                -- ضع هنا نفس السكريبت من الـ Artifact السابق
                -- أو اقرأه من ملف خارجي
            ";
        }

        // ====================================
        // Helper Methods
        // ====================================

        public int ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, params SQLiteParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteScalar();
                }
            }
        }

        public DataTable ExecuteQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public SQLiteDataReader ExecuteReader(string query, SQLiteConnection connection, params SQLiteParameter[] parameters)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                return command.ExecuteReader();
            }
        }

        // ====================================
        // Backup & Restore
        // ====================================

        public bool BackupDatabase(string backupPath)
        {
            try
            {
                string dbPath = new SQLiteConnectionStringBuilder(_connectionString).DataSource;
                File.Copy(dbPath, backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        public bool RestoreDatabase(string backupPath)
        {
            try
            {
                string dbPath = new SQLiteConnectionStringBuilder(_connectionString).DataSource;
                File.Copy(backupPath, dbPath, true);
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        // ====================================
        // Transaction Support
        // ====================================

        public void ExecuteTransaction(Action<SQLiteConnection, SQLiteTransaction> action)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        action(connection, transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}