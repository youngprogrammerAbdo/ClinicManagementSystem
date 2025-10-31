using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;
using System;
using System.Windows;

namespace ClinicManagementSystem.Dialogs
{
    public partial class AppointmentDialog : Window
    {
        private readonly AppointmentRepository _appointmentRepo;
        private readonly PatientRepository _patientRepo;
        private Appointment _appointment;
        private bool _isEditMode;

        public AppointmentDialog(Appointment appointment = null)
        {
            InitializeComponent();
            _appointmentRepo = new AppointmentRepository();
            _patientRepo = new PatientRepository();

            if (appointment != null)
            {
                _appointment = appointment;
                _isEditMode = true;
                LoadAppointmentData();
            }
            else
            {
                _appointment = new Appointment();
                _isEditMode = false;
                dpDate.SelectedDate = DateTime.Today;
            }

            LoadPatients();
        }

        private void LoadPatients()
        {
            cmbPatient.ItemsSource = _patientRepo.GetAllPatients();
            if (_isEditMode)
            {
                cmbPatient.SelectedValue = _appointment.PatientID;
            }
        }

        private void LoadAppointmentData()
        {
            dpDate.SelectedDate = _appointment.AppointmentDate;
            // Load other fields...
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                _appointment.PatientID = (int)cmbPatient.SelectedValue;
                _appointment.AppointmentDate = dpDate.SelectedDate.Value;
                // Set other fields...

                if (_isEditMode)
                {
                    _appointmentRepo.UpdateAppointment(_appointment);
                }
                else
                {
                    _appointmentRepo.AddAppointment(_appointment);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (cmbPatient.SelectedValue == null)
            {
                MessageBox.Show("الرجاء اختيار المريض", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
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