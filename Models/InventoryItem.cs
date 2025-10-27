using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Inventory Item Model
    // ====================================
    public class InventoryItem : BaseModel
    {
        private int _itemID;
        private string _itemCode;
        private string _itemName;
        private string _category;
        private string _description;
        private string _unit;
        private int _quantity;
        private int _minimumStock;
        private decimal _unitPrice;
        private string _supplierName;
        private string _supplierPhone;
        private DateTime? _expiryDate;
        private string _location;
        private bool _isActive;

        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        public string ItemCode
        {
            get => _itemCode;
            set => SetProperty(ref _itemCode, value);
        }

        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public int MinimumStock
        {
            get => _minimumStock;
            set => SetProperty(ref _minimumStock, value);
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public string SupplierName
        {
            get => _supplierName;
            set => SetProperty(ref _supplierName, value);
        }

        public string SupplierPhone
        {
            get => _supplierPhone;
            set => SetProperty(ref _supplierPhone, value);
        }

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsLowStock => Quantity <= MinimumStock;
        public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddMonths(3);
    }
}
