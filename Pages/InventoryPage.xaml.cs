// ====================================
// InventoryPage.xaml.cs
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.Models;
using ClinicManagementSystem.Helpers;

namespace ClinicManagementSystem.Pages
{
    public partial class InventoryPage : Page
    {
        private readonly InventoryRepository _inventoryRepo;
        private List<InventoryItem> _allItems;
        private List<InventoryItem> _filteredItems;

        public InventoryPage()
        {
            InitializeComponent();
            _inventoryRepo = new InventoryRepository();
            LoadInventory();
        }

        private void LoadInventory()
        {
            try
            {
                _allItems = _inventoryRepo.GetAllItems();
                _filteredItems = new List<InventoryItem>(_allItems);

                dgInventory.ItemsSource = _filteredItems;
                UpdateStatistics();
                UpdateTotalCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            txtTotalItems.Text = _allItems.Count.ToString();
            txtLowStock.Text = _inventoryRepo.GetLowStockItems().Count.ToString();
            txtExpiringSoon.Text = _inventoryRepo.GetExpiringSoonItems(30).Count.ToString();
            txtTotalValue.Text = $"{_inventoryRepo.GetTotalInventoryValue():N0} جنيه";
        }

        private void UpdateTotalCount()
        {
            txtTotalCount.Text = _filteredItems.Count.ToString();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allItems == null || !_allItems.Any())
                return;

            if (dgInventory.SelectedItem == null)
            {
                dgInventory.ItemsSource = _allItems;
                return;
            }

            var searchTerm = txtSearch.Text.ToLower();
            var selectedCategory = (cmbCategoryFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            
            _filteredItems = _allItems.Where(i =>
            {
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) ||
                    i.ItemName.ToLower().Contains(searchTerm) ||
                    i.ItemCode.ToLower().Contains(searchTerm);

                bool matchesCategory = selectedCategory == "جميع الفئات" || i.Category == selectedCategory;

                return matchesSearch && matchesCategory;
            }).ToList();

            dgInventory.ItemsSource = _filteredItems;
            UpdateTotalCount();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            // يمكن إضافة Dialog هنا
            MessageBox.Show("إضافة صنف - قيد التطوير", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddStock_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int itemId = Convert.ToInt32(button.Tag);
                // إضافة كمية
                var input = Microsoft.VisualBasic.Interaction.InputBox("أدخل الكمية المضافة:", "إضافة كمية", "10");
                if (int.TryParse(input, out int quantity))
                {
                    _inventoryRepo.AddTransaction(itemId, "إضافة", quantity, null, "إضافة يدوية", MainWindow.CurrentUser?.UserID);
                    LoadInventory();
                }
            }
        }

        private void RemoveStock_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int itemId = Convert.ToInt32(button.Tag);
                MessageBox.Show("استهلاك - قيد التطوير", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("تعديل - قيد التطوير", "معلومة", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                var result = MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    int itemId = Convert.ToInt32(button.Tag);
                    _inventoryRepo.DeleteItem(itemId);
                    LoadInventory();
                }
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Inventory_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var excelHelper = new ExcelHelper();
                    if (excelHelper.ExportInventory(_filteredItems, saveDialog.FileName))
                    {
                        MessageBox.Show("تم التصدير بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}