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
        }

        /// <summary>
        /// تهيئة قاعدة البيانات - يجب استدعاؤها عند بدء التطبيق
        /// </summary>
        public void Initialize()
        {
            string dbPath = new SQLiteConnectionStringBuilder(_connectionString).DataSource;

            // إنشاء قاعدة البيانات إذا لم تكن موجودة
            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }
            else
            {
                // التحقق من وجود الجداول
                VerifyTables();
            }
        }

        private void VerifyTables()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string checkQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users'";
                    using (var command = new SQLiteCommand(checkQuery, connection))
                    {
                        var result = command.ExecuteScalar();

                        // إذا الجدول مش موجود، أنشئ كل الجداول
                        if (result == null)
                        {
                            CreateDatabase();
                        }
                    }
                }
            }
            catch
            {
                // إذا حصل خطأ، أنشئ قاعدة البيانات من جديد
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
            return @"
-- ====================================
-- 1. Users Table - المستخدمين والموظفين
-- ====================================
CREATE TABLE IF NOT EXISTS Users (
    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Role TEXT NOT NULL,
    PhoneNumber TEXT,
    Email TEXT,
    Salary REAL DEFAULT 0,
    HireDate TEXT,
    IsActive INTEGER DEFAULT 1,
    Permissions TEXT,
    ProfileImagePath TEXT,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    UpdatedAt TEXT DEFAULT (datetime('now', 'localtime'))
);

-- ====================================
-- 2. Patients Table - المرضى
-- ====================================
CREATE TABLE IF NOT EXISTS Patients (
    PatientID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientCode TEXT NOT NULL UNIQUE,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    DateOfBirth TEXT NOT NULL,
    Gender TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    PhoneNumber2 TEXT,
    Address TEXT,
    NationalID TEXT UNIQUE,
    BloodType TEXT,
    Email TEXT,
    EmergencyContact TEXT,
    EmergencyPhone TEXT,
    Notes TEXT,
    ProfileImagePath TEXT,
    RegistrationDate TEXT DEFAULT (datetime('now', 'localtime')),
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    UpdatedAt TEXT DEFAULT (datetime('now', 'localtime'))
);

-- ====================================
-- 3. Medical History - التاريخ المرضي
-- ====================================
CREATE TABLE IF NOT EXISTS MedicalHistory (
    HistoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    ChronicDiseases TEXT,
    Allergies TEXT,
    CurrentMedications TEXT,
    PreviousSurgeries TEXT,
    FamilyHistory TEXT,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    UpdatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE
);

-- ====================================
-- 4. Medical Documents - المستندات الطبية
-- ====================================
CREATE TABLE IF NOT EXISTS MedicalDocuments (
    DocumentID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    DocumentType TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    FilePath TEXT NOT NULL,
    FileSize INTEGER,
    UploadDate TEXT DEFAULT (datetime('now', 'localtime')),
    UploadedBy INTEGER,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES Users(UserID)
);

-- ====================================
-- 5. Visits - الزيارات والكشوفات
-- ====================================
CREATE TABLE IF NOT EXISTS Visits (
    VisitID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    VisitDate TEXT DEFAULT (datetime('now', 'localtime')),
    VisitType TEXT NOT NULL,
    ChiefComplaint TEXT,
    Diagnosis TEXT,
    Treatment TEXT,
    Notes TEXT,
    DoctorID INTEGER,
    VisitStatus TEXT DEFAULT 'منتظر',
    ExaminationFee REAL DEFAULT 0,
    IsPaid INTEGER DEFAULT 0,
    QueueNumber INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Users(UserID)
);

-- ====================================
-- 6. Appointments - المواعيد
-- ====================================
CREATE TABLE IF NOT EXISTS Appointments (
    AppointmentID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    AppointmentDate TEXT NOT NULL,
    AppointmentTime TEXT NOT NULL,
    DoctorID INTEGER,
    AppointmentType TEXT NOT NULL,
    Status TEXT DEFAULT 'محجوز',
    Notes TEXT,
    ReminderSent INTEGER DEFAULT 0,
    CreatedBy INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Users(UserID),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

-- ====================================
-- 7. Surgeries - العمليات الجراحية
-- ====================================
CREATE TABLE IF NOT EXISTS Surgeries (
    SurgeryID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    SurgeryName TEXT NOT NULL,
    SurgeryDate TEXT,
    ScheduledDate TEXT,
    SurgeryType TEXT,
    DoctorID INTEGER,
    AssistantDoctors TEXT,
    Anesthesia TEXT,
    Duration INTEGER,
    Notes TEXT,
    Cost REAL DEFAULT 0,
    Status TEXT DEFAULT 'مجدولة',
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Users(UserID)
);

-- ====================================
-- 8. Prescriptions - الروشتات الطبية
-- ====================================
CREATE TABLE IF NOT EXISTS Prescriptions (
    PrescriptionID INTEGER PRIMARY KEY AUTOINCREMENT,
    VisitID INTEGER,
    PatientID INTEGER NOT NULL,
    PrescriptionDate TEXT DEFAULT (datetime('now', 'localtime')),
    DoctorID INTEGER,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (VisitID) REFERENCES Visits(VisitID) ON DELETE SET NULL,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Users(UserID)
);

CREATE TABLE IF NOT EXISTS PrescriptionDetails (
    DetailID INTEGER PRIMARY KEY AUTOINCREMENT,
    PrescriptionID INTEGER NOT NULL,
    MedicineName TEXT NOT NULL,
    Dosage TEXT NOT NULL,
    Frequency TEXT NOT NULL,
    Duration TEXT NOT NULL,
    Instructions TEXT,
    FOREIGN KEY (PrescriptionID) REFERENCES Prescriptions(PrescriptionID) ON DELETE CASCADE
);

-- ====================================
-- 9. Invoices - الفواتير
-- ====================================
CREATE TABLE IF NOT EXISTS Invoices (
    InvoiceID INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceNumber TEXT NOT NULL UNIQUE,
    PatientID INTEGER NOT NULL,
    InvoiceDate TEXT DEFAULT (datetime('now', 'localtime')),
    TotalAmount REAL NOT NULL,
    DiscountAmount REAL DEFAULT 0,
    DiscountPercentage REAL DEFAULT 0,
    NetAmount REAL NOT NULL,
    PaidAmount REAL DEFAULT 0,
    RemainingAmount REAL NOT NULL,
    InvoiceType TEXT,
    PaymentStatus TEXT DEFAULT 'غير مدفوع',
    Notes TEXT,
    CreatedBy INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
);

CREATE TABLE IF NOT EXISTS InvoiceItems (
    ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceID INTEGER NOT NULL,
    Description TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice REAL NOT NULL,
    TotalPrice REAL NOT NULL,
    FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID) ON DELETE CASCADE
);

-- ====================================
-- 10. Payments - المدفوعات
-- ====================================
CREATE TABLE IF NOT EXISTS Payments (
    PaymentID INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceID INTEGER NOT NULL,
    PaymentDate TEXT DEFAULT (datetime('now', 'localtime')),
    Amount REAL NOT NULL,
    PaymentMethod TEXT NOT NULL,
    Reference TEXT,
    Notes TEXT,
    ReceivedBy INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID) ON DELETE CASCADE,
    FOREIGN KEY (ReceivedBy) REFERENCES Users(UserID)
);

