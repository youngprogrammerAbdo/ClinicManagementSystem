// ====================================
// Patient Repository - CRUD Operations
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicManagementSystem.Repositories
{
    public class PatientRepository
    {
        private readonly DatabaseHelper _db;

        public PatientRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Create
        // ====================================
        public int AddPatient(Patient patient)
        {
            string query = @"
                INSERT INTO Patients (
                    PatientCode, FirstName, LastName, DateOfBirth, Gender,
                    PhoneNumber, PhoneNumber2, Address, NationalID, BloodType,
                    Email, EmergencyContact, EmergencyPhone, Notes, 
                    ProfileImagePath, RegistrationDate, IsActive
                ) VALUES (
                    @PatientCode, @FirstName, @LastName, @DateOfBirth, @Gender,
                    @PhoneNumber, @PhoneNumber2, @Address, @NationalID, @BloodType,
                    @Email, @EmergencyContact, @EmergencyPhone, @Notes,
                    @ProfileImagePath, @RegistrationDate, @IsActive
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@PatientCode", patient.PatientCode ?? GeneratePatientCode()),
                new SQLiteParameter("@FirstName", patient.FirstName),
                new SQLiteParameter("@LastName", patient.LastName),
                new SQLiteParameter("@DateOfBirth", patient.DateOfBirth.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@Gender", patient.Gender),
                new SQLiteParameter("@PhoneNumber", patient.PhoneNumber),
                new SQLiteParameter("@PhoneNumber2", (object)patient.PhoneNumber2 ?? DBNull.Value),
                new SQLiteParameter("@Address", (object)patient.Address ?? DBNull.Value),
                new SQLiteParameter("@NationalID", (object)patient.NationalID ?? DBNull.Value),
                new SQLiteParameter("@BloodType", (object)patient.BloodType ?? DBNull.Value),
                new SQLiteParameter("@Email", (object)patient.Email ?? DBNull.Value),
                new SQLiteParameter("@EmergencyContact", (object)patient.EmergencyContact ?? DBNull.Value),
                new SQLiteParameter("@EmergencyPhone", (object)patient.EmergencyPhone ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)patient.Notes ?? DBNull.Value),
                new SQLiteParameter("@ProfileImagePath", (object)patient.ProfileImagePath ?? DBNull.Value),
                new SQLiteParameter("@RegistrationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@IsActive", patient.IsActive ? 1 : 0)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        // ====================================
        // Read
        // ====================================
        public Patient GetPatientById(int patientId)
        {
            string query = "SELECT * FROM Patients WHERE PatientID = @PatientID";
            var parameter = new SQLiteParameter("@PatientID", patientId);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToPatient(table.Rows[0]);
            }

            return null;
        }

        public Patient GetPatientByCode(string patientCode)
        {
            string query = "SELECT * FROM Patients WHERE PatientCode = @PatientCode";
            var parameter = new SQLiteParameter("@PatientCode", patientCode);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToPatient(table.Rows[0]);
            }

            return null;
        }

        public List<Patient> GetAllPatients(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM Patients WHERE IsActive = 1 ORDER BY RegistrationDate DESC"
                : "SELECT * FROM Patients ORDER BY RegistrationDate DESC";

            var table = _db.ExecuteQuery(query);
            var patients = new List<Patient>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                patients.Add(MapToPatient(row));
            }

            return patients;
        }

        public List<Patient> SearchPatients(string searchTerm)
        {
            string query = @"
                SELECT * FROM Patients 
                WHERE (FirstName LIKE @SearchTerm 
                    OR LastName LIKE @SearchTerm 
                    OR PatientCode LIKE @SearchTerm 
                    OR PhoneNumber LIKE @SearchTerm
                    OR NationalID LIKE @SearchTerm)
                AND IsActive = 1
                ORDER BY RegistrationDate DESC";

            var parameter = new SQLiteParameter("@SearchTerm", $"%{searchTerm}%");
            var table = _db.ExecuteQuery(query, parameter);
            var patients = new List<Patient>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                patients.Add(MapToPatient(row));
            }

            return patients;
        }

        // ====================================
        // Update
        // ====================================
        public bool UpdatePatient(Patient patient)
        {
            string query = @"
                UPDATE Patients SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    DateOfBirth = @DateOfBirth,
                    Gender = @Gender,
                    PhoneNumber = @PhoneNumber,
                    PhoneNumber2 = @PhoneNumber2,
                    Address = @Address,
                    NationalID = @NationalID,
                    BloodType = @BloodType,
                    Email = @Email,
                    EmergencyContact = @EmergencyContact,
                    EmergencyPhone = @EmergencyPhone,
                    Notes = @Notes,
                    ProfileImagePath = @ProfileImagePath,
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt
                WHERE PatientID = @PatientID";

            var parameters = new[]
            {
                new SQLiteParameter("@FirstName", patient.FirstName),
                new SQLiteParameter("@LastName", patient.LastName),
                new SQLiteParameter("@DateOfBirth", patient.DateOfBirth.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@Gender", patient.Gender),
                new SQLiteParameter("@PhoneNumber", patient.PhoneNumber),
                new SQLiteParameter("@PhoneNumber2", (object)patient.PhoneNumber2 ?? DBNull.Value),
                new SQLiteParameter("@Address", (object)patient.Address ?? DBNull.Value),
                new SQLiteParameter("@NationalID", (object)patient.NationalID ?? DBNull.Value),
                new SQLiteParameter("@BloodType", (object)patient.BloodType ?? DBNull.Value),
                new SQLiteParameter("@Email", (object)patient.Email ?? DBNull.Value),
                new SQLiteParameter("@EmergencyContact", (object)patient.EmergencyContact ?? DBNull.Value),
                new SQLiteParameter("@EmergencyPhone", (object)patient.EmergencyPhone ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)patient.Notes ?? DBNull.Value),
                new SQLiteParameter("@ProfileImagePath", (object)patient.ProfileImagePath ?? DBNull.Value),
                new SQLiteParameter("@IsActive", patient.IsActive ? 1 : 0),
                new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@PatientID", patient.PatientID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // Delete (Soft Delete)
        // ====================================
        public bool DeletePatient(int patientId)
        {
            string query = "UPDATE Patients SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE PatientID = @PatientID";
            var parameters = new[]
            {
                new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@PatientID", patientId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool HardDeletePatient(int patientId)
        {
            string query = "DELETE FROM Patients WHERE PatientID = @PatientID";
            var parameter = new SQLiteParameter("@PatientID", patientId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        // ====================================
        // Restore Patient
        // ====================================
        public bool RestorePatient(int patientId)
        {
            string query = "UPDATE Patients SET IsActive = 1, UpdatedAt = @UpdatedAt WHERE PatientID = @PatientID";
            var parameters = new[]
            {
                new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@PatientID", patientId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // Medical History
        // ====================================
        public int AddOrUpdateMedicalHistory(MedicalHistory history)
        {
            // تحقق إذا كان موجود
            string checkQuery = "SELECT COUNT(*) FROM MedicalHistory WHERE PatientID = @PatientID";
            var checkParam = new SQLiteParameter("@PatientID", history.PatientID);
            var count = Convert.ToInt32(_db.ExecuteScalar(checkQuery, checkParam));

            if (count > 0)
            {
                // Update
                string updateQuery = @"
                    UPDATE MedicalHistory SET
                        ChronicDiseases = @ChronicDiseases,
                        Allergies = @Allergies,
                        CurrentMedications = @CurrentMedications,
                        PreviousSurgeries = @PreviousSurgeries,
                        FamilyHistory = @FamilyHistory,
                        Notes = @Notes,
                        UpdatedAt = @UpdatedAt
                    WHERE PatientID = @PatientID";

                var updateParams = new[]
                {
                    new SQLiteParameter("@ChronicDiseases", (object)history.ChronicDiseases ?? DBNull.Value),
                    new SQLiteParameter("@Allergies", (object)history.Allergies ?? DBNull.Value),
                    new SQLiteParameter("@CurrentMedications", (object)history.CurrentMedications ?? DBNull.Value),
                    new SQLiteParameter("@PreviousSurgeries", (object)history.PreviousSurgeries ?? DBNull.Value),
                    new SQLiteParameter("@FamilyHistory", (object)history.FamilyHistory ?? DBNull.Value),
                    new SQLiteParameter("@Notes", (object)history.Notes ?? DBNull.Value),
                    new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SQLiteParameter("@PatientID", history.PatientID)
                };
                
                _db.ExecuteNonQuery(updateQuery, updateParams);
                return history.PatientID;
            }
            else
            {
                // Insert
                string insertQuery = @"
                    INSERT INTO MedicalHistory (
                        PatientID, ChronicDiseases, Allergies, CurrentMedications,
                        PreviousSurgeries, FamilyHistory, Notes, CreatedAt
                    ) VALUES (
                        @PatientID, @ChronicDiseases, @Allergies, @CurrentMedications,
                        @PreviousSurgeries, @FamilyHistory, @Notes, @CreatedAt
                    );
                    SELECT last_insert_rowid();";

                var insertParams = new[]
                {
                    new SQLiteParameter("@PatientID", history.PatientID),
                    new SQLiteParameter("@ChronicDiseases", (object)history.ChronicDiseases ?? DBNull.Value),
                    new SQLiteParameter("@Allergies", (object)history.Allergies ?? DBNull.Value),
                    new SQLiteParameter("@CurrentMedications", (object)history.CurrentMedications ?? DBNull.Value),
                    new SQLiteParameter("@PreviousSurgeries", (object)history.PreviousSurgeries ?? DBNull.Value),
                    new SQLiteParameter("@FamilyHistory", (object)history.FamilyHistory ?? DBNull.Value),
                    new SQLiteParameter("@Notes", (object)history.Notes ?? DBNull.Value),
                    new SQLiteParameter("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                var result = _db.ExecuteScalar(insertQuery, insertParams);
                return Convert.ToInt32(result);
            }
        }

        public MedicalHistory GetMedicalHistory(int patientId)
        {
            string query = "SELECT * FROM MedicalHistory WHERE PatientID = @PatientID";
            var parameter = new SQLiteParameter("@PatientID", patientId);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                var row = table.Rows[0];
                return new MedicalHistory
                {
                    HistoryID = Convert.ToInt32(row["HistoryID"]),
                    PatientID = Convert.ToInt32(row["PatientID"]),
                    ChronicDiseases = row["ChronicDiseases"]?.ToString(),
                    Allergies = row["Allergies"]?.ToString(),
                    CurrentMedications = row["CurrentMedications"]?.ToString(),
                    PreviousSurgeries = row["PreviousSurgeries"]?.ToString(),
                    FamilyHistory = row["FamilyHistory"]?.ToString(),
                    Notes = row["Notes"]?.ToString()
                };
            }

            return null;
        }

        // ====================================
        // Statistics
        // ====================================
        public int GetTotalPatientsCount()
        {
            string query = "SELECT COUNT(*) FROM Patients WHERE IsActive = 1";
            return Convert.ToInt32(_db.ExecuteScalar(query));
        }

        public int GetNewPatientsCount(DateTime fromDate)
        {
            string query = "SELECT COUNT(*) FROM Patients WHERE RegistrationDate >= @FromDate AND IsActive = 1";
            var parameter = new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(_db.ExecuteScalar(query, parameter));
        }

        public int GetNewPatientsCountByDateRange(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT COUNT(*) FROM Patients 
                WHERE RegistrationDate >= @FromDate 
                AND RegistrationDate <= @ToDate 
                AND IsActive = 1";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd"))
            };

            return Convert.ToInt32(_db.ExecuteScalar(query, parameters));
        }

        public List<Patient> GetPatientsWithDebts()
        {
            string query = @"
                SELECT DISTINCT p.* 
                FROM Patients p
                INNER JOIN Invoices i ON p.PatientID = i.PatientID
                WHERE i.RemainingAmount > 0 AND p.IsActive = 1
                ORDER BY i.RemainingAmount DESC";

            var table = _db.ExecuteQuery(query);
            var patients = new List<Patient>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                patients.Add(MapToPatient(row));
            }

            return patients;
        }

        public Dictionary<string, int> GetPatientsByGender()
        {
            string query = @"
                SELECT Gender, COUNT(*) as Count 
                FROM Patients 
                WHERE IsActive = 1 
                GROUP BY Gender";

            var table = _db.ExecuteQuery(query);
            var result = new Dictionary<string, int>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                result[row["Gender"].ToString()] = Convert.ToInt32(row["Count"]);
            }

            return result;
        }

        public Dictionary<string, int> GetPatientsByAgeGroup()
        {
            string query = @"
                SELECT 
                    CASE 
                        WHEN (julianday('now') - julianday(DateOfBirth)) / 365 < 18 THEN 'أقل من 18'
                        WHEN (julianday('now') - julianday(DateOfBirth)) / 365 BETWEEN 18 AND 35 THEN '18-35'
                        WHEN (julianday('now') - julianday(DateOfBirth)) / 365 BETWEEN 36 AND 50 THEN '36-50'
                        WHEN (julianday('now') - julianday(DateOfBirth)) / 365 BETWEEN 51 AND 65 THEN '51-65'
                        ELSE 'أكبر من 65'
                    END as AgeGroup,
                    COUNT(*) as Count
                FROM Patients 
                WHERE IsActive = 1
                GROUP BY AgeGroup
                ORDER BY 
                    CASE AgeGroup
                        WHEN 'أقل من 18' THEN 1
                        WHEN '18-35' THEN 2
                        WHEN '36-50' THEN 3
                        WHEN '51-65' THEN 4
                        ELSE 5
                    END";

            var table = _db.ExecuteQuery(query);
            var result = new Dictionary<string, int>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                result[row["AgeGroup"].ToString()] = Convert.ToInt32(row["Count"]);
            }

            return result;
        }

        // ====================================
        // Advanced Search with Filters
        // ====================================
        public List<Patient> AdvancedSearch(string name = null, string phone = null,
            string nationalId = null, DateTime? fromDate = null, DateTime? toDate = null,
            string gender = null, string bloodType = null)
        {
            var query = "SELECT * FROM Patients WHERE IsActive = 1";
            var parameters = new List<SQLiteParameter>();

            if (!string.IsNullOrEmpty(name))
            {
                query += " AND (FirstName LIKE @Name OR LastName LIKE @Name)";
                parameters.Add(new SQLiteParameter("@Name", $"%{name}%"));
            }

            if (!string.IsNullOrEmpty(phone))
            {
                query += " AND (PhoneNumber LIKE @Phone OR PhoneNumber2 LIKE @Phone)";
                parameters.Add(new SQLiteParameter("@Phone", $"%{phone}%"));
            }

            if (!string.IsNullOrEmpty(nationalId))
            {
                query += " AND NationalID LIKE @NationalID";
                parameters.Add(new SQLiteParameter("@NationalID", $"%{nationalId}%"));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query += " AND Gender = @Gender";
                parameters.Add(new SQLiteParameter("@Gender", gender));
            }

            if (!string.IsNullOrEmpty(bloodType))
            {
                query += " AND BloodType = @BloodType";
                parameters.Add(new SQLiteParameter("@BloodType", bloodType));
            }

            if (fromDate.HasValue)
            {
                query += " AND RegistrationDate >= @FromDate";
                parameters.Add(new SQLiteParameter("@FromDate", fromDate.Value.ToString("yyyy-MM-dd")));
            }

            if (toDate.HasValue)
            {
                query += " AND RegistrationDate <= @ToDate";
                parameters.Add(new SQLiteParameter("@ToDate", toDate.Value.ToString("yyyy-MM-dd")));
            }

            query += " ORDER BY RegistrationDate DESC";

            var table = _db.ExecuteQuery(query, parameters.ToArray());
            var patients = new List<Patient>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                patients.Add(MapToPatient(row));
            }

            return patients;
        }

        // ====================================
        // Get Recent Patients
        // ====================================
        public List<Patient> GetRecentPatients(int count = 10)
        {
            string query = @"
                SELECT * FROM Patients 
                WHERE IsActive = 1 
                ORDER BY RegistrationDate DESC 
                LIMIT @Count";

            var parameter = new SQLiteParameter("@Count", count);
            var table = _db.ExecuteQuery(query, parameter);
            var patients = new List<Patient>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                patients.Add(MapToPatient(row));
            }

            return patients;
        }

        // ====================================
        // Check if Patient Code Exists
        // ====================================
        public bool PatientCodeExists(string patientCode, int? excludePatientId = null)
        {
            string query = "SELECT COUNT(*) FROM Patients WHERE PatientCode = @PatientCode";
            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@PatientCode", patientCode)
            };

            if (excludePatientId.HasValue)
            {
                query += " AND PatientID != @PatientID";
                parameters.Add(new SQLiteParameter("@PatientID", excludePatientId.Value));
            }

            var count = Convert.ToInt32(_db.ExecuteScalar(query, parameters.ToArray()));
            return count > 0;
        }

        // ====================================
        // Helper Methods
        // ====================================
        private Patient MapToPatient(System.Data.DataRow row)
        {
            return new Patient
            {
                PatientID = Convert.ToInt32(row["PatientID"]),
                PatientCode = row["PatientCode"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                Gender = row["Gender"].ToString(),
                PhoneNumber = row["PhoneNumber"].ToString(),
                PhoneNumber2 = row["PhoneNumber2"]?.ToString(),
                Address = row["Address"]?.ToString(),
                NationalID = row["NationalID"]?.ToString(),
                BloodType = row["BloodType"]?.ToString(),
                Email = row["Email"]?.ToString(),
                EmergencyContact = row["EmergencyContact"]?.ToString(),
                EmergencyPhone = row["EmergencyPhone"]?.ToString(),
                Notes = row["Notes"]?.ToString(),
                ProfileImagePath = row["ProfileImagePath"]?.ToString(),
                RegistrationDate = Convert.ToDateTime(row["RegistrationDate"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }

        private string GeneratePatientCode()
        {
            string query = "SELECT COUNT(*) FROM Patients";
            int count = Convert.ToInt32(_db.ExecuteScalar(query));
            return $"P{DateTime.Now:yyyyMMdd}{(count + 1):D4}";
        }

        // ====================================
        // Bulk Operations
        // ====================================
        public bool BulkDeletePatients(List<int> patientIds)
        {
            if (patientIds == null || patientIds.Count == 0)
                return false;

            string ids = string.Join(",", patientIds);
            string query = $"UPDATE Patients SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE PatientID IN ({ids})";
            var parameter = new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        public bool BulkRestorePatients(List<int> patientIds)
        {
            if (patientIds == null || patientIds.Count == 0)
                return false;

            string ids = string.Join(",", patientIds);
            string query = $"UPDATE Patients SET IsActive = 1, UpdatedAt = @UpdatedAt WHERE PatientID IN ({ids})";
            var parameter = new SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }
    }
}