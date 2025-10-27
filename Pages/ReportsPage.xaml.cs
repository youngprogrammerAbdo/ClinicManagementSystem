
                using System;
                using System.Windows;
                using System.Windows.Controls;
                using ClinicManagementSystem.Repositories;
                using ClinicManagementSystem.Helpers;

namespace ClinicManagementSystem.Pages
    {
        public partial class ReportsPage : Page
        {
            private readonly VisitRepository _visitRepo;
            private readonly InvoiceRepository _invoiceRepo;
            private readonly PatientRepository _patientRepo;
            private readonly InventoryRepository _inventoryRepo;
            private readonly PDFHelper _pdfHelper;
            private readonly ExcelHelper _excelHelper;

            public ReportsPage()
            {
                InitializeComponent();
                _visitRepo = new VisitRepository();
                _invoiceRepo = new InvoiceRepository();
                _patientRepo = new PatientRepository();
                _inventoryRepo = new InventoryRepository();
                _pdfHelper = new PDFHelper();
                _excelHelper = new ExcelHelper();

                LoadPatients();
                InitializeDates();
            }

            private void LoadPatients()
            {
                cmbPatient.ItemsSource = _patientRepo.GetAllPatients();
            }

            private void InitializeDates()
            {
                dpCustomFrom.SelectedDate = DateTime.Today.AddMonths(-1);
                dpCustomTo.SelectedDate = DateTime.Today;
            }

            private void DailyReportPDF_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var date = dpDailyDate.SelectedDate ?? DateTime.Today;
                    var visits = _visitRepo.GetVisitsByDateRange(date, date);
                    var revenue = _invoiceRepo.GetTotalRevenue(date, date);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF Files|*.pdf",
                        FileName = $"DailyReport_{date:yyyyMMdd}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_pdfHelper.GenerateDailyReport(date, visits, revenue, saveDialog.FileName))
                        {
                            MessageBox.Show("تم إنشاء التقرير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void DailyReportExcel_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var date = dpDailyDate.SelectedDate ?? DateTime.Today;
                    var visits = _visitRepo.GetVisitsByDateRange(date, date);
                    var revenue = _invoiceRepo.GetTotalRevenue(date, date);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        FileName = $"DailyReport_{date:yyyyMMdd}.xlsx"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_excelHelper.ExportDailyReport(date, visits, revenue, saveDialog.FileName))
                        {
                            MessageBox.Show("تم التصدير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void MonthlyReport_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    int month = Convert.ToInt32((cmbMonth.SelectedItem as ComboBoxItem).Tag);
                    int year = Convert.ToInt32((cmbYear.SelectedItem as ComboBoxItem).Content);

                    var firstDay = new DateTime(year, month, 1);
                    var lastDay = firstDay.AddMonths(1).AddDays(-1);

                    var stats = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["عدد الزيارات"] = _visitRepo.GetVisitsCountByDateRange(firstDay, lastDay),
                        ["إجمالي الإيرادات"] = _invoiceRepo.GetTotalRevenue(firstDay, lastDay).ToString("N2") + " جنيه",
                        ["عدد المرضى الجدد"] = _patientRepo.GetNewPatientsCount(firstDay),
                        ["إجمالي الديون"] = _invoiceRepo.GetTotalDebts().ToString("N2") + " جنيه"
                    };

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        FileName = $"MonthlyReport_{year}_{month:D2}.xlsx"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_excelHelper.ExportMonthlyReport(year, month, stats, saveDialog.FileName))
                        {
                            MessageBox.Show("تم إنشاء التقرير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void DebtsReportPDF_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("تقرير الديون PDF - قيد التطوير", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            private void DebtsReportExcel_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var unpaidInvoices = _invoiceRepo.GetUnpaidInvoices();

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        FileName = $"DebtsReport_{DateTime.Now:yyyyMMdd}.xlsx"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_excelHelper.ExportInvoices(unpaidInvoices, saveDialog.FileName))
                        {
                            MessageBox.Show("تم التصدير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void InventoryReport_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var items = chkLowStockOnly.IsChecked == true
                        ? _inventoryRepo.GetLowStockItems()
                        : _inventoryRepo.GetAllItems();

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        FileName = $"InventoryReport_{DateTime.Now:yyyyMMdd}.xlsx"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_excelHelper.ExportInventory(items, saveDialog.FileName))
                        {
                            MessageBox.Show("تم التصدير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void CustomReport_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var fromDate = dpCustomFrom.SelectedDate ?? DateTime.Today.AddMonths(-1);
                    var toDate = dpCustomTo.SelectedDate ?? DateTime.Today;

                    var visits = _visitRepo.GetVisitsByDateRange(fromDate, toDate);
                    var revenue = _invoiceRepo.GetTotalRevenue(fromDate, toDate);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF Files|*.pdf",
                        FileName = $"CustomReport_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_pdfHelper.GenerateDailyReport(fromDate, visits, revenue, saveDialog.FileName))
                        {
                            MessageBox.Show("تم إنشاء التقرير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void SearchPatient_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("بحث عن مريض", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            private void PatientReport_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (cmbPatient.SelectedValue == null)
                    {
                        MessageBox.Show("الرجاء اختيار مريض", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int patientId = (int)cmbPatient.SelectedValue;
                    var patient = _patientRepo.GetPatientById(patientId);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF Files|*.pdf",
                        FileName = $"PatientReport_{patient.PatientCode}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (_pdfHelper.GeneratePatientReport(patient, saveDialog.FileName))
                        {
                            MessageBox.Show("تم إنشاء التقرير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }