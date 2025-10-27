
            // ====================================
            // App.xaml.cs - بداية التطبيق
            // ====================================

            using System;
            using System.Windows;
            using System.IO;
            using ClinicManagementSystem.Data;

namespace ClinicManagementSystem
    {
        public partial class App : Application
        {
            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);

                // معالج للأخطاء غير المتوقعة
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

                try
                {
                    // التأكد من وجود قاعدة البيانات
                    InitializeDatabase();

                    // إنشاء مجلدات التطبيق
                    CreateApplicationFolders();

                    // فتح نافذة تسجيل الدخول
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"حدث خطأ أثناء بدء التطبيق:\n{ex.Message}",
                        "خطأ فادح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Application.Current.Shutdown();
                }
            }

            private void InitializeDatabase()
            {
                // هذا سيقوم بإنشاء قاعدة البيانات تلقائياً إذا لم تكن موجودة
                var dbHelper = DatabaseHelper.Instance;

                // يمكن إضافة كود للتحقق من الإصدار وتحديث الجداول إذا لزم الأمر
            }

            private void CreateApplicationFolders()
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ClinicManagementSystem");

                // مجلد الصور
                string imagesPath = Path.Combine(appDataPath, "Images");
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                // مجلد المستندات
                string documentsPath = Path.Combine(appDataPath, "Documents");
                if (!Directory.Exists(documentsPath))
                    Directory.CreateDirectory(documentsPath);

                // مجلد التقارير
                string reportsPath = Path.Combine(appDataPath, "Reports");
                if (!Directory.Exists(reportsPath))
                    Directory.CreateDirectory(reportsPath);

                // مجلد النسخ الاحتياطية
                string backupsPath = Path.Combine(appDataPath, "Backups");
                if (!Directory.Exists(backupsPath))
                    Directory.CreateDirectory(backupsPath);

                // مجلد السجلات
                string logsPath = Path.Combine(appDataPath, "Logs");
                if (!Directory.Exists(logsPath))
                    Directory.CreateDirectory(logsPath);
            }

            private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                if (e.ExceptionObject is Exception ex)
                {
                    LogError(ex);
                    MessageBox.Show(
                        $"حدث خطأ غير متوقع:\n{ex.Message}\n\nسيتم إغلاق التطبيق.",
                        "خطأ فادح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
            {
                LogError(e.Exception);

                MessageBox.Show(
                    $"حدث خطأ:\n{e.Exception.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                e.Handled = true;
            }

            private void LogError(Exception ex)
            {
                try
                {
                    string logPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "ClinicManagementSystem",
                        "Logs",
                        $"Error_{DateTime.Now:yyyyMMdd}.log");

                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n" +
                                      $"Message: {ex.Message}\n" +
                                      $"StackTrace: {ex.StackTrace}\n" +
                                      $"Source: {ex.Source}\n\n";

                    File.AppendAllText(logPath, logMessage);
                }
                catch
                {
                    // تجاهل الأخطاء في تسجيل الأخطاء
                }
            }

            protected override void OnExit(ExitEventArgs e)
            {
                // يمكن إضافة كود التنظيف هنا
                base.OnExit(e);
            }
        }
    }