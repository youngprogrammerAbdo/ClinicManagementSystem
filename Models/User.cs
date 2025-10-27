using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // User Model
    // ====================================
    public class User : BaseModel
    {
        private int _userID;
        private string _username;
        private string _password;
        private string _fullName;
        private string _role;
        private string _phoneNumber;
        private string _email;
        private decimal _salary;
        private DateTime? _hireDate;
        private bool _isActive;
        private string _permissions;
        private string _profileImagePath;

        public int UserID
        {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public decimal Salary
        {
            get => _salary;
            set => SetProperty(ref _salary, value);
        }

        public DateTime? HireDate
        {
            get => _hireDate;
            set => SetProperty(ref _hireDate, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string Permissions
        {
            get => _permissions;
            set => SetProperty(ref _permissions, value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => SetProperty(ref _profileImagePath, value);
        }
    }
}
