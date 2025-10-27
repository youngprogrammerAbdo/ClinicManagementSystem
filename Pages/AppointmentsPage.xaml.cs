using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Pages
{
    public partial class AppointmentsPage : Page
    {
        private readonly AppointmentRepository _appointmentRepo;
        private readonly PatientRepository _patientRepo;
        private DateTime _selectedDate;

        public AppointmentsPage()
        {
            InitializeComponent();
            _appointmentRepo = new AppointmentRepository();
            _patientRepo = new PatientRepository();
            _selectedDate = DateTime.Today;
            LoadAppointments();
            LoadStatistics();
        }

        private void LoadAppointments()
        {
            try
            {
                var appointments = _appointmentRepo.GetAppointmentsByDate(_selectedDate);
                var displayList = new List<dynamic>();

                foreach (var apt in appointments)
                {
                    var patient = _patientRepo.GetPatientById(apt.PatientID);
                    displayList.Add(new
                    {
                        AppointmentID = apt.AppointmentID,
                        AppointmentTime = apt.AppointmentTime.ToString(@"hh\:mm"),
                        PatientName = patient?.FullName ?? "غير محدد",
                        PhoneNumber = patient?.PhoneNumber ?? "",
                        AppointmentType = apt.AppointmentType,
                        Status = apt.Status
                    });
                }

                dgAppointments.ItemsSource = displayList;
                txtSelectedDate.Text = $"مواعيد {_selectedDate:dd/MM/yyyy}";
                txtAppointmentCount.Text = $"{displayList.Count} موعد";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            var todayCount = _appointmentRepo.GetAppointmentsByDate(DateTime.Today).Count;
            var weekCount = _appointmentRepo.GetAppointmentsByDateRange(DateTime.Today, DateTime.Today.AddDays(7)).Count;
            var noShowCount = _appointmentRepo.GetAppointmentsCountByStatus("لم يحضر");

            txtTodayCount.Text = $"{todayCount} موعد";
            txtWeekCount.Text = $"{weekCount} موعد";
            txtNoShowCount.Text = noShowCount.ToString();
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendarView.SelectedDate.HasValue)
            {
                _selectedDate = calendarView.SelectedDate.Value;
                LoadAppointments();
            }
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadAppointments();
        }

        private void NewAppointment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("حجز موعد - AppointmentDialog", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MarkAttended_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int appointmentId = Convert.ToInt32(button.Tag);
                _appointmentRepo.UpdateStatus(appointmentId, "حضر");
                LoadAppointments();
                LoadStatistics();
            }
        }

        private void EditAppointment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تعديل موعد", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show("هل تريد إلغاء هذا الموعد؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    int appointmentId = Convert.ToInt32(button.Tag);
                    _appointmentRepo.UpdateStatus(appointmentId, "ملغي");
                    LoadAppointments();
                }
            }
        }
    }
}