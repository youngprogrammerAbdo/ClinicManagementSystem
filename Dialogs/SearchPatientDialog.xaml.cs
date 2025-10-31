// ====================================
// SearchPatientDialog.xaml.cs
// ====================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;

namespace ClinicManagementSystem.Dialogs
{
    public partial class SearchPatientDialog : Window
    {
        private readonly PatientRepository _patientRepo;
        private List<Patient> _allPatients;
        public Patient SelectedPatient { get; private set; }

        public SearchPatientDialog()
        {
            InitializeComponent();
            _patientRepo = new PatientRepository();
            LoadAllPatients();
            txtSearch.Focus();
        }

        private void LoadAllPatients()
        {
            _allPatients = _patientRepo.GetAllPatients();
            dgResults.ItemsSource = _allPatients;
            UpdateResultCount();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgResults.ItemsSource = _allPatients;
            }
            else
            {
                var searchTerm = txtSearch.Text.ToLower();
                var results = _allPatients.Where(p =>
                    p.FullName.ToLower().Contains(searchTerm) ||
                    p.PatientCode.ToLower().Contains(searchTerm) ||
                    p.PhoneNumber.Contains(searchTerm) ||
                    (p.NationalID != null && p.NationalID.Contains(searchTerm))
                ).ToList();

                dgResults.ItemsSource = results;
            }

            UpdateResultCount();
        }

        private void UpdateResultCount()
        {
            var count = dgResults.Items.Count;
            txtResultCount.Text = count.ToString();
            pnlNoResults.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (dgResults.SelectedItem is Patient patient)
            {
                SelectedPatient = patient;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("الرجاء اختيار مريض", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgResults.SelectedItem is Patient patient)
            {
                SelectedPatient = patient;
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}