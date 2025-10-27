using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Payment Model
    // ====================================
    public class Payment : BaseModel
    {
        private int _paymentID;
        private int _invoiceID;
        private DateTime _paymentDate;
        private decimal _amount;
        private string _paymentMethod;
        private string _reference;
        private string _notes;
        private int? _receivedBy;

        public int PaymentID
        {
            get => _paymentID;
            set => SetProperty(ref _paymentID, value);
        }

        public int InvoiceID
        {
            get => _invoiceID;
            set => SetProperty(ref _invoiceID, value);
        }

        public DateTime PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public string Reference
        {
            get => _reference;
            set => SetProperty(ref _reference, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public int? ReceivedBy
        {
            get => _receivedBy;
            set => SetProperty(ref _receivedBy, value);
        }

        public User ReceivedByUser { get; set; }
    }

}
