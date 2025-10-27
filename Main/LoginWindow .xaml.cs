// ====================================
// LoginWindow.xaml.cs
// ====================================

using System;
using System.Windows;
using System.Windows.Input;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem
{
    public partial class LoginWindow : Window
    {
        private readonly UserRepository _userRepo;

        public LoginWindow()
        {
            InitializeComponent();
            _userRepo = new UserRepository();

            // تحميل اسم المستخدم المحفوظ
            LoadSavedUsername();

            txtUsername.Focus();
        }

        private void LoadSavedUsername()
        {
            try
            {
                string savedUsername = Properties.Settings.Default.SavedUsername;
                bool rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.IsChecked = true;
                    txtPassword.Focus();
                }
            }
            catch { }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
        {
            // إخفاء رسالة الخطأ
            txtError.Visibility = Visibility.Collapsed;

            // التحقق من إدخال البيانات
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("الرجاء إدخال اسم المستخدم");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                ShowError("الرجاء إدخال كلمة المرور");
                txtPassword.Focus();
                return;
            }

            try
            {
                // تعطيل زر الدخول أثناء المعالجة
                btnLogin.IsEnabled = false;
                btnLogin.Content = "جاري التحقق...";

                // محاولة تسجيل الدخول
                var user = _userRepo.Login(txtUsername.Text, txtPassword.Password);

                if (user != null)
                {
                    // نجح تسجيل الدخول

                    // حفظ اسم المستخدم إذا كان الخيار مفعل
                    if (chkRememberMe.IsChecked == true)
                    {
                        Properties.Settings.Default.SavedUsername = txtUsername.Text;
                        Properties.Settings.Default.RememberMe = true;
                    }
                    else
                    {
                        Properties.Settings.Default.SavedUsername = string.Empty;
                        Properties.Settings.Default.RememberMe = false;
                    }
                    Properties.Settings.Default.Save();

                    // تسجيل النشاط
                    _userRepo.LogActivity(user.UserID, "تسجيل دخول", "Users", user.UserID,
                        $"تسجيل دخول المستخدم {user.Username}");

                    // فتح النافذة الرئيسية
                    var mainWindow = new MainWindow();
                    MainWindow.CurrentUser = user;
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // فشل تسجيل الدخول
                    ShowError("اسم المستخدم أو كلمة المرور غير صحيحة");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"حدث خطأ: {ex.Message}");
            }
            finally
            {
                // إعادة تفعيل زر الدخول
                btnLogin.IsEnabled = true;
                btnLogin.Content = "تسجيل الدخول";
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}