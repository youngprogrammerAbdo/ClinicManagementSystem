// ====================================
// SurgeriesPage.xaml.cs
// ====================================
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Pages
{
    public partial class SurgeriesPage : Page
    {
        private List<dynamic> _allSurgeries;
        private List<dynamic> _filteredSurgeries;

        public SurgeriesPage()
        {
            InitializeComponent();
            LoadSurgeries();
        }

        private void LoadSurgeries()
        {
            try
            {
                // TODO: تحميل العمليات من قاعدة البيانات
                _allSurgeries = new List<dynamic>();
                _filteredSurgeries = new List<dynamic>(_allSurgeries);

                dgSurgeries.ItemsSource = _filteredSurgeries;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            txtTotalSurgeries.Text = _allSurgeries.Count.ToString();
            // TODO: حساب الإحصائيات الأخرى
            txtScheduled.Text = "0";
            txtCompleted.Text = "0";
            txtCancelled.Text = "0";
        }

        private void AddSurgery_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("إضافة عملية جديدة", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: فتح نافذة إضافة عملية
        }

        private void ViewSurgery_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int surgeryId = Convert.ToInt32(button.Tag);
                MessageBox.Show($"عرض تفاصيل العملية {surgeryId}", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditSurgery_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int surgeryId = Convert.ToInt32(button.Tag);
                MessageBox.Show($"تعديل العملية {surgeryId}", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelSurgery_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show("هل تريد إلغاء هذه العملية؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int surgeryId = Convert.ToInt32(button.Tag);
                    // TODO: إلغاء العملية
                    LoadSurgeries();
                }
            }
        }

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
            // TODO: تطبيق الفلاتر
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تصدير Excel", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: تصدير إلى Excel
        }
    }
}
