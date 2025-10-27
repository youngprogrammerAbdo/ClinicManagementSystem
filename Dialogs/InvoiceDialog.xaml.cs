// ====================================
// InvoiceDialog.xaml.cs
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Helpers;

namespace ClinicManagementSystem.Dialogs
{
    public partial class InvoiceDialog : Window
    {
        private readonly InvoiceRepository _invoiceRepo;
        private readonly PatientRepository _patientRepo;
        private List<InvoiceItemRow> _items;
        private int? _patientId;
        private int? _visitId;

        public InvoiceDialog(int? patientId = null, int? visitId = null)
        {
            InitializeComponent();
            _invoiceRepo = new InvoiceRepository();
            _patientRepo = new PatientRepository();
            _items = new List<InvoiceItemRow>();
            _patientId = patientId;
            _visitId = visitId;

            LoadPatients();
            AddInitialItem();
        }

        private void LoadPatients()
        {
            var patients = _patientRepo.GetAllPatients();
            cmbPatient.ItemsSource = patients;

            if (_patientId.HasValue)
            {
                cmbPatient.SelectedValue = _patientId.Value;
            }
        }

        private void AddInitialItem()
        {
            // إضافة بند افتراضي
            AddItemRow("كشف", 1, 200);
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            AddItemRow("", 1, 0);
        }

        private void AddItemRow(string description, int quantity, decimal price)
        {
            var itemRow = new Grid
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            itemRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            itemRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            itemRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            itemRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            itemRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

            // Description
            var txtDescription = new TextBox
            {
                Text = description,
                Padding = new Thickness(5),
                Margin = new Thickness(5)
            };
            Grid.SetColumn(txtDescription, 0);
            itemRow.Children.Add(txtDescription);

            // Quantity
            var txtQuantity = new TextBox
            {
                Text = quantity.ToString(),
                Padding = new Thickness(5),
                Margin = new Thickness(5)
            };
            txtQuantity.TextChanged += (s, e) => CalculateTotals();
            Grid.SetColumn(txtQuantity, 1);
            itemRow.Children.Add(txtQuantity);

            // Unit Price
            var txtUnitPrice = new TextBox
            {
                Text = price.ToString(),
                Padding = new Thickness(5),
                Margin = new Thickness(5)
            };
            txtUnitPrice.TextChanged += (s, e) => CalculateTotals();
            Grid.SetColumn(txtUnitPrice, 2);
            itemRow.Children.Add(txtUnitPrice);

            // Total
            var txtItemTotal = new TextBox
            {
                IsReadOnly = true,
                Background = System.Windows.Media.Brushes.LightGray,
                Padding = new Thickness(5),
                Margin = new Thickness(5)
            };
            Grid.SetColumn(txtItemTotal, 3);
            itemRow.Children.Add(txtItemTotal);

            // Delete Button
            var btnDelete = new Button
            {
                Content = "🗑️",
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(5)
            };
            btnDelete.Click += (s, e) =>
            {
                pnlInvoiceItems.Children.Remove(itemRow);
                CalculateTotals();
            };
            Grid.SetColumn(btnDelete, 4);
            itemRow.Children.Add(btnDelete);

            pnlInvoiceItems.Children.Add(itemRow);

            var item = new InvoiceItemRow
            {
                ItemRow = itemRow,
                DescriptionBox = txtDescription,
                QuantityBox = txtQuantity,
                UnitPriceBox = txtUnitPrice,
                TotalBox = txtItemTotal
            };
            _items.Add(item);

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal total = 0;

            foreach (var item in _items)
            {
                if (item.ItemRow.Parent == null) continue; // تم حذفه

                if (decimal.TryParse(item.QuantityBox.Text, out decimal quantity) &&
                    decimal.TryParse(item.UnitPriceBox.Text, out decimal unitPrice))
                {
                    decimal itemTotal = quantity * unitPrice;
                    item.TotalBox.Text = itemTotal.ToString("F2");
                    total += itemTotal;
                }
            }

            txtTotal.Text = $"{total:N2} جنيه";

            // حساب الخصم
            decimal discount = 0;
            if (decimal.TryParse(txtDiscountAmount.Text, out decimal discountAmount))
            {
                discount = discountAmount;
            }

            decimal netAmount = total - discount;
            txtNetAmount.Text = $"{netAmount:N2} جنيه";

            // حساب المتبقي
            decimal paidAmount = 0;
            if (decimal.TryParse(txtPaidAmount.Text, out decimal paid))
            {
                paidAmount = paid;
            }

            decimal remaining = netAmount - paidAmount;
            txtRemainingAmount.Text = $"{remaining:N2} جنيه";
        }

