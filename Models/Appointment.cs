using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Appointment Model
    // ====================================
    public class Appointment : BaseModel
    {
        private int _appointmentID;
        private int _patientID;
        private DateTime _appointmentDate;
        private TimeSpan _appointmentTime;
        private int? _doctorID;
        private string _appointmentType;
        private string _status;
        private string _notes;
        private bool _reminderSent;

        public int AppointmentID
        {
            get => _appointmentID;
            set => SetProperty(ref _appointmentID, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set => SetProperty(ref _appointmentTime, value);
        }

        public int? DoctorID
        {
            get => _doctorID;
            set => SetProperty(ref _doctorID, value);
        }

        public string AppointmentType
        {
            get => _appointmentType;
            set => SetProperty(ref _appointmentType, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public bool ReminderSent
        {
            get => _reminderSent;
            set => SetProperty(ref _reminderSent, value);
        }

        public Patient Patient { get; set; }
        public User Doctor { get; set; }
    }

}
