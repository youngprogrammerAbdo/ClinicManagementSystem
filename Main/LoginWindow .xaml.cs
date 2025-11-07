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
            LoadSavedUsername();
            txtUsername.Focus();
        }

        private void LoadSavedUsername()
        {
            try
            {
                Settings.Default.Reload();
                string savedUsername = Settings.Default.SavedUsername;
                bool rememberMe = Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername))
                {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.IsChecked = true;
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في تحميل اسم المستخدم: {ex.Message}");
            }
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
            txtError.Visibility = Visibility.Collapsed;

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
                btnLogin.IsEnabled = false;
                btnLogin.Content = "جاري التحقق...";

                var user = _userRepo.Login(txtUsername.Text, txtPassword.Password);

                if (user != null)
                {
                    // حفظ اسم المستخدم
                    if (chkRememberMe.IsChecked == true)
                    {
                        Settings.Default.SavedUsername = txtUsername.Text;
                        Settings.Default.RememberMe = true;
                    }
                    else
                    {
                        Settings.Default.SavedUsername = string.Empty;
                        Settings.Default.RememberMe = false;
                    }
                    Settings.Default.Save();

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