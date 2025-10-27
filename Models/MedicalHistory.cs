using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Medical History Model
    // ====================================
    public class MedicalHistory : BaseModel
    {
        private int _historyID;
        private int _patientID;
        private string _chronicDiseases;
        private string _allergies;
        private string _currentMedications;
        private string _previousSurgeries;
        private string _familyHistory;
        private string _notes;

        public int HistoryID
        {
            get => _historyID;
            set => SetProperty(ref _historyID, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public string ChronicDiseases
        {
            get => _chronicDiseases;
            set => SetProperty(ref _chronicDiseases, value);
        }

        public string Allergies
        {
            get => _allergies;
            set => SetProperty(ref _allergies, value);
        }

        public string CurrentMedications
        {
            get => _currentMedications;
            set => SetProperty(ref _currentMedications, value);
        }

        public string PreviousSurgeries
        {
            get => _previousSurgeries;
            set => SetProperty(ref _previousSurgeries, value);
        }

        public string FamilyHistory
        {
            get => _familyHistory;
            set => SetProperty(ref _familyHistory, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
    }

}
