using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Invoice Model
    // ====================================
    public class Invoice : BaseModel
    {
        private int _invoiceID;
        private string _invoiceNumber;
        private int _patientID;
        private DateTime _invoiceDate;
        private decimal _totalAmount;
        private decimal _discountAmount;
        private decimal _discountPercentage;
        private decimal _netAmount;
        private decimal _paidAmount;
        private decimal _remainingAmount;
        private string _invoiceType;
        private string _paymentStatus;
        private string _notes;

        public int InvoiceID
        {
            get => _invoiceID;
            set => SetProperty(ref _invoiceID, value);
        }

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set => SetProperty(ref _invoiceNumber, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set => SetProperty(ref _invoiceDate, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                SetProperty(ref _discountAmount, value);
                CalculateNetAmount();
            }
        }

        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set
            {
                SetProperty(ref _discountPercentage, value);
                DiscountAmount = TotalAmount * (value / 100);
            }
        }

        public decimal NetAmount
        {
            get => _netAmount;
            set => SetProperty(ref _netAmount, value);
        }

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                SetProperty(ref _paidAmount, value);
                RemainingAmount = NetAmount - value;
                UpdatePaymentStatus();
            }
        }

        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set => SetProperty(ref _remainingAmount, value);
        }

        public string InvoiceType
        {
            get => _invoiceType;
            set => SetProperty(ref _invoiceType, value);
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public Patient Patient { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public List<Payment> Payments { get; set; }

        private void CalculateNetAmount()
        {
            NetAmount = TotalAmount - DiscountAmount;
            RemainingAmount = NetAmount - PaidAmount;
        }

        private void UpdatePaymentStatus()
        {
            if (PaidAmount >= NetAmount)
                PaymentStatus = "مدفوع";
            else if (PaidAmount > 0)
                PaymentStatus = "مدفوع جزئياً";
            else
                PaymentStatus = "غير مدفوع";
        }
    }
}
