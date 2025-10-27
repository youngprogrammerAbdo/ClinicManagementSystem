// ====================================
// QueuePage.xaml.cs
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Dialogs;

namespace ClinicManagementSystem.Pages
{
    public partial class QueuePage : Page
    {
        private readonly VisitRepository _visitRepo;
        private readonly PatientRepository _patientRepo;
        private DispatcherTimer _refreshTimer;
        private Visit _currentVisit;

        public QueuePage()
        {
            InitializeComponent();
            _visitRepo = new VisitRepository();
            _patientRepo = new PatientRepository();

            UpdateDateTime();
            LoadQueue();
            StartAutoRefresh();
        }

        private void UpdateDateTime()
        {
            txtCurrentDate.Text = DateTime.Now.ToString("dddd، dd MMMM yyyy",
                new System.Globalization.CultureInfo("ar-EG"));
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(30); // تحديث كل 30 ثانية
            _refreshTimer.Tick += (s, e) => LoadQueue();
            _refreshTimer.Start();
        }

        private void LoadQueue()
        {
            try
            {
                var todayVisits = _visitRepo.GetTodayVisits();

                // تحميل البيانات
                var queueData = new List<QueueDisplay>();
                foreach (var visit in todayVisits)
                {
                    var patient = _patientRepo.GetPatientById(visit.PatientID);
                    queueData.Add(new QueueDisplay
                    {
                        VisitID = visit.VisitID,
                        QueueNumber = visit.QueueNumber ?? 0,
                        PatientName = patient?.FullName ?? "غير محدد",
                        VisitType = visit.VisitType,
                        VisitDate = visit.VisitDate,
                        VisitStatus = visit.VisitStatus
                    });
                }

                dgQueue.ItemsSource = queueData.OrderBy(q => q.QueueNumber).ToList();

                // تحديث الإحصائيات
                UpdateStatistics(queueData);

                // تحديث المريض الحالي والتالي
                UpdateCurrentPatient();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الدور: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics(List<QueueDisplay> queue)
        {
            int waitingCount = queue.Count(q => q.VisitStatus == "منتظر");
            int inProgressCount = queue.Count(q => q.VisitStatus == "جاري الكشف");
            int completedCount = queue.Count(q => q.VisitStatus == "منتهي");

            txtWaitingCount.Text = waitingCount.ToString();
            txtInProgressCount.Text = inProgressCount.ToString();
            txtCompletedCount.Text = completedCount.ToString();
            txtTotalVisits.Text = queue.Count.ToString();

            // حساب متوسط وقت الانتظار (تقريبي)
            if (completedCount > 0)
            {
                txtAverageWaitTime.Text = $"{completedCount * 15} دقيقة"; // تقدير
            }
            else
            {
                txtAverageWaitTime.Text = "0 دقيقة";
            }
        }

        private void UpdateCurrentPatient()
        {
            _currentVisit = _visitRepo.GetCurrentPatientInQueue();

            if (_currentVisit != null)
            {
                txtCurrentQueueNumber.Text = _currentVisit.QueueNumber?.ToString() ?? "--";
                var patient = _patientRepo.GetPatientById(_currentVisit.PatientID);
                txtCurrentPatientName.Text = patient?.FullName ?? "غير محدد";
            }
            else
            {
                txtCurrentQueueNumber.Text = "--";
                txtCurrentPatientName.Text = "لا يوجد";
            }

            // التالي في الدور
            var nextVisit = _visitRepo.GetNextPatientInQueue();
            if (nextVisit != null)
            {
                txtNextQueueNumber.Text = nextVisit.QueueNumber?.ToString() ?? "--";
                var patient = _patientRepo.GetPatientById(nextVisit.PatientID);
                txtNextPatientName.Text = patient?.FullName ?? "غير محدد";
                btnCallNext.IsEnabled = true;
            }
            else
            {
                txtNextQueueNumber.Text = "--";
                txtNextPatientName.Text = "لا يوجد";
                btnCallNext.IsEnabled = false;
            }
        }

        // ====================================
        // Actions
        // ====================================
        private void AddToQueue_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VisitDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadQueue();
                MessageBox.Show("تم إضافة المريض للدور بنجاح", "نجح",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadQueue();
        }

        private void StartExamination_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int visitId = Convert.ToInt32(button.Tag);

                if (_visitRepo.UpdateVisitStatus(visitId, "جاري الكشف"))
                {
                    LoadQueue();
                    MessageBox.Show("تم بدء الكشف", "نجح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void CompleteExamination_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int visitId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show(
                    "هل تم الانتهاء من الكشف؟",
                    "تأكيد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_visitRepo.UpdateVisitStatus(visitId, "منتهي"))
                    {
                        LoadQueue();
                        MessageBox.Show("تم إنهاء الكشف", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void CancelVisit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من إلغاء هذه الزيارة؟",
                    "تأكيد الإلغاء",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int visitId = Convert.ToInt32(button.Tag);
                    if (_visitRepo.UpdateVisitStatus(visitId, "ملغي"))
                    {
                        LoadQueue();
                        MessageBox.Show("تم إلغاء الزيارة", "نجح",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void CallNext_Click(object sender, RoutedEventArgs e)
        {
            var nextVisit = _visitRepo.GetNextPatientInQueue();
            if (nextVisit != null)
            {
                // إنهاء المريض الحالي إذا كان موجود
                if (_currentVisit != null)
                {
                    _visitRepo.UpdateVisitStatus(_currentVisit.VisitID, "منتهي");
                }

                // بدء الكشف للمريض التالي
                _visitRepo.UpdateVisitStatus(nextVisit.VisitID, "جاري الكشف");

                LoadQueue();

                // يمكن إضافة صوت نداء هنا
                MessageBox.Show($"تم استدعاء المريض رقم {nextVisit.QueueNumber}",
                    "استدعاء", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RepeatCall_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVisit != null)
            {
                MessageBox.Show($"إعادة استدعاء المريض رقم {_currentVisit.QueueNumber}",
                    "استدعاء", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("لا يوجد مريض حالي", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewPatientDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVisit != null)
            {
                var dialog = new PatientDetailsDialog(_currentVisit.PatientID);
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("لا يوجد مريض حالي", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NewPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVisit != null)
            {
                var dialog = new PrescriptionDialog(_currentVisit.VisitID);
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("لا يوجد مريض حالي", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVisit != null)
            {
                var dialog = new InvoiceDialog(_currentVisit.PatientID, _currentVisit.VisitID);
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("لا يوجد مريض حالي", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    // Helper class for display
    public class QueueDisplay
    {
        public int VisitID { get; set; }
        public int QueueNumber { get; set; }
        public string PatientName { get; set; }
        public string VisitType { get; set; }
        public DateTime VisitDate { get; set; }
        public string VisitStatus { get; set; }
    }
}