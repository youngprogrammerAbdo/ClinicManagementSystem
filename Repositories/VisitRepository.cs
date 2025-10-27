// ====================================
// Visit Repository - إدارة الزيارات والكشوفات
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClinicManagementSystem.Repositories
{
    public class VisitRepository
    {
        private readonly DatabaseHelper _db;

        public VisitRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Create Visit
        // ====================================
        public int AddVisit(Visit visit)
        {
            // الحصول على رقم الدور التالي
            if (visit.QueueNumber == null || visit.QueueNumber == 0)
            {
                visit.QueueNumber = GetNextQueueNumber();
            }

            string query = @"
                INSERT INTO Visits (
                    PatientID, VisitDate, VisitType, ChiefComplaint,
                    Diagnosis, Treatment, Notes, DoctorID, VisitStatus,
                    ExaminationFee, IsPaid, QueueNumber
                ) VALUES (
                    @PatientID, @VisitDate, @VisitType, @ChiefComplaint,
                    @Diagnosis, @Treatment, @Notes, @DoctorID, @VisitStatus,
                    @ExaminationFee, @IsPaid, @QueueNumber
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@PatientID", visit.PatientID),
                new SQLiteParameter("@VisitDate", visit.VisitDate.ToString("yyyy-MM-dd HH:mm:ss")),
                new SQLiteParameter("@VisitType", visit.VisitType),
                new SQLiteParameter("@ChiefComplaint", (object)visit.ChiefComplaint ?? DBNull.Value),
                new SQLiteParameter("@Diagnosis", (object)visit.Diagnosis ?? DBNull.Value),
                new SQLiteParameter("@Treatment", (object)visit.Treatment ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)visit.Notes ?? DBNull.Value),
                new SQLiteParameter("@DoctorID", (object)visit.DoctorID ?? DBNull.Value),
                new SQLiteParameter("@VisitStatus", visit.VisitStatus),
                new SQLiteParameter("@ExaminationFee", visit.ExaminationFee),
                new SQLiteParameter("@IsPaid", visit.IsPaid ? 1 : 0),
                new SQLiteParameter("@QueueNumber", visit.QueueNumber)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        // ====================================
        // Read Visits
        // ====================================
        public Visit GetVisitById(int visitId)
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       u.FullName as DoctorName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                LEFT JOIN Users u ON v.DoctorID = u.UserID
                WHERE v.VisitID = @VisitID";

            var parameter = new SQLiteParameter("@VisitID", visitId);
            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToVisit(table.Rows[0]);
            }

            return null;
        }

        public List<Visit> GetPatientVisits(int patientId)
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       u.FullName as DoctorName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                LEFT JOIN Users u ON v.DoctorID = u.UserID
                WHERE v.PatientID = @PatientID
                ORDER BY v.VisitDate DESC";

            var parameter = new SQLiteParameter("@PatientID", patientId);
            var table = _db.ExecuteQuery(query, parameter);
            var visits = new List<Visit>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                visits.Add(MapToVisit(row));
            }

            return visits;
        }

        public List<Visit> GetTodayVisits()
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       u.FullName as DoctorName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                LEFT JOIN Users u ON v.DoctorID = u.UserID
                WHERE DATE(v.VisitDate) = DATE('now')
                ORDER BY v.QueueNumber";

            var table = _db.ExecuteQuery(query);
            var visits = new List<Visit>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                visits.Add(MapToVisit(row));
            }

            return visits;
        }

        public List<Visit> GetVisitsByDateRange(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       u.FullName as DoctorName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                LEFT JOIN Users u ON v.DoctorID = u.UserID
                WHERE v.VisitDate BETWEEN @FromDate AND @ToDate
                ORDER BY v.VisitDate DESC";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd 00:00:00")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var visits = new List<Visit>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                visits.Add(MapToVisit(row));
            }

            return visits;
        }

        // ====================================
        // Update Visit
        // ====================================
        public bool UpdateVisit(Visit visit)
        {
            string query = @"
                UPDATE Visits SET
                    VisitType = @VisitType,
                    ChiefComplaint = @ChiefComplaint,
                    Diagnosis = @Diagnosis,
                    Treatment = @Treatment,
                    Notes = @Notes,
                    DoctorID = @DoctorID,
                    VisitStatus = @VisitStatus,
                    ExaminationFee = @ExaminationFee,
                    IsPaid = @IsPaid
                WHERE VisitID = @VisitID";

            var parameters = new[]
            {
                new SQLiteParameter("@VisitType", visit.VisitType),
                new SQLiteParameter("@ChiefComplaint", (object)visit.ChiefComplaint ?? DBNull.Value),
                new SQLiteParameter("@Diagnosis", (object)visit.Diagnosis ?? DBNull.Value),
                new SQLiteParameter("@Treatment", (object)visit.Treatment ?? DBNull.Value),
                new SQLiteParameter("@Notes", (object)visit.Notes ?? DBNull.Value),
                new SQLiteParameter("@DoctorID", (object)visit.DoctorID ?? DBNull.Value),
                new SQLiteParameter("@VisitStatus", visit.VisitStatus),
                new SQLiteParameter("@ExaminationFee", visit.ExaminationFee),
                new SQLiteParameter("@IsPaid", visit.IsPaid ? 1 : 0),
                new SQLiteParameter("@VisitID", visit.VisitID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool UpdateVisitStatus(int visitId, string status)
        {
            string query = "UPDATE Visits SET VisitStatus = @Status WHERE VisitID = @VisitID";
            var parameters = new[]
            {
                new SQLiteParameter("@Status", status),
                new SQLiteParameter("@VisitID", visitId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // Delete Visit
        // ====================================
        public bool DeleteVisit(int visitId)
        {
            string query = "DELETE FROM Visits WHERE VisitID = @VisitID";
            var parameter = new SQLiteParameter("@VisitID", visitId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        // ====================================
        // Queue Management
        // ====================================
        public int GetNextQueueNumber()
        {
            string query = "SELECT COALESCE(MAX(QueueNumber), 0) + 1 FROM Visits WHERE DATE(VisitDate) = DATE('now')";
            var result = _db.ExecuteScalar(query);
            return Convert.ToInt32(result);
        }

        public List<Visit> GetQueueByStatus(string status)
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                WHERE DATE(v.VisitDate) = DATE('now') 
                  AND v.VisitStatus = @Status
                ORDER BY v.QueueNumber";

            var parameter = new SQLiteParameter("@Status", status);
            var table = _db.ExecuteQuery(query, parameter);
            var visits = new List<Visit>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                visits.Add(MapToVisit(row));
            }

            return visits;
        }

        public Visit GetCurrentPatientInQueue()
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                WHERE DATE(v.VisitDate) = DATE('now') 
                  AND v.VisitStatus = 'جاري الكشف'
                LIMIT 1";

            var table = _db.ExecuteQuery(query);

            if (table.Rows.Count > 0)
            {
                return MapToVisit(table.Rows[0]);
            }

            return null;
        }

        public Visit GetNextPatientInQueue()
        {
            string query = @"
                SELECT v.*, 
                       p.FirstName || ' ' || p.LastName as PatientName
                FROM Visits v
                INNER JOIN Patients p ON v.PatientID = p.PatientID
                WHERE DATE(v.VisitDate) = DATE('now') 
                  AND v.VisitStatus = 'منتظر'
                ORDER BY v.QueueNumber
                LIMIT 1";

            var table = _db.ExecuteQuery(query);

            if (table.Rows.Count > 0)
            {
                return MapToVisit(table.Rows[0]);
            }

            return null;
        }

        // ====================================
        // Statistics
        // ====================================
        public int GetTodayVisitsCount()
        {
            string query = "SELECT COUNT(*) FROM Visits WHERE DATE(VisitDate) = DATE('now')";
            return Convert.ToInt32(_db.ExecuteScalar(query));
        }

        public int GetVisitsCountByDateRange(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Visits 
                WHERE VisitDate BETWEEN @FromDate AND @ToDate";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd 00:00:00")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            return Convert.ToInt32(_db.ExecuteScalar(query, parameters));
        }

        public Dictionary<string, int> GetVisitTypeStatistics(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT VisitType, COUNT(*) as Count
                FROM Visits
                WHERE VisitDate BETWEEN @FromDate AND @ToDate
                GROUP BY VisitType";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd 00:00:00")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var stats = new Dictionary<string, int>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                stats[row["VisitType"].ToString()] = Convert.ToInt32(row["Count"]);
            }

            return stats;
        }

        // ====================================
        // Helper Methods
        // ====================================
        private Visit MapToVisit(System.Data.DataRow row)
        {
            var visit = new Visit
            {
                VisitID = Convert.ToInt32(row["VisitID"]),
                PatientID = Convert.ToInt32(row["PatientID"]),
                VisitDate = Convert.ToDateTime(row["VisitDate"]),
                VisitType = row["VisitType"].ToString(),
                ChiefComplaint = row["ChiefComplaint"]?.ToString(),
                Diagnosis = row["Diagnosis"]?.ToString(),
                Treatment = row["Treatment"]?.ToString(),
                Notes = row["Notes"]?.ToString(),
                DoctorID = row["DoctorID"] != DBNull.Value ? Convert.ToInt32(row["DoctorID"]) : (int?)null,
                VisitStatus = row["VisitStatus"].ToString(),
                ExaminationFee = Convert.ToDecimal(row["ExaminationFee"]),
                IsPaid = Convert.ToBoolean(row["IsPaid"]),
                QueueNumber = row["QueueNumber"] != DBNull.Value ? Convert.ToInt32(row["QueueNumber"]) : (int?)null
            };

            // إضافة اسم المريض إذا كان متاحاً
            if (row.Table.Columns.Contains("PatientName"))
            {
                visit.Patient = new Patient { FullName = row["PatientName"].ToString() };
            }

            return visit;
        }
    }
}