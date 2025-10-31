// ====================================
// PatientDetailsDialog.xaml.cs
// ====================================
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;
using System.Windows;

namespace ClinicManagementSystem.Dialogs
{
    public partial class PatientDetailsDialog : Window
    {
        private readonly PatientRepository _patientRepo;
        private readonly VisitRepository _visitRepo;
        private readonly InvoiceRepository _invoiceRepo;
        private Patient _patient;

        public PatientDetailsDialog(int patientId)
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            _visitRepo = new VisitRepository();
            _invoiceRepo = new InvoiceRepository();
            LoadPatientData(patientId);
        }

        private void LoadPatientData(int patientId)
        {
            _patient = _patientRepo.GetPatientById(patientId);
            if (_patient != null)
            {
                // عرض البيانات الأساسية
                DataContext = _patient;

                // تحميل التاريخ المرضي
                _patient.MedicalHistory = _patientRepo.GetMedicalHistory(patientId);

                // تحميل الزيارات
                var visits = _visitRepo.GetPatientVisits(patientId);
                dgVisits.ItemsSource = visits;

                // تحميل الفواتير
                var invoices = _invoiceRepo.GetPatientInvoices(patientId);
                dgInvoices.ItemsSource = invoices;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PatientDialog(_patient);
            if (dialog.ShowDialog() == true)
            {
                LoadPatientData(_patient.PatientID);
            }
        }
    }
}