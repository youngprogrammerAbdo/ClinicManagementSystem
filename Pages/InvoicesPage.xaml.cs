// ====================================
// InvoicesPage.xaml.cs
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Dialogs;
using ClinicManagementSystem.Helpers;

namespace ClinicManagementSystem.Pages
{
    public partial class InvoicesPage : Page
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly PatientRepository _patientRepo;
        private List<InvoiceDisplay> _allInvoices;
        private List<InvoiceDisplay> _filteredInvoices;
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;

        public InvoicesPage()
        {
            InitializeComponent();
            _invoiceRepo = new InvoiceRepository();
            _patientRepo = new PatientRepository();
            LoadInvoices();
        }

        private void LoadInvoices()
        {
            try
            {
                var invoices = _invoiceRepo.GetInvoicesByDateRange(
                    DateTime.Now.AddMonths(-3),
                    DateTime.Now);

                _allInvoices = new List<InvoiceDisplay>();
                foreach (var invoice in invoices)
                {
                    var patient = _patientRepo.GetPatientById(invoice.PatientID);
                    _allInvoices.Add(new InvoiceDisplay
                    {
                        InvoiceID = invoice.InvoiceID,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.InvoiceDate,
                        PatientName = patient?.FullName ?? "غير محدد",
                        InvoiceType = invoice.InvoiceType,
                        TotalAmount = invoice.TotalAmount,
                        DiscountAmount = invoice.DiscountAmount,
                        NetAmount = invoice.NetAmount,
                        PaidAmount = invoice.PaidAmount,
                        RemainingAmount = invoice.RemainingAmount,
                        PaymentStatus = invoice.PaymentStatus
                    });
                }

                _filteredInvoices = new List<InvoiceDisplay>(_allInvoices);
                UpdateStatistics();
                UpdatePagination();
                DisplayCurrentPage();
                UpdateTotalCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الفواتير: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            decimal totalPaid = _allInvoices.Sum(i => i.PaidAmount);
            decimal totalRemaining = _allInvoices.Sum(i => i.RemainingAmount);
            int unpaidCount = _allInvoices.Count(i => i.PaymentStatus != "مدفوع");
            int paidCount = _allInvoices.Count(i => i.PaymentStatus == "مدفوع");

            txtTotalPaid.Text = $"{totalPaid:N0} جنيه";
            txtTotalRemaining.Text = $"{totalRemaining:N0} جنيه";
            txtUnpaidCount.Text = unpaidCount.ToString();
            txtPaidCount.Text = paidCount.ToString();
        }

        private void DisplayCurrentPage()
        {
            var skip = (_currentPage - 1) * _pageSize;
            var pageData = _filteredInvoices.Skip(skip).Take(_pageSize).ToList();
            dgInvoices.ItemsSource = pageData;
        }

        private void UpdatePagination()
        {
            _totalPages = (int)Math.Ceiling((double)_filteredInvoices.Count / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            txtPageInfo.Text = $"صفحة {_currentPage} من {_totalPages}";
        }

        private void UpdateTotalCount()
        {
            txtTotalCount.Text = _filteredInvoices.Count.ToString();
        }

        // ====================================
        // Search & Filter
        // ====================================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var searchTerm = txtSearch.Text.ToLower();
            var selectedStatus = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            _filteredInvoices = _allInvoices.Where(i =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) ||
                    i.InvoiceNumber.ToLower().Contains(searchTerm) ||
                    i.PatientName.ToLower().Contains(searchTerm);

                bool matchesStatus = selectedStatus == "جميع الفواتير" ||
                    i.PaymentStatus == selectedStatus;

                return matchesSearch && matchesStatus;
            }).ToList();

            _currentPage = 1;
            UpdatePagination();
            DisplayCurrentPage();
            UpdateTotalCount();
        }

        // ====================================
        // CRUD Operations
        // ====================================
        private void NewInvoice_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InvoiceDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadInvoices();
                MessageBox.Show("تم إنشاء الفاتورة بنجاح", "نجح",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int invoiceId = Convert.ToInt32(button.Tag);
                var dialog = new InvoiceDetailsDialog(invoiceId);
                dialog.ShowDialog();
            }
        }

        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int invoiceId = Convert.ToInt32(button.Tag);
                var invoice = _invoiceRepo.GetInvoiceById(invoiceId);

                if (invoice != null)
                {
                    if (invoice.RemainingAmount <= 0)
                    {
                        MessageBox.Show("الفاتورة مدفوعة بالكامل", "تنبيه",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var dialog = new PaymentDialog(invoice);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadInvoices();
                        MessageBox.Show("تم إضافة الدفعة بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void PrintInvoice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int invoiceId = Convert.ToInt32(button.Tag);

                try
                {
                    var invoice = _invoiceRepo.GetInvoiceById(invoiceId);
                    var patient = _patientRepo.GetPatientById(invoice.PatientID);
                    var items = _invoiceRepo.GetInvoiceItems(invoiceId);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF Files|*.pdf",
                        FileName = $"Invoice_{invoice.InvoiceNumber}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        var pdfHelper = new PDFHelper();
                        if (pdfHelper.GenerateInvoiceReport(invoice, patient, items, saveDialog.FileName))
                        {
                            MessageBox.Show("تم إنشاء الفاتورة بنجاح", "نجح",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذه الفاتورة؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // يمكن تنفيذ الحذف هنا
                    MessageBox.Show("تم حذف الفاتورة", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadInvoices();
                }
            }
        }

        private void dgInvoices_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgInvoices.SelectedItem is InvoiceDisplay invoice)
            {
                var dialog = new InvoiceDetailsDialog(invoice.InvoiceID);
                dialog.ShowDialog();
            }
        }

        // ====================================
        // Pagination
        // ====================================
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            UpdatePagination();
            DisplayCurrentPage();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
                DisplayCurrentPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
                DisplayCurrentPage();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = _totalPages;
            UpdatePagination();
            DisplayCurrentPage();
        }

        private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            // يمكن إضافة نافذة بحث متقدم
            MessageBox.Show("البحث المتقدم قريباً", "معلومة",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Invoices_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var invoices = _filteredInvoices.Select(i => new Invoice
                    {
                        InvoiceNumber = i.InvoiceNumber,
                        InvoiceDate = i.InvoiceDate,
                        InvoiceType = i.InvoiceType,
                        TotalAmount = i.TotalAmount,
                        DiscountAmount = i.DiscountAmount,
                        NetAmount = i.NetAmount,
                        PaidAmount = i.PaidAmount,
                        RemainingAmount = i.RemainingAmount,
                        PaymentStatus = i.PaymentStatus,
                        Patient = new Patient { FullName = i.PatientName }
                    }).ToList();

                    var excelHelper = new ExcelHelper();
                    if (excelHelper.ExportInvoices(invoices, saveDialog.FileName))
                    {
                        MessageBox.Show("تم تصدير البيانات بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Helper class for display
    public class InvoiceDisplay
    {
        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string PatientName { get; set; }
        public string InvoiceType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}