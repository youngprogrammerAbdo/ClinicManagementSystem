using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Visit Model
    // ====================================
    public class Visit : BaseModel
    {
        private int _visitID;
        private int _patientID;
        private DateTime _visitDate;
        private string _visitType;
        private string _chiefComplaint;
        private string _diagnosis;
        private string _treatment;
        private string _notes;
        private int? _doctorID;
        private string _visitStatus;
        private decimal _examinationFee;
        private bool _isPaid;
        private int? _queueNumber;

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

        public DateTime VisitDate
        {
            get => _visitDate;
            set => SetProperty(ref _visitDate, value);
        }

        public string VisitType
        {
            get => _visitType;
            set => SetProperty(ref _visitType, value);
        }

        public string ChiefComplaint
        {
            get => _chiefComplaint;
            set => SetProperty(ref _chiefComplaint, value);
        }

        public string Diagnosis
        {
            get => _diagnosis;
            set => SetProperty(ref _diagnosis, value);
        }

        public string Treatment
        {
            get => _treatment;
            set => SetProperty(ref _treatment, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public int? DoctorID
        {
            get => _doctorID;
            set => SetProperty(ref _doctorID, value);
        }

        public string VisitStatus
        {
            get => _visitStatus;
            set => SetProperty(ref _visitStatus, value);
        }

        public decimal ExaminationFee
        {
            get => _examinationFee;
            set => SetProperty(ref _examinationFee, value);
        }

        public bool IsPaid
        {
            get => _isPaid;
            set => SetProperty(ref _isPaid, value);
        }

        public int? QueueNumber
        {
            get => _queueNumber;
            set => SetProperty(ref _queueNumber, value);
        }

        public Patient Patient { get; set; }
        public User Doctor { get; set; }
    }
}
