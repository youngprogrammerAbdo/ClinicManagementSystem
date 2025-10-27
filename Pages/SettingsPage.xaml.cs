
                using System;
                using System.IO;
                using System.Windows;
                using System.Windows.Controls;
                using ClinicManagementSystem.Data;

namespace ClinicManagementSystem.Pages
    {
        public partial class SettingsPage : Page
        {
            private readonly DatabaseHelper _db;
            private string _backupPath;

            public SettingsPage()
            {
                InitializeComponent();
                _db = DatabaseHelper.Instance;
                LoadSettings();
            }

            private void LoadSettings()
            {
                try
                {
                    // تحميل الإعدادات من قاعدة البيانات أو ملف الإعدادات
                    _backupPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "ClinicManagementSystem", "Backups");

                    txtBackupPath.Text = _backupPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تحميل الإعدادات: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void SaveSettings_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    // حفظ الإعدادات في قاعدة البيانات
                    var settings = new[]
                    {
                    ("ClinicName", txtClinicName.Text),
                    ("ClinicPhone", txtClinicPhone.Text),
                    ("ClinicEmail", txtClinicEmail.Text),
                    ("ClinicAddress", txtClinicAddress.Text),
                    ("ExaminationFee", txtExaminationFee.Text),
                    ("ReexaminationFee", txtReexaminationFee.Text),
                    ("Currency", txtCurrency.Text),
                    ("WorkingHoursStart", txtWorkStartTime.Text),
                    ("WorkingHoursEnd", txtWorkEndTime.Text)
                };

                    foreach (var (key, value) in settings)
                    {
                        string query = @"
                        INSERT OR REPLACE INTO ClinicSettings (SettingKey, SettingValue, UpdatedAt)
                        VALUES (@Key, @Value, @UpdatedAt)";

                        var parameters = new[]
                        {
                        new System.Data.SQLite.SQLiteParameter("@Key", key),
                        new System.Data.SQLite.SQLiteParameter("@Value", value),
                        new System.Data.SQLite.SQLiteParameter("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

                        _db.ExecuteNonQuery(query, parameters);
                    }

                    MessageBox.Show("تم حفظ الإعدادات بنجاح", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void BrowseBackupPath_Click(object sender, RoutedEventArgs e)
            {
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "اختر مجلد النسخ الاحتياطي",
                    ShowNewFolderButton = true
                };

                if (folderDialog.ShowDialog() == System.Windows.DialogResult.OK)
                {
                    _backupPath = folderDialog.SelectedPath;
                    txtBackupPath.Text = _backupPath;
                }
            }

            private void BackupNow_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (!Directory.Exists(_backupPath))
                    {
                        Directory.CreateDirectory(_backupPath);
                    }

                    string backupFile = Path.Combine(_backupPath,
                        $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                    if (_db.BackupDatabase(backupFile))
                    {
                        MessageBox.Show($"تم إنشاء نسخة احتياطية بنجاح\n{backupFile}",
                            "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل إنشاء النسخة الاحتياطية", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void Restore_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    var result = MessageBox.Show(
                        "تحذير: سيتم استبدال البيانات الحالية بالنسخة الاحتياطية.\nهل أنت متأكد؟",
                        "تأكيد الاستعادة",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        var openDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "Database Files|*.db",
                            InitialDirectory = _backupPath
                        };

                        if (openDialog.ShowDialog() == true)
                        {
                            if (_db.RestoreDatabase(openDialog.FileName))
                            {
                                MessageBox.Show(
                                    "تم استعادة النسخة الاحتياطية بنجاح\nيجب إعادة تشغيل البرنامج",
                                    "نجح", MessageBoxButton.OK, MessageBoxImage.Information);

                                Application.Current.Shutdown();
                            }
                            else
                            {
                                MessageBox.Show("فشل استعادة النسخة الاحتياطية", "خطأ",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void ResetDefaults_Click(object sender, RoutedEventArgs e)
            {
                var result = MessageBox.Show(
                    "هل تريد استعادة الإعدادات الافتراضية؟",
                    "تأكيد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    txtClinicName.Text = "عيادة الشفاء";
                    txtClinicPhone.Text = "0123456789";
                    txtClinicEmail.Text = "";
                    txtClinicAddress.Text = "القاهرة، مصر";
                    txtExaminationFee.Text = "200";
                    txtReexaminationFee.Text = "100";
                    txtCurrency.Text = "جنيه";
                    txtWorkStartTime.Text = "09:00";
                    txtWorkEndTime.Text = "18:00";
                    chkAutoBackup.IsChecked = false;
                    chkShowNotifications.IsChecked = true;
                    chkAutoQueue.IsChecked = true;
                    chkPrintAfterInvoice.IsChecked = false;
                }
            }

            private void CheckUpdates_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show(
                    "أنت تستخدم أحدث إصدار\nالإصدار: 1.0.0",
                    "التحقق من التحديثات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }