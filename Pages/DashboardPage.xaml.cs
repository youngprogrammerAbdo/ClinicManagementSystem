// ====================================
// DashboardPage.xaml.cs
// ====================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly PatientRepository _patientRepo;
        private readonly VisitRepository _visitRepo;
        private readonly InvoiceRepository _invoiceRepo;

        public DashboardPage()
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            _visitRepo = new VisitRepository();
            _invoiceRepo = new InvoiceRepository();

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // إحصائيات عامة
                LoadStatistics();

                // دور اليوم
                LoadTodayQueue();

                // آخر الزيارات
                LoadRecentVisits();

                // التنبيهات
                LoadAlerts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            // إجمالي المرضى
            txtTotalPatients.Text = _patientRepo.GetTotalPatientsCount().ToString();

            // زيارات اليوم
            txtTodayVisits.Text = _visitRepo.GetTodayVisitsCount().ToString();

            // إيرادات الشهر
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var monthlyRevenue = _invoiceRepo.GetTotalRevenue(firstDayOfMonth, lastDayOfMonth);
            txtMonthlyRevenue.Text = $"{monthlyRevenue:N0} جنيه";

            // إجمالي الديون
            var totalDebts = _invoiceRepo.GetTotalDebts();
            txtTotalDebts.Text = $"{totalDebts:N0} جنيه";
        }

        private void LoadTodayQueue()
        {
            var todayVisits = _visitRepo.GetTodayVisits();

            // تحضير البيانات للعرض
            var queueData = new List<dynamic>();
            foreach (var visit in todayVisits)
            {
                queueData.Add(new
                {
                    QueueNumber = visit.QueueNumber,
                    PatientName = visit.Patient?.FullName ?? "غير محدد",
                    VisitType = visit.VisitType,
                    VisitStatus = visit.VisitStatus
                });
            }

            dgTodayQueue.ItemsSource = queueData;
        }

        private void LoadRecentVisits()
        {
            var recentVisits = _visitRepo.GetVisitsByDateRange(
                DateTime.Now.AddDays(-7),
                DateTime.Now);

            // أخذ آخر 10 زيارات
            if (recentVisits.Count > 10)
            {
                recentVisits = recentVisits.GetRange(0, 10);
            }

            var visitsData = new List<dynamic>();
            foreach (var visit in recentVisits)
            {
                visitsData.Add(new
                {
                    VisitDate = visit.VisitDate,
                    PatientName = visit.Patient?.FullName ?? "غير محدد",
                    Diagnosis = visit.Diagnosis ?? "لم يتم التشخيص",
                    VisitStatus = visit.VisitStatus
                });
            }

            dgRecentVisits.ItemsSource = visitsData;
        }

        private void LoadAlerts()
        {
            pnlAlerts.Children.Clear();

            // تنبيهات المخزون المنخفض
            var inventoryRepo = new InventoryRepository();
            var lowStockItems = inventoryRepo.GetLowStockItems();

            if (lowStockItems.Count > 0)
            {
                AddAlert($"⚠️ {lowStockItems.Count} صنف ينخفض عن الحد الأدنى",
                    Brushes.Orange);
            }

            // تنبيهات الصلاحية
            var expiringSoon = inventoryRepo.GetExpiringSoonItems(30);
            if (expiringSoon.Count > 0)
            {
                AddAlert($"⏰ {expiringSoon.Count} صنف قارب على الانتهاء",
                    Brushes.Red);
            }

            // تنبيهات المواعيد القادمة
            var appointmentRepo = new AppointmentRepository();
            var upcomingAppointments = appointmentRepo.GetUpcomingAppointments(1);

            if (upcomingAppointments.Count > 0)
            {
                AddAlert($"📅 {upcomingAppointments.Count} موعد غداً",
                    Brushes.Blue);
            }

            // تنبيهات الديون
            var patientsWithDebts = _patientRepo.GetPatientsWithDebts();
            if (patientsWithDebts.Count > 0)
            {
                AddAlert($"💰 {patientsWithDebts.Count} مريض عليه ديون",
                    Brushes.Red);
            }

            // رسالة إذا لم يكن هناك تنبيهات
            if (pnlAlerts.Children.Count == 0)
            {
                AddAlert("✅ لا توجد تنبيهات", Brushes.Green);
            }
        }

        private void AddAlert(string message, Brush color)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30,
                    ((SolidColorBrush)color).Color.R,
                    ((SolidColorBrush)color).Color.G,
                    ((SolidColorBrush)color).Color.B)),
                BorderBrush = color,
                BorderThickness = new Thickness(2, 0, 0, 0),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                CornerRadius = new CornerRadius(5)
            };

            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 13,
                Foreground = color,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            pnlAlerts.Children.Add(border);
        }

        // ====================================
        // Quick Actions
        // ====================================
        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.PatientDialog();
            dialog.ShowDialog();
            LoadDashboardData(); // تحديث البيانات
        }

        private void NewVisit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.VisitDialog();
            dialog.ShowDialog();
            LoadDashboardData();
        }

        private void NewInvoice_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.InvoiceDialog();
            dialog.ShowDialog();
            LoadDashboardData();
        }

        private void NewAppointment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.AppointmentDialog();
            dialog.ShowDialog();
            LoadDashboardData();
        }

        private void SearchPatient_Click(object sender, RoutedEventArgs e)
        {
            var searchDialog = new Dialogs.SearchPatientDialog();
            searchDialog.ShowDialog();
        }

        private void ViewAllQueue_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new QueuePage());
        }
    }
}