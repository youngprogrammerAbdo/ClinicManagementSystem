using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Prescription Models
    // ====================================
    public class Prescription : BaseModel
    {
        private int _prescriptionID;
        private int _visitID;
        private int _patientID;
        private DateTime _prescriptionDate;
        private int? _doctorID;
        private string _notes;

        public int PrescriptionID
        {
            get => _prescriptionID;
            set => SetProperty(ref _prescriptionID, value);
        }

        public int VisitID
        {
            get => _visitID;
            set => SetProperty(ref _visitID, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public DateTime PrescriptionDate
        {
            get => _prescriptionDate;
            set => SetProperty(ref _prescriptionDate, value);
        }

        public int? DoctorID
        {
            get => _doctorID;
            set => SetProperty(ref _doctorID, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public List<PrescriptionDetail> Details { get; set; }
        public Patient Patient { get; set; }
        public User Doctor { get; set; }
    }
    public class PrescriptionDetail : BaseModel
    {
        private int _detailID;
        private int _prescriptionID;
        private string _medicineName;
        private string _dosage;
        private string _frequency;
        private string _duration;
        private string _instructions;

        public int DetailID
        {
            get => _detailID;
            set => SetProperty(ref _detailID, value);
        }

        public int PrescriptionID
        {
            get => _prescriptionID;
            set => SetProperty(ref _prescriptionID, value);
        }

        public string MedicineName
        {
            get => _medicineName;
            set => SetProperty(ref _medicineName, value);
        }

        public string Dosage
        {
            get => _dosage;
            set => SetProperty(ref _dosage, value);
        }

        public string Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public string Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }
    }
}
