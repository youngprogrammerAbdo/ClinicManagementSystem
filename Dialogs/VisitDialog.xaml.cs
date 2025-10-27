// ====================================
// VisitDialog.xaml.cs
// ====================================

using System;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;

namespace ClinicManagementSystem.Dialogs
{
    public partial class VisitDialog : Window
    {
        private readonly VisitRepository _visitRepo;
        private readonly PatientRepository _patientRepo;
        private Patient _selectedPatient;

        public VisitDialog()
        {
            InitializeComponent();
            _visitRepo = new VisitRepository();
            _patientRepo = new PatientRepository();
            LoadPatients();
        }

        private void LoadPatients()
        {
            var patients = _patientRepo.GetAllPatients();
            cmbPatient.ItemsSource = patients;
        }

        private void Patient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPatient.SelectedValue != null)
            {
                int patientId = (int)cmbPatient.SelectedValue;
                _selectedPatient = _patientRepo.GetPatientById(patientId);

                if (_selectedPatient != null)
                {
                    txtPatientInfo.Text = $"العمر: {_selectedPatient.Age} سنة | " +
                                        $"الهاتف: {_selectedPatient.PhoneNumber} | " +
                                        $"آخر زيارة: {GetLastVisitDate(patientId)}";
                    pnlPatientInfo.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetLastVisitDate(int patientId)
        {
            var visits = _visitRepo.GetPatientVisits(patientId);
            if (visits.Count > 0)
            {
                return visits[0].VisitDate.ToString("dd/MM/yyyy");
            }
            return "أول زيارة";
        }

        private void SearchPatient_Click(object sender, RoutedEventArgs e)
        {
            var searchDialog = new SearchPatientDialog();
            if (searchDialog.ShowDialog() == true && searchDialog.SelectedPatient != null)
            {
                cmbPatient.SelectedValue = searchDialog.SelectedPatient.PatientID;
            }
        }

        private void VisitType_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = (cmbVisitType.SelectedItem as ComboBoxItem)?.Content.ToString();

            // تغيير الرسوم حسب النوع
            if (selectedType == "إعادة")
            {
                txtExaminationFee.Text = "100"; // سعر الإعادة
            }
            else if (selectedType == "طوارئ")
            {
                txtExaminationFee.Text = "300"; // سعر الطوارئ
            }
            else
            {
                txtExaminationFee.Text = "200"; // السعر العادي
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveVisit(false);
        }

        private void SaveAndAddToQueue_Click(object sender, RoutedEventArgs e)
        {
            SaveVisit(true);
        }

        private void SaveVisit(bool addToQueue)
        {
            if (!ValidateInput())
                return;

            try
            {
                decimal.TryParse(txtExaminationFee.Text, out decimal fee);

                var visit = new Visit
                {
                    PatientID = (int)cmbPatient.SelectedValue,
                    VisitDate = DateTime.Now,
                    VisitType = (cmbVisitType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    ChiefComplaint = txtChiefComplaint.Text,
                    Diagnosis = txtDiagnosis.Text,
                    Treatment = txtTreatment.Text,
                    Notes = txtNotes.Text,
                    ExaminationFee = fee,
                    VisitStatus = addToQueue ? "منتظر" : "منتهي",
                    IsPaid = false
                };

                int visitId = _visitRepo.AddVisit(visit);

                if (visitId > 0)
                {
                    string message = addToQueue
                        ? $"تم إضافة المريض للدور برقم {_visitRepo.GetNextQueueNumber() - 1}"
                        : "تم حفظ الزيارة بنجاح";

                    MessageBox.Show(message, "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (cmbPatient.SelectedValue == null)
            {
                MessageBox.Show("الرجاء اختيار المريض", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbPatient.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtExaminationFee.Text))
            {
                MessageBox.Show("الرجاء إدخال رسوم الكشف", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtExaminationFee.Focus();
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}