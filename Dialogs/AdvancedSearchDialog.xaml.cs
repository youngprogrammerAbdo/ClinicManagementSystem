// ====================================
// AdvancedSearchDialog.xaml.cs
// ====================================
using System;
using System.Windows;

namespace ClinicManagementSystem.Dialogs
{
    public partial class AdvancedSearchDialog : Window
    {
        public string SearchName { get; private set; }
        public string SearchPhone { get; private set; }
        public string SearchNationalID { get; private set; }
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }

        public AdvancedSearchDialog()
        {
            InitializeComponent();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchName = txtName.Text;
            SearchPhone = txtPhone.Text;
            SearchNationalID = txtNationalID.Text;
            FromDate = dpFromDate.SelectedDate;
            ToDate = dpToDate.SelectedDate;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            txtName.Clear();
            txtPhone.Clear();
            txtNationalID.Clear();
            dpFromDate.SelectedDate = null;
            dpToDate.SelectedDate = null;
        }
    }
}