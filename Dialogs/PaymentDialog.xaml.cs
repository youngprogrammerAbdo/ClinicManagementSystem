// ====================================
// PaymentDialog.xaml.cs
// ====================================

using System;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;

namespace ClinicManagementSystem.Dialogs
{
    public partial class PaymentDialog : Window
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly Invoice _invoice;
        private decimal _remainingAmount;

        public PaymentDialog(Invoice invoice)
        {
            InitializeComponent();
            _invoiceRepo = new InvoiceRepository();
            _invoice = invoice;
            LoadInvoiceData();
        }

        private void LoadInvoiceData()
        {
            txtInvoiceNumber.Text = $"رقم الفاتورة: {_invoice.InvoiceNumber}";
            txtNetAmount.Text = $"{_invoice.NetAmount:N2} جنيه";
            txtPreviouslyPaid.Text = $"{_invoice.PaidAmount:N2} جنيه";
            _remainingAmount = _invoice.RemainingAmount;
            txtRemainingAmount.Text = $"{_remainingAmount:N2} جنيه";

            // ملء المبلغ بالمتبقي بشكل افتراضي
            txtAmount.Text = _remainingAmount.ToString("F2");
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                if (amount > _remainingAmount)
                {
                    txtAmount.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    txtAmount.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void QuickAmount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int percentage = Convert.ToInt32(button.Tag);
                decimal amount = (_remainingAmount * percentage) / 100;
                txtAmount.Text = amount.ToString("F2");
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                decimal amount = decimal.Parse(txtAmount.Text);

                if (amount > _remainingAmount)
                {
                    var result = MessageBox.Show(
                        $"المبلغ المدفوع ({amount:N2}) أكبر من المتبقي ({_remainingAmount:N2})\nهل تريد المتابعة؟",
                        "تنبيه",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                        return;
                }

                var payment = new Payment
                {
                    InvoiceID = _invoice.InvoiceID,
                    Amount = amount,
                    PaymentMethod = (cmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Reference = txtReference.Text,
                    Notes = txtNotes.Text,
                    PaymentDate = DateTime.Now,
                    ReceivedBy = MainWindow.CurrentUser?.UserID
                };

                int paymentId = _invoiceRepo.AddPayment(payment);

                if (paymentId > 0)
                {
                    MessageBox.Show("تم إضافة الدفعة بنجاح", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("الرجاء إدخال المبلغ", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("الرجاء إدخال مبلغ صحيح", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
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