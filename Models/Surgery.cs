using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Surgery Model
    // ====================================
    public class Surgery : BaseModel
    {
        private int _surgeryID;
        private int _patientID;
        private string _surgeryName;
        private DateTime? _surgeryDate;
        private DateTime? _scheduledDate;
        private string _surgeryType;
        private int? _doctorID;
        private string _assistantDoctors;
        private string _anesthesia;
        private int? _duration;
        private string _notes;
        private decimal _cost;
        private string _status;

        public int SurgeryID
        {
            get => _surgeryID;
            set => SetProperty(ref _surgeryID, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public string SurgeryName
        {
            get => _surgeryName;
            set => SetProperty(ref _surgeryName, value);
        }

        public DateTime? SurgeryDate
        {
            get => _surgeryDate;
            set => SetProperty(ref _surgeryDate, value);
        }

        public DateTime? ScheduledDate
        {
            get => _scheduledDate;
            set => SetProperty(ref _scheduledDate, value);
        }

        public string SurgeryType
        {
            get => _surgeryType;
            set => SetProperty(ref _surgeryType, value);
        }

        public int? DoctorID
        {
            get => _doctorID;
            set => SetProperty(ref _doctorID, value);
        }

        public string AssistantDoctors
        {
            get => _assistantDoctors;
            set => SetProperty(ref _assistantDoctors, value);
        }

        public string Anesthesia
        {
            get => _anesthesia;
            set => SetProperty(ref _anesthesia, value);
        }

        public int? Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public decimal Cost
        {
            get => _cost;
            set => SetProperty(ref _cost, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public Patient Patient { get; set; }
        public User Doctor { get; set; }
    }

}
