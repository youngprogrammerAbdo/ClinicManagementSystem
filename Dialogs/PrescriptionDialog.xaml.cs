// ====================================
// PrescriptionDialog.xaml.cs
// ====================================
using ClinicManagementSystem.Repositories;
using System.Windows;

namespace ClinicManagementSystem.Dialogs
{
    public partial class PrescriptionDialog : Window
    {
        private readonly VisitRepository _visitRepo;
        private readonly PatientRepository _patientRepo;
        private int _visitId;
        private int _patientId;

        public PrescriptionDialog(int visitId)
        {
            InitializeComponent();
            _visitRepo = new VisitRepository();
            _patientRepo = new PatientRepository();
            _visitId = visitId;

            var visit = _visitRepo.GetVisitById(visitId);
            if (visit != null)
            {
                _patientId = visit.PatientID;
                var patient = _patientRepo.GetPatientById(_patientId);
                txtPatientName.Text = patient?.FullName ?? "غير محدد";
            }

            AddMedicineRow();
        }

        private void AddMedicine_Click(object sender, RoutedEventArgs e)
        {
            AddMedicineRow();
        }

        private void AddMedicineRow()
        {
            // Add medicine input row to panel
            MessageBox.Show("إضافة دواء", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save prescription
            MessageBox.Show("حفظ الروشتة", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
