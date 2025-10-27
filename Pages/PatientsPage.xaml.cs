// ====================================
// PatientsPage.xaml.cs - إدارة المرضى
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Dialogs;

namespace ClinicManagementSystem.Pages
{
    public partial class PatientsPage : Page
    {
        private readonly PatientRepository _patientRepo;
        private List<Patient> _allPatients;
        private List<Patient> _filteredPatients;
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;

        public PatientsPage()
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            LoadPatients();
        }

        private void LoadPatients()
        {
            try
            {
                _allPatients = _patientRepo.GetAllPatients();
                _filteredPatients = new List<Patient>(_allPatients);
                UpdatePagination();
                DisplayCurrentPage();
                UpdateTotalCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المرضى: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCurrentPage()
        {
            var skip = (_currentPage - 1) * _pageSize;
            var pageData = _filteredPatients.Skip(skip).Take(_pageSize).ToList();
            dgPatients.ItemsSource = pageData;
        }

        private void UpdatePagination()
        {
            _totalPages = (int)Math.Ceiling((double)_filteredPatients.Count / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            txtPageInfo.Text = $"صفحة {_currentPage} من {_totalPages}";
        }

        private void UpdateTotalCount()
        {
            txtTotalCount.Text = _filteredPatients.Count.ToString();
        }

        // ====================================
        // Search & Filter
        // ====================================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                _filteredPatients = new List<Patient>(_allPatients);
            }
            else
            {
                var searchTerm = txtSearch.Text.ToLower();
                _filteredPatients = _allPatients.Where(p =>
                    p.FirstName.ToLower().Contains(searchTerm) ||
                    p.LastName.ToLower().Contains(searchTerm) ||
                    p.PatientCode.ToLower().Contains(searchTerm) ||
                    p.PhoneNumber.Contains(searchTerm) ||
                    (p.NationalID != null && p.NationalID.Contains(searchTerm))
                ).ToList();
            }

            _currentPage = 1;
            UpdatePagination();
            DisplayCurrentPage();
            UpdateTotalCount();
        }

        private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AdvancedSearchDialog();
            if (dialog.ShowDialog() == true)
            {
                // تطبيق معايير البحث المتقدم
                _filteredPatients = _patientRepo.AdvancedSearch(
                    dialog.SearchName,
                    dialog.SearchPhone,
                    dialog.SearchNationalID,
                    dialog.FromDate,
                    dialog.ToDate
                );

                _currentPage = 1;
                UpdatePagination();
                DisplayCurrentPage();
                UpdateTotalCount();
            }
        }

        // ====================================
        // CRUD Operations
        // ====================================
        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PatientDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadPatients();
                MessageBox.Show("تم إضافة المريض بنجاح", "نجح",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewPatient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int patientId = Convert.ToInt32(button.Tag);
                var dialog = new PatientDetailsDialog(patientId);
                dialog.ShowDialog();
            }
        }

        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int patientId = Convert.ToInt32(button.Tag);
                var patient = _patientRepo.GetPatientById(patientId);

                if (patient != null)
                {
                    var dialog = new PatientDialog(patient);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadPatients();
                        MessageBox.Show("تم تحديث بيانات المريض بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذا المريض؟\nسيتم حذف جميع البيانات المرتبطة به.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int patientId = Convert.ToInt32(button.Tag);
                    if (_patientRepo.DeletePatient(patientId))
                    {
                        LoadPatients();
                        MessageBox.Show("تم حذف المريض بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل حذف المريض", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgPatients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgPatients.SelectedItem is Patient patient)
            {
                var dialog = new PatientDetailsDialog(patient.PatientID);
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

        // ====================================
        // Export
        // ====================================
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Patients_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var excelHelper = new Helpers.ExcelHelper();
                    if (excelHelper.ExportPatients(_filteredPatients, saveDialog.FileName))
                    {
                        MessageBox.Show("تم تصدير البيانات بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // فتح الملف
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                    else
                    {
                        MessageBox.Show("فشل تصدير البيانات", "خطأ",
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
    }
}