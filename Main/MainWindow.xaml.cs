using ClinicManagementSystem.Models;
using ClinicManagementSystem.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicManagementSystem
{
    public partial class MainWindow : Window
    {
        public static User CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // عرض اسم المستخدم
            if (CurrentUser != null)
            {
                txtUsername.Text = CurrentUser.FullName;
            }

            LoadDashboard();
            UpdateDateTime();

            // تحديث التاريخ كل دقيقة
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) => UpdateDateTime();
            timer.Start();
        }

        private void UpdateDateTime()
        {
            txtCurrentDate.Text = DateTime.Now.ToString("dddd، dd MMMM yyyy",
                new System.Globalization.CultureInfo("ar-EG"));
        }

        private void LoadDashboard()
        {
            txtPageTitle.Text = "لوحة التحكم";
            MainFrame.Navigate(new DashboardPage());
        }

        // Navigation Events
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "لوحة التحكم";
            MainFrame.Navigate(new DashboardPage());
        }

        private void Patients_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة المرضى";
            MainFrame.Navigate(new PatientsPage());
        }

        private void Queue_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "نظام الدور";
            MainFrame.Navigate(new QueuePage());
        }

        private void Appointments_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة المواعيد";
            MainFrame.Navigate(new AppointmentsPage());
        }

        private void Surgeries_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة العمليات";
            MainFrame.Navigate(new SurgeriesPage());
        }

        private void Invoices_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة الفواتير";
            MainFrame.Navigate(new InvoicesPage());
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة المخزون";
            MainFrame.Navigate(new InventoryPage());
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "التقارير";
            MainFrame.Navigate(new ReportsPage());
        }

        private void Employees_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "إدارة الموظفين";
            MainFrame.Navigate(new EmployeesPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "الإعدادات";
            MainFrame.Navigate(new SettingsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من تسجيل الخروج؟",
                "تسجيل الخروج",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // لا نمسح البيانات المحفوظة - نتركها كما هي
                    // المستخدم اختار "تذكرني" من قبل
                    CurrentUser = null;

                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}