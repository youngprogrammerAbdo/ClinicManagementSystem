using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Invoice Item Model
    // ====================================
    public class InvoiceItem : BaseModel
    {
        private int _itemID;
        private int _invoiceID;
        private string _description;
        private int _quantity;
        private decimal _unitPrice;
        private decimal _totalPrice;

        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        public int InvoiceID
        {
            get => _invoiceID;
            set => SetProperty(ref _invoiceID, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                SetProperty(ref _quantity, value);
                TotalPrice = value * UnitPrice;
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                SetProperty(ref _unitPrice, value);
                TotalPrice = Quantity * value;
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }
    }
}