        private void DiscountAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtDiscountAmount.Text, out decimal discountAmount) &&
                decimal.TryParse(txtTotal.Text.Replace(" جنيه", "").Replace(",", ""), out decimal total))
            {
                decimal percent = (discountAmount / total) * 100;
                txtDiscountPercent.Text = percent.ToString("F2");
            }

            CalculateTotals();
        }

        private void DiscountPercent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtDiscountPercent.Text, out decimal percent) &&
                decimal.TryParse(txtTotal.Text.Replace(" جنيه", "").Replace(",", ""), out decimal total))
            {
                decimal discountAmount = (total * percent) / 100;
                txtDiscountAmount.Text = discountAmount.ToString("F2");
            }
        }

        private void PaidAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotals();
        }

        private void SearchPatient_Click(object sender, RoutedEventArgs e)
        {
            var searchDialog = new SearchPatientDialog();
            if (searchDialog.ShowDialog() == true && searchDialog.SelectedPatient != null)
            {
                cmbPatient.SelectedValue = searchDialog.SelectedPatient.PatientID;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveInvoice(false);
        }

        private void SaveAndPrint_Click(object sender, RoutedEventArgs e)
        {
            SaveInvoice(true);
        }

        private void SaveInvoice(bool print)
        {
            if (!ValidateInput())
                return;

            try
            {
                // إنشاء الفاتورة
                decimal.TryParse(txtTotal.Text.Replace(" جنيه", "").Replace(",", ""), out decimal total);
                decimal.TryParse(txtDiscountAmount.Text, out decimal discount);
                decimal.TryParse(txtNetAmount.Text.Replace(" جنيه", "").Replace(",", ""), out decimal netAmount);
                decimal.TryParse(txtPaidAmount.Text, out decimal paidAmount);
                decimal.TryParse(txtRemainingAmount.Text.Replace(" جنيه", "").Replace(",", ""), out decimal remaining);

                var invoice = new Invoice
                {
                    PatientID = (int)cmbPatient.SelectedValue,
                    InvoiceDate = dpInvoiceDate.SelectedDate.Value,
                    InvoiceType = (cmbInvoiceType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    TotalAmount = total,
                    DiscountAmount = discount,
                    DiscountPercentage = decimal.TryParse(txtDiscountPercent.Text, out decimal dp) ? dp : 0,
                    NetAmount = netAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = remaining,
                    PaymentStatus = paidAmount >= netAmount ? "مدفوع" : paidAmount > 0 ? "مدفوع جزئياً" : "غير مدفوع",
                    Notes = txtNotes.Text
                };

                var items = new List<InvoiceItem>();
                foreach (var item in _items)
                {
                    if (item.ItemRow.Parent == null) continue;

                    decimal.TryParse(item.QuantityBox.Text, out decimal qty);
                    decimal.TryParse(item.UnitPriceBox.Text, out decimal price);

                    items.Add(new InvoiceItem
                    {
                        Description = item.DescriptionBox.Text,
                        Quantity = (int)qty,
                        UnitPrice = price,
                        TotalPrice = qty * price
                    });
                }

                int invoiceId = _invoiceRepo.CreateInvoice(invoice, items);

                if (invoiceId > 0)
                {
                    // إضافة الدفعة إذا تم الدفع
                    if (paidAmount > 0)
                    {
                        var payment = new Payment
                        {
                            InvoiceID = invoiceId,
                            Amount = paidAmount,
                            PaymentMethod = (cmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content.ToString(),
                            PaymentDate = DateTime.Now
                        };
                        _invoiceRepo.AddPayment(payment);
                    }

                    if (print)
                    {
                        // طباعة الفاتورة
                        var savedInvoice = _invoiceRepo.GetInvoiceById(invoiceId);
                        var patient = _patientRepo.GetPatientById(invoice.PatientID);

                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Filter = "PDF Files|*.pdf",
                            FileName = $"Invoice_{savedInvoice.InvoiceNumber}.pdf"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            var pdfHelper = new PDFHelper();
                            pdfHelper.GenerateInvoiceReport(savedInvoice, patient, items, saveDialog.FileName);
                            System.Diagnostics.Process.Start(saveDialog.FileName);
                        }
                    }

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (cmbPatient.SelectedValue == null)
            {
                MessageBox.Show("الرجاء اختيار المريض", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_items.Count(i => i.ItemRow.Parent != null) == 0)
            {
                MessageBox.Show("الرجاء إضافة بند واحد على الأقل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    internal class InvoiceItemRow
    {
        public Grid ItemRow { get; set; }
        public TextBox DescriptionBox { get; set; }
        public TextBox QuantityBox { get; set; }
        public TextBox UnitPriceBox { get; set; }
        public TextBox TotalBox { get; set; }
    }
}