-- ====================================
-- 11. Inventory - المخزون
-- ====================================
CREATE TABLE IF NOT EXISTS Inventory (
    ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemCode TEXT NOT NULL UNIQUE,
    ItemName TEXT NOT NULL,
    Category TEXT NOT NULL,
    Description TEXT,
    Unit TEXT,
    Quantity INTEGER NOT NULL DEFAULT 0,
    MinimumStock INTEGER DEFAULT 10,
    UnitPrice REAL NOT NULL,
    SupplierName TEXT,
    SupplierPhone TEXT,
    ExpiryDate TEXT,
    Location TEXT,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now', 'localtime')),
    UpdatedAt TEXT DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE IF NOT EXISTS InventoryTransactions (
    TransactionID INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemID INTEGER NOT NULL,
    TransactionType TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    TransactionDate TEXT DEFAULT (datetime('now', 'localtime')),
    Reference TEXT,
    Notes TEXT,
    UserID INTEGER,
    FOREIGN KEY (ItemID) REFERENCES Inventory(ItemID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- ====================================
-- 12. Clinic Settings - إعدادات العيادة
-- ====================================
CREATE TABLE IF NOT EXISTS ClinicSettings (
    SettingID INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingKey TEXT NOT NULL UNIQUE,
    SettingValue TEXT,
    UpdatedAt TEXT DEFAULT (datetime('now', 'localtime'))
);

-- ====================================
-- 13. Audit Log - سجل النشاطات
-- ====================================
CREATE TABLE IF NOT EXISTS AuditLog (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID INTEGER,
    Action TEXT NOT NULL,
    TableName TEXT,
    RecordID INTEGER,
    Details TEXT,
    Timestamp TEXT DEFAULT (datetime('now', 'localtime')),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- ====================================
-- Indexes
-- ====================================
CREATE INDEX IF NOT EXISTS idx_patients_code ON Patients(PatientCode);
CREATE INDEX IF NOT EXISTS idx_patients_phone ON Patients(PhoneNumber);
CREATE INDEX IF NOT EXISTS idx_patients_national ON Patients(NationalID);
CREATE INDEX IF NOT EXISTS idx_visits_patient ON Visits(PatientID);
CREATE INDEX IF NOT EXISTS idx_visits_date ON Visits(VisitDate);
CREATE INDEX IF NOT EXISTS idx_appointments_date ON Appointments(AppointmentDate);
CREATE INDEX IF NOT EXISTS idx_invoices_patient ON Invoices(PatientID);
CREATE INDEX IF NOT EXISTS idx_invoices_date ON Invoices(InvoiceDate);
CREATE INDEX IF NOT EXISTS idx_inventory_code ON Inventory(ItemCode);

-- ====================================
-- Initial Data
-- ====================================
INSERT OR IGNORE INTO Users (Username, Password, FullName, Role, IsActive)
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'المدير العام', 'مدير', 1);

INSERT OR IGNORE INTO ClinicSettings (SettingKey, SettingValue) VALUES
('ClinicName', 'عيادة الشفاء'),
('ClinicPhone', '0123456789'),
('ClinicAddress', 'القاهرة، مصر'),
('ExaminationFee', '200'),
('ReexaminationFee', '100'),
('Currency', 'جنيه'),
('WorkingHoursStart', '09:00'),
('WorkingHoursEnd', '18:00');

-- ====================================
-- Views - طرق العرض
-- ====================================

-- عرض المرضى النشطين مع عدد الزيارات
CREATE VIEW IF NOT EXISTS v_ActivePatients AS
SELECT 
    p.*,
    COUNT(v.VisitID) as TotalVisits,
    MAX(v.VisitDate) as LastVisit
FROM Patients p
LEFT JOIN Visits v ON p.PatientID = v.PatientID
WHERE p.IsActive = 1
GROUP BY p.PatientID;

-- عرض الفواتير مع بيانات المريض
CREATE VIEW IF NOT EXISTS v_InvoicesWithPatients AS
SELECT 
    i.*,
    p.PatientCode,
    p.FirstName || ' ' || p.LastName as PatientName,
    p.PhoneNumber
FROM Invoices i
INNER JOIN Patients p ON i.PatientID = p.PatientID;

-- عرض المخزون المنخفض
CREATE VIEW IF NOT EXISTS v_LowStockItems AS
SELECT *
FROM Inventory
WHERE Quantity <= MinimumStock 
  AND IsActive = 1;

-- عرض المواعيد مع بيانات المريض
CREATE VIEW IF NOT EXISTS v_AppointmentsWithPatients AS
SELECT 
    a.*,
    p.PatientCode,
    p.FirstName || ' ' || p.LastName as PatientName,
    p.PhoneNumber,
    u.FullName as DoctorName
FROM Appointments a
INNER JOIN Patients p ON a.PatientID = p.PatientID
LEFT JOIN Users u ON a.DoctorID = u.UserID;

-- عرض الزيارات مع بيانات المريض
CREATE VIEW IF NOT EXISTS v_VisitsWithPatients AS
SELECT 
    v.*,
    p.PatientCode,
    p.FirstName || ' ' || p.LastName as PatientName,
    p.PhoneNumber,
    u.FullName as DoctorName
FROM Visits v
INNER JOIN Patients p ON v.PatientID = p.PatientID
LEFT JOIN Users u ON v.DoctorID = u.UserID;

-- ====================================
-- Triggers
-- ====================================
CREATE TRIGGER IF NOT EXISTS update_patients_timestamp 
AFTER UPDATE ON Patients
BEGIN
    UPDATE Patients SET UpdatedAt = datetime('now', 'localtime')
    WHERE PatientID = NEW.PatientID;
END;

CREATE TRIGGER IF NOT EXISTS update_inventory_timestamp 
AFTER UPDATE ON Inventory
BEGIN
    UPDATE Inventory SET UpdatedAt = datetime('now', 'localtime')
    WHERE ItemID = NEW.ItemID;
END;

CREATE TRIGGER IF NOT EXISTS update_invoice_payment_status
AFTER INSERT ON Payments
BEGIN
    UPDATE Invoices 
    SET 
        PaidAmount = (SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE InvoiceID = NEW.InvoiceID),
        RemainingAmount = NetAmount - (SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE InvoiceID = NEW.InvoiceID),
        PaymentStatus = CASE
            WHEN (SELECT SUM(Amount) FROM Payments WHERE InvoiceID = NEW.InvoiceID) >= NetAmount THEN 'مدفوع'
            WHEN (SELECT SUM(Amount) FROM Payments WHERE InvoiceID = NEW.InvoiceID) > 0 THEN 'مدفوع جزئياً'
            ELSE 'غير مدفوع'
        END
    WHERE InvoiceID = NEW.InvoiceID;
END;
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