using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Patient Model
    // ====================================
    public class Patient : BaseModel
    {
        private int _patientID;
        private string _patientCode;
        private string _firstName;
        private string _lastName;
        private DateTime _dateOfBirth;
        private string _gender;
        private string _phoneNumber;
        private string _phoneNumber2;
        private string _address;
        private string _nationalID;
        private string _bloodType;
        private string _email;
        private string _emergencyContact;
        private string _emergencyPhone;
        private string _notes;
        private string _profileImagePath;
        private DateTime _registrationDate;
        private bool _isActive;

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public string PatientCode
        {
            get => _patientCode;
            set => SetProperty(ref _patientCode, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string FullName
        {
            get => $"{FirstName} {LastName}";
            set
            {
                var parts = value.Split(' ');
                if (parts.Length >= 2)
                {
                    FirstName = parts[0];
                    LastName = string.Join(" ", parts.Skip(1));
                }
            }
        }


        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string PhoneNumber2
        {
            get => _phoneNumber2;
            set => SetProperty(ref _phoneNumber2, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string NationalID
        {
            get => _nationalID;
            set => SetProperty(ref _nationalID, value);
        }

        public string BloodType
        {
            get => _bloodType;
            set => SetProperty(ref _bloodType, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string EmergencyContact
        {
            get => _emergencyContact;
            set => SetProperty(ref _emergencyContact, value);
        }

        public string EmergencyPhone
        {
            get => _emergencyPhone;
            set => SetProperty(ref _emergencyPhone, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => SetProperty(ref _profileImagePath, value);
        }

        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set => SetProperty(ref _registrationDate, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public MedicalHistory MedicalHistory { get; set; }
        public List<Visit> Visits { get; set; }
        public List<MedicalDocument> Documents { get; set; }
        public List<Invoice> Invoices { get; set; }
    }
}
