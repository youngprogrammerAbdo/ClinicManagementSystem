using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;
using System.Windows;

namespace ClinicManagementSystem.Dialogs
{
    public partial class InvoiceDetailsDialog : Window
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly PatientRepository _patientRepo;
        private Invoice _invoice;

        public InvoiceDetailsDialog(int invoiceId)
        {
            InitializeComponent();
            _invoiceRepo = new InvoiceRepository();
            _patientRepo = new PatientRepository();
            LoadInvoiceData(invoiceId);
        }

        private void LoadInvoiceData(int invoiceId)
        {
            _invoice = _invoiceRepo.GetInvoiceById(invoiceId);
            if (_invoice != null)
            {
                DataContext = _invoice;

                var patient = _patientRepo.GetPatientById(_invoice.PatientID);
                txtPatientName.Text = patient?.FullName ?? "غير محدد";

                dgItems.ItemsSource = _invoice.Items;
                dgPayments.ItemsSource = _invoice.Payments;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // طباعة الفاتورة
            MessageBox.Show("طباعة الفاتورة", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice.RemainingAmount > 0)
            {
                var dialog = new PaymentDialog(_invoice);
                if (dialog.ShowDialog() == true)
                {
                    LoadInvoiceData(_invoice.InvoiceID);
                }
            }
            else
            {
                MessageBox.Show("الفاتورة مدفوعة بالكامل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}