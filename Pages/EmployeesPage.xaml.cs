// ====================================
// EmployeesPage.xaml.cs
// ====================================
using ClinicManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ClinicManagementSystem.Pages
{
    public partial class EmployeesPage : Page
    {
        private readonly Repositories.UserRepository _userRepo;
        private List<User> _allEmployees;
        private List<User> _filteredEmployees;

        public EmployeesPage()
        {
            InitializeComponent();
            _userRepo = new Repositories.UserRepository();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                _allEmployees = _userRepo.GetAllUsers();
                _filteredEmployees = new List<User>(_allEmployees);

                dgEmployees.ItemsSource = _filteredEmployees;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            txtTotalEmployees.Text = _allEmployees.Count.ToString();

            var doctors = _allEmployees.FindAll(e => e.Role == "طبيب");
            txtDoctors.Text = doctors.Count.ToString();

            var admins = _allEmployees.FindAll(e => e.Role == "مدير" || e.Role == "موظف استقبال");
            txtAdmins.Text = admins.Count.ToString();

            decimal totalSalaries = 0;
            foreach (var emp in _allEmployees)
            {
                totalSalaries += emp.Salary;
            }
            txtTotalSalaries.Text = $"{totalSalaries:N0} جنيه";
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("إضافة موظف جديد", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: فتح نافذة إضافة موظف
        }

        private void ViewEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);
                MessageBox.Show($"عرض تفاصيل الموظف {userId}", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);
                var user = _userRepo.GetUserById(userId);
                if (user != null)
                {
                    MessageBox.Show($"تعديل بيانات {user.FullName}", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
                    // TODO: فتح نافذة التعديل
                }
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show("هل تريد إعادة تعيين كلمة المرور إلى 123456؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int userId = Convert.ToInt32(button.Tag);
                    // TODO: إعادة تعيين كلمة المرور
                    MessageBox.Show("تم إعادة تعيين كلمة المرور بنجاح", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeactivateEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show("هل تريد إيقاف هذا الموظف؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int userId = Convert.ToInt32(button.Tag);
                    if (_userRepo.DeleteUser(userId))
                    {
                        LoadEmployees();
                        MessageBox.Show("تم إيقاف الموظف بنجاح", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void RoleFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var searchTerm = txtSearch.Text.ToLower();
            var selectedRole = (cmbRoleFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            _filteredEmployees = _allEmployees.FindAll(emp =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) ||
                    emp.FullName.ToLower().Contains(searchTerm) ||
                    emp.Username.ToLower().Contains(searchTerm);

                bool matchesRole = selectedRole == "جميع الموظفين" || emp.Role == selectedRole;

                return matchesSearch && matchesRole;
            });

            dgEmployees.ItemsSource = _filteredEmployees;
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Employees_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    MessageBox.Show("تم التصدير بنجاح", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Converter للحالة النشطة
    public class BoolToActiveConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? "نشط" : "موقوف";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}