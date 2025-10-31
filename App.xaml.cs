using System;
using System.Windows;
using ClinicManagementSystem.Data;

namespace ClinicManagementSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 👈 استدعاء Initialize بشكل صريح
                DatabaseHelper.Instance.Initialize();

                Console.WriteLine("✅ تم تهيئة قاعدة البيانات بنجاح");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"فشل في تهيئة قاعدة البيانات:\n\n{ex.Message}\n\nالمسار: {ex.StackTrace}",
                    "خطأ خطير",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                // إغلاق البرنامج
                Shutdown();
            }
        }
    }
}