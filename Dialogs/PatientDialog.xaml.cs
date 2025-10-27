// ====================================
// PatientDialog.xaml.cs - نموذج إضافة/تعديل مريض
// ====================================

using System;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;

namespace ClinicManagementSystem.Dialogs
{
    public partial class PatientDialog : Window
    {
        private readonly PatientRepository _patientRepo;
        private Patient _patient;
        private bool _isEditMode = false;

        // Constructor للإضافة
        public PatientDialog()
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            _patient = new Patient();
            txtTitle.Text = "➕ إضافة مريض جديد";
            dpDateOfBirth.SelectedDate = DateTime.Now.AddYears(-30);
        }

        // Constructor للتعديل
        public PatientDialog(Patient patient)
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            _patient = patient;
            _isEditMode = true;
            txtTitle.Text = "✏️ تعديل بيانات المريض";
            LoadPatientData();
        }

        private void LoadPatientData()
        {
            // البيانات الشخصية
            txtFirstName.Text = _patient.FirstName;
            txtLastName.Text = _patient.LastName;
            dpDateOfBirth.SelectedDate = _patient.DateOfBirth;
            cmbGender.Text = _patient.Gender;
            txtPhoneNumber.Text = _patient.PhoneNumber;
            txtPhoneNumber2.Text = _patient.PhoneNumber2;
            txtAddress.Text = _patient.Address;
            txtNationalID.Text = _patient.NationalID;
            cmbBloodType.Text = _patient.BloodType;
            txtEmail.Text = _patient.Email;

            // جهة الاتصال الطارئة
            txtEmergencyContact.Text = _patient.EmergencyContact;
            txtEmergencyPhone.Text = _patient.EmergencyPhone;

            // ملاحظات
            txtNotes.Text = _patient.Notes;

            // التاريخ المرضي
            if (_patient.MedicalHistory == null)
            {
                _patient.MedicalHistory = _patientRepo.GetMedicalHistory(_patient.PatientID);
            }

            if (_patient.MedicalHistory != null)
            {
                txtChronicDiseases.Text = _patient.MedicalHistory.ChronicDiseases;
                txtAllergies.Text = _patient.MedicalHistory.Allergies;
                txtCurrentMedications.Text = _patient.MedicalHistory.CurrentMedications;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                // جمع البيانات
                _patient.FirstName = txtFirstName.Text.Trim();
                _patient.LastName = txtLastName.Text.Trim();
                _patient.DateOfBirth = dpDateOfBirth.SelectedDate.Value;
                _patient.Gender = (cmbGender.SelectedItem as ComboBoxItem)?.Content.ToString();
                _patient.PhoneNumber = txtPhoneNumber.Text.Trim();
                _patient.PhoneNumber2 = txtPhoneNumber2.Text.Trim();
                _patient.Address = txtAddress.Text.Trim();
                _patient.NationalID = txtNationalID.Text.Trim();
                _patient.BloodType = (cmbBloodType.SelectedItem as ComboBoxItem)?.Content.ToString();
                _patient.Email = txtEmail.Text.Trim();
                _patient.EmergencyContact = txtEmergencyContact.Text.Trim();
                _patient.EmergencyPhone = txtEmergencyPhone.Text.Trim();
                _patient.Notes = txtNotes.Text.Trim();
                _patient.IsActive = true;

                if (_isEditMode)
                {
                    // تحديث
                    if (_patientRepo.UpdatePatient(_patient))
                    {
                        // تحديث التاريخ المرضي
                        SaveMedicalHistory();

                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("فشل في تحديث بيانات المريض", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // إضافة
                    int patientId = _patientRepo.AddPatient(_patient);
                    if (patientId > 0)
                    {
                        _patient.PatientID = patientId;

                        // حفظ التاريخ المرضي
                        SaveMedicalHistory();

                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("فشل في إضافة المريض", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMedicalHistory()
        {
            var history = new MedicalHistory
            {
                PatientID = _patient.PatientID,
                ChronicDiseases = txtChronicDiseases.Text.Trim(),
                Allergies = txtAllergies.Text.Trim(),
                CurrentMedications = txtCurrentMedications.Text.Trim()
            };

            _patientRepo.AddOrUpdateMedicalHistory(history);
        }

        private bool ValidateInput()
        {
            // التحقق من الحقول المطلوبة
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("الرجاء إدخال الاسم الأول", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم العائلة", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return false;
            }

            if (!dpDateOfBirth.SelectedDate.HasValue)
            {
                MessageBox.Show("الرجاء اختيار تاريخ الميلاد", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDateOfBirth.Focus();
                return false;
            }

            if (dpDateOfBirth.SelectedDate.Value > DateTime.Now)
            {
                MessageBox.Show("تاريخ الميلاد غير صحيح", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbGender.SelectedIndex == -1)
            {
                MessageBox.Show("الرجاء اختيار الجنس", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbGender.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                MessageBox.Show("الرجاء إدخال رقم الهاتف", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            // التحقق من رقم الهاتف
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtPhoneNumber.Text, @"^[0-9\+\-\s]+$"))
            {
                MessageBox.Show("رقم الهاتف غير صحيح", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhoneNumber.Focus();
                return false;
            }

            // التحقق من البريد الإلكتروني إذا تم إدخاله
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("البريد الإلكتروني غير صحيح", "تنبيه",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            // التحقق من الرقم القومي إذا تم إدخاله
            if (!string.IsNullOrWhiteSpace(txtNationalID.Text))
            {
                if (txtNationalID.Text.Length != 14)
                {
                    MessageBox.Show("الرقم القومي يجب أن يكون 14 رقم", "تنبيه",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNationalID.Focus();
                    return false;
                }

                // التحقق من عدم تكرار الرقم القومي
                if (!_isEditMode || txtNationalID.Text != _patient.NationalID)
                {
                    var existingPatient = _patientRepo.GetPatientByCode(txtNationalID.Text);
                    if (existingPatient != null)
                    {
                        MessageBox.Show("الرقم القومي مسجل بالفعل لمريض آخر", "تنبيه",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtNationalID.Focus();
                        return false;
                    }
                }
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من الإلغاء؟ سيتم فقد جميع التغييرات.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}