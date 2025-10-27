// ====================================
// Appointment Repository - إدارة المواعيد
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClinicManagementSystem.Repositories
{
    public class AppointmentRepository
    {
        private readonly DatabaseHelper _db;

        public AppointmentRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Create Appointment
        // ====================================
        public int AddAppointment(Appointment appointment)
        {
            string query = @"
                INSERT INTO Appointments (
                    PatientID, AppointmentDate, AppointmentTime, DoctorID,
                    AppointmentType, Status, Notes, ReminderSent, CreatedBy
                ) VALUES (
                    @PatientID, @AppointmentDate, @AppointmentTime, @DoctorID,
                    @AppointmentType, @Status, @Notes, @ReminderSent, @CreatedBy
                );
                SELECT last_insert_rowid();";

            var parameters = new[]
            {
                new SQLiteParameter("@PatientID", appointment.PatientID),
                new SQLiteParameter("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@AppointmentTime", appointment.AppointmentTime.ToString()),
                new SQLiteParameter("@DoctorID", (object)appointment.DoctorID ?? DBNull.Value),
                new SQLiteParameter("@AppointmentType", appointment.AppointmentType),
                new SQLiteParameter("@Status", appointment.Status),
                new SQLiteParameter("@Notes", (object)appointment.Notes ?? DBNull.Value),
                new SQLiteParameter("@ReminderSent", appointment.ReminderSent ? 1 : 0),
                new SQLiteParameter("@CreatedBy", (object)null ?? DBNull.Value)
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result);
        }

        // ====================================
        // Read Appointments
        // ====================================
        public Appointment GetAppointmentById(int appointmentId)
        {
            string query = @"
                SELECT a.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       p.PhoneNumber,
                       u.FullName as DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                LEFT JOIN Users u ON a.DoctorID = u.UserID
                WHERE a.AppointmentID = @AppointmentID";

            var parameter = new SQLiteParameter("@AppointmentID", appointmentId);
            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                return MapToAppointment(table.Rows[0]);
            }

            return null;
        }

        public List<Appointment> GetAppointmentsByDate(DateTime date)
        {
            string query = @"
                SELECT a.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       p.PhoneNumber,
                       u.FullName as DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                LEFT JOIN Users u ON a.DoctorID = u.UserID
                WHERE DATE(a.AppointmentDate) = DATE(@Date)
                ORDER BY a.AppointmentTime";

            var parameter = new SQLiteParameter("@Date", date.ToString("yyyy-MM-dd"));
            var table = _db.ExecuteQuery(query, parameter);
            var appointments = new List<Appointment>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                appointments.Add(MapToAppointment(row));
            }

            return appointments;
        }

        public List<Appointment> GetTodayAppointments()
        {
            return GetAppointmentsByDate(DateTime.Today);
        }

        public List<Appointment> GetPatientAppointments(int patientId)
        {
            string query = @"
                SELECT a.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       p.PhoneNumber,
                       u.FullName as DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                LEFT JOIN Users u ON a.DoctorID = u.UserID
                WHERE a.PatientID = @PatientID
                ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC";

            var parameter = new SQLiteParameter("@PatientID", patientId);
            var table = _db.ExecuteQuery(query, parameter);
            var appointments = new List<Appointment>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                appointments.Add(MapToAppointment(row));
            }

            return appointments;
        }

        public List<Appointment> GetUpcomingAppointments(int days = 7)
        {
            string query = @"
                SELECT a.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       p.PhoneNumber,
                       u.FullName as DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                LEFT JOIN Users u ON a.DoctorID = u.UserID
                WHERE DATE(a.AppointmentDate) BETWEEN DATE('now') AND DATE('now', '+' || @Days || ' days')
                  AND a.Status = 'محجوز'
                ORDER BY a.AppointmentDate, a.AppointmentTime";

            var parameter = new SQLiteParameter("@Days", days);
            var table = _db.ExecuteQuery(query, parameter);
            var appointments = new List<Appointment>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                appointments.Add(MapToAppointment(row));
            }

            return appointments;
        }

        public List<Appointment> GetAppointmentsByDateRange(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT a.*, 
                       p.FirstName || ' ' || p.LastName as PatientName,
                       p.PhoneNumber,
                       u.FullName as DoctorName
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientID = p.PatientID
                LEFT JOIN Users u ON a.DoctorID = u.UserID
                WHERE a.AppointmentDate BETWEEN @FromDate AND @ToDate
                ORDER BY a.AppointmentDate, a.AppointmentTime";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var appointments = new List<Appointment>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                appointments.Add(MapToAppointment(row));
            }

            return appointments;
        }

        // ====================================
        // Update Appointment
        // ====================================
        public bool UpdateAppointment(Appointment appointment)
        {
            string query = @"
                UPDATE Appointments SET
                    PatientID = @PatientID,
                    AppointmentDate = @AppointmentDate,
                    AppointmentTime = @AppointmentTime,
                    DoctorID = @DoctorID,
                    AppointmentType = @AppointmentType,
                    Status = @Status,
                    Notes = @Notes,
                    ReminderSent = @ReminderSent
                WHERE AppointmentID = @AppointmentID";

            var parameters = new[]
            {
                new SQLiteParameter("@PatientID", appointment.PatientID),
                new SQLiteParameter("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@AppointmentTime", appointment.AppointmentTime.ToString()),
                new SQLiteParameter("@DoctorID", (object)appointment.DoctorID ?? DBNull.Value),
                new SQLiteParameter("@AppointmentType", appointment.AppointmentType),
                new SQLiteParameter("@Status", appointment.Status),
                new SQLiteParameter("@Notes", (object)appointment.Notes ?? DBNull.Value),
                new SQLiteParameter("@ReminderSent", appointment.ReminderSent ? 1 : 0),
                new SQLiteParameter("@AppointmentID", appointment.AppointmentID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool UpdateStatus(int appointmentId, string status)
        {
            string query = "UPDATE Appointments SET Status = @Status WHERE AppointmentID = @AppointmentID";
            var parameters = new[]
            {
                new SQLiteParameter("@Status", status),
                new SQLiteParameter("@AppointmentID", appointmentId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        public bool MarkReminderSent(int appointmentId)
        {
            string query = "UPDATE Appointments SET ReminderSent = 1 WHERE AppointmentID = @AppointmentID";
            var parameter = new SQLiteParameter("@AppointmentID", appointmentId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        // ====================================
        // Delete Appointment
        // ====================================
        public bool DeleteAppointment(int appointmentId)
        {
            string query = "DELETE FROM Appointments WHERE AppointmentID = @AppointmentID";
            var parameter = new SQLiteParameter("@AppointmentID", appointmentId);

            int rowsAffected = _db.ExecuteNonQuery(query, parameter);
            return rowsAffected > 0;
        }

        public bool CancelAppointment(int appointmentId)
        {
            return UpdateStatus(appointmentId, "ملغي");
        }

        // ====================================
        // Availability Check
        // ====================================
        public bool IsTimeSlotAvailable(DateTime date, TimeSpan time, int? doctorId = null)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Appointments 
                WHERE DATE(AppointmentDate) = DATE(@Date)
                  AND AppointmentTime = @Time
                  AND Status = 'محجوز'";

            if (doctorId.HasValue)
            {
                query += " AND DoctorID = @DoctorID";
            }

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@Date", date.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@Time", time.ToString())
            };

            if (doctorId.HasValue)
            {
                parameters.Add(new SQLiteParameter("@DoctorID", doctorId.Value));
            }

            var count = Convert.ToInt32(_db.ExecuteScalar(query, parameters.ToArray()));
            return count == 0;
        }

        public List<TimeSpan> GetAvailableTimeSlots(DateTime date, int? doctorId = null)
        {
            // الأوقات المتاحة من 9 صباحاً إلى 6 مساءً كل 30 دقيقة
            var allSlots = new List<TimeSpan>();
            var startTime = new TimeSpan(9, 0, 0);
            var endTime = new TimeSpan(18, 0, 0);
            var interval = TimeSpan.FromMinutes(30);

            for (var time = startTime; time < endTime; time = time.Add(interval))
            {
                allSlots.Add(time);
            }

            // إزالة الأوقات المحجوزة
            var bookedAppointments = GetAppointmentsByDate(date);
            var availableSlots = new List<TimeSpan>();

            foreach (var slot in allSlots)
            {
                bool isBooked = false;
                foreach (var appointment in bookedAppointments)
                {
                    if (appointment.AppointmentTime == slot && appointment.Status == "محجوز")
                    {
                        if (!doctorId.HasValue || appointment.DoctorID == doctorId)
                        {
                            isBooked = true;
                            break;
                        }
                    }
                }

                if (!isBooked)
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }

        // ====================================
        // Statistics
        // ====================================
        public int GetAppointmentsCountByStatus(string status, DateTime? date = null)
        {
            string query = "SELECT COUNT(*) FROM Appointments WHERE Status = @Status";

            if (date.HasValue)
            {
                query += " AND DATE(AppointmentDate) = DATE(@Date)";
            }

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@Status", status)
            };

            if (date.HasValue)
            {
                parameters.Add(new SQLiteParameter("@Date", date.Value.ToString("yyyy-MM-dd")));
            }

            return Convert.ToInt32(_db.ExecuteScalar(query, parameters.ToArray()));
        }

        public Dictionary<string, int> GetAppointmentStatistics(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT Status, COUNT(*) as Count
                FROM Appointments
                WHERE AppointmentDate BETWEEN @FromDate AND @ToDate
                GROUP BY Status";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var stats = new Dictionary<string, int>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                stats[row["Status"].ToString()] = Convert.ToInt32(row["Count"]);
            }

            return stats;
        }

        // ====================================
        // Helper Methods
        // ====================================
        private Appointment MapToAppointment(System.Data.DataRow row)
        {
            var appointment = new Appointment
            {
                AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                PatientID = Convert.ToInt32(row["PatientID"]),
                AppointmentDate = Convert.ToDateTime(row["AppointmentDate"]),
                AppointmentTime = TimeSpan.Parse(row["AppointmentTime"].ToString()),
                DoctorID = row["DoctorID"] != DBNull.Value ? Convert.ToInt32(row["DoctorID"]) : (int?)null,
                AppointmentType = row["AppointmentType"].ToString(),
                Status = row["Status"].ToString(),
                Notes = row["Notes"]?.ToString(),
                ReminderSent = Convert.ToBoolean(row["ReminderSent"])
            };

            // إضافة بيانات المريض والطبيب إذا كانت متاحة
            if (row.Table.Columns.Contains("PatientName"))
            {
                appointment.Patient = new Patient
                {
                    FullName = row["PatientName"].ToString(),
                    PhoneNumber = row["PhoneNumber"]?.ToString()
                };
            }

            if (row.Table.Columns.Contains("DoctorName") && row["DoctorName"] != DBNull.Value)
            {
                appointment.Doctor = new User { FullName = row["DoctorName"].ToString() };
            }

            return appointment;
        }
    }
}