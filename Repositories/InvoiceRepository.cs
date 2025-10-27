// ====================================
// Invoice & Payment Repository
// ====================================

using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ClinicManagementSystem.Repositories
{
    public class InvoiceRepository
    {
        private readonly DatabaseHelper _db;

        public InvoiceRepository()
        {
            _db = DatabaseHelper.Instance;
        }

        // ====================================
        // Create Invoice
        // ====================================
        public int CreateInvoice(Invoice invoice, List<InvoiceItem> items)
        {
            int invoiceId = 0;

            _db.ExecuteTransaction((connection, transaction) =>
            {
                // إنشاء رقم الفاتورة
                invoice.InvoiceNumber = GenerateInvoiceNumber();

                // إدخال الفاتورة
                string invoiceQuery = @"
                    INSERT INTO Invoices (
                        InvoiceNumber, PatientID, InvoiceDate, TotalAmount,
                        DiscountAmount, DiscountPercentage, NetAmount,
                        PaidAmount, RemainingAmount, InvoiceType, PaymentStatus, Notes
                    ) VALUES (
                        @InvoiceNumber, @PatientID, @InvoiceDate, @TotalAmount,
                        @DiscountAmount, @DiscountPercentage, @NetAmount,
                        @PaidAmount, @RemainingAmount, @InvoiceType, @PaymentStatus, @Notes
                    );
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(invoiceQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@PatientID", invoice.PatientID);
                    cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
                    cmd.Parameters.AddWithValue("@DiscountAmount", invoice.DiscountAmount);
                    cmd.Parameters.AddWithValue("@DiscountPercentage", invoice.DiscountPercentage);
                    cmd.Parameters.AddWithValue("@NetAmount", invoice.NetAmount);
                    cmd.Parameters.AddWithValue("@PaidAmount", invoice.PaidAmount);
                    cmd.Parameters.AddWithValue("@RemainingAmount", invoice.RemainingAmount);
                    cmd.Parameters.AddWithValue("@InvoiceType", invoice.InvoiceType);
                    cmd.Parameters.AddWithValue("@PaymentStatus", invoice.PaymentStatus);
                    cmd.Parameters.AddWithValue("@Notes", (object)invoice.Notes ?? DBNull.Value);

                    invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // إدخال بنود الفاتورة
                string itemQuery = @"
                    INSERT INTO InvoiceItems (InvoiceID, Description, Quantity, UnitPrice, TotalPrice)
                    VALUES (@InvoiceID, @Description, @Quantity, @UnitPrice, @TotalPrice)";

                foreach (var item in items)
                {
                    using (var cmd = new SQLiteCommand(itemQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        cmd.Parameters.AddWithValue("@Description", item.Description);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                        cmd.ExecuteNonQuery();
                    }
                }
            });

            return invoiceId;
        }

        // ====================================
        // Read Invoice
        // ====================================
        public Invoice GetInvoiceById(int invoiceId)
        {
            string query = "SELECT * FROM Invoices WHERE InvoiceID = @InvoiceID";
            var parameter = new SQLiteParameter("@InvoiceID", invoiceId);

            var table = _db.ExecuteQuery(query, parameter);

            if (table.Rows.Count > 0)
            {
                var invoice = MapToInvoice(table.Rows[0]);
                invoice.Items = GetInvoiceItems(invoiceId);
                invoice.Payments = GetInvoicePayments(invoiceId);
                return invoice;
            }

            return null;
        }

        public List<Invoice> GetPatientInvoices(int patientId)
        {
            string query = "SELECT * FROM Invoices WHERE PatientID = @PatientID ORDER BY InvoiceDate DESC";
            var parameter = new SQLiteParameter("@PatientID", patientId);

            var table = _db.ExecuteQuery(query, parameter);
            var invoices = new List<Invoice>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                var invoice = MapToInvoice(row);
                invoice.Items = GetInvoiceItems(invoice.InvoiceID);
                invoices.Add(invoice);
            }

            return invoices;
        }

        public List<Invoice> GetUnpaidInvoices()
        {
            string query = @"
                SELECT * FROM Invoices 
                WHERE PaymentStatus IN ('غير مدفوع', 'مدفوع جزئياً') 
                ORDER BY InvoiceDate DESC";

            var table = _db.ExecuteQuery(query);
            var invoices = new List<Invoice>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                invoices.Add(MapToInvoice(row));
            }

            return invoices;
        }

        // ====================================
        // Update Invoice
        // ====================================
        public bool UpdateInvoice(Invoice invoice)
        {
            string query = @"
                UPDATE Invoices SET
                    TotalAmount = @TotalAmount,
                    DiscountAmount = @DiscountAmount,
                    DiscountPercentage = @DiscountPercentage,
                    NetAmount = @NetAmount,
                    PaidAmount = @PaidAmount,
                    RemainingAmount = @RemainingAmount,
                    PaymentStatus = @PaymentStatus,
                    Notes = @Notes
                WHERE InvoiceID = @InvoiceID";

            var parameters = new[]
            {
                new SQLiteParameter("@TotalAmount", invoice.TotalAmount),
                new SQLiteParameter("@DiscountAmount", invoice.DiscountAmount),
                new SQLiteParameter("@DiscountPercentage", invoice.DiscountPercentage),
                new SQLiteParameter("@NetAmount", invoice.NetAmount),
                new SQLiteParameter("@PaidAmount", invoice.PaidAmount),
                new SQLiteParameter("@RemainingAmount", invoice.RemainingAmount),
                new SQLiteParameter("@PaymentStatus", invoice.PaymentStatus),
                new SQLiteParameter("@Notes", (object)invoice.Notes ?? DBNull.Value),
                new SQLiteParameter("@InvoiceID", invoice.InvoiceID)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // ====================================
        // Payments
        // ====================================
        public int AddPayment(Payment payment)
        {
            int paymentId = 0;

            _db.ExecuteTransaction((connection, transaction) =>
            {
                // إضافة الدفعة
                string paymentQuery = @"
                    INSERT INTO Payments (
                        InvoiceID, PaymentDate, Amount, PaymentMethod,
                        Reference, Notes, ReceivedBy
                    ) VALUES (
                        @InvoiceID, @PaymentDate, @Amount, @PaymentMethod,
                        @Reference, @Notes, @ReceivedBy
                    );
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(paymentQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@InvoiceID", payment.InvoiceID);
                    cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                    cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                    cmd.Parameters.AddWithValue("@Reference", (object)payment.Reference ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object)payment.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReceivedBy", (object)payment.ReceivedBy ?? DBNull.Value);

                    paymentId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // تحديث الفاتورة
                string updateInvoiceQuery = @"
                    UPDATE Invoices SET
                        PaidAmount = PaidAmount + @Amount,
                        RemainingAmount = NetAmount - (PaidAmount + @Amount),
                        PaymentStatus = CASE
                            WHEN (PaidAmount + @Amount) >= NetAmount THEN 'مدفوع'
                            WHEN (PaidAmount + @Amount) > 0 THEN 'مدفوع جزئياً'
                            ELSE 'غير مدفوع'
                        END
                    WHERE InvoiceID = @InvoiceID";

                using (var cmd = new SQLiteCommand(updateInvoiceQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                    cmd.Parameters.AddWithValue("@InvoiceID", payment.InvoiceID);
                    cmd.ExecuteNonQuery();
                }
            });

            return paymentId;
        }

        public List<Payment> GetInvoicePayments(int invoiceId)
        {
            string query = "SELECT * FROM Payments WHERE InvoiceID = @InvoiceID ORDER BY PaymentDate DESC";
            var parameter = new SQLiteParameter("@InvoiceID", invoiceId);

            var table = _db.ExecuteQuery(query, parameter);
            var payments = new List<Payment>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                payments.Add(new Payment
                {
                    PaymentID = Convert.ToInt32(row["PaymentID"]),
                    InvoiceID = Convert.ToInt32(row["InvoiceID"]),
                    PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                    Amount = Convert.ToDecimal(row["Amount"]),
                    PaymentMethod = row["PaymentMethod"].ToString(),
                    Reference = row["Reference"]?.ToString(),
                    Notes = row["Notes"]?.ToString(),
                    ReceivedBy = row["ReceivedBy"] != DBNull.Value ? Convert.ToInt32(row["ReceivedBy"]) : (int?)null
                });
            }

            return payments;
        }

        // ====================================
        // Invoice Items
        // ====================================
        public List<InvoiceItem> GetInvoiceItems(int invoiceId)
        {
            string query = "SELECT * FROM InvoiceItems WHERE InvoiceID = @InvoiceID";
            var parameter = new SQLiteParameter("@InvoiceID", invoiceId);

            var table = _db.ExecuteQuery(query, parameter);
            var items = new List<InvoiceItem>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                items.Add(new InvoiceItem
                {
                    ItemID = Convert.ToInt32(row["ItemID"]),
                    InvoiceID = Convert.ToInt32(row["InvoiceID"]),
                    Description = row["Description"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    TotalPrice = Convert.ToDecimal(row["TotalPrice"])
                });
            }

            return items;
        }

        // ====================================
        // Reports & Statistics
        // ====================================
        public decimal GetTotalRevenue(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT COALESCE(SUM(PaidAmount), 0) 
                FROM Invoices 
                WHERE InvoiceDate BETWEEN @FromDate AND @ToDate";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            var result = _db.ExecuteScalar(query, parameters);
            return Convert.ToDecimal(result);
        }

        public decimal GetTotalDebts()
        {
            string query = "SELECT COALESCE(SUM(RemainingAmount), 0) FROM Invoices WHERE PaymentStatus != 'مدفوع'";
            var result = _db.ExecuteScalar(query);
            return Convert.ToDecimal(result);
        }

        public Dictionary<string, decimal> GetRevenueByType(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT InvoiceType, COALESCE(SUM(PaidAmount), 0) as Total
                FROM Invoices 
                WHERE InvoiceDate BETWEEN @FromDate AND @ToDate
                GROUP BY InvoiceType";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var revenue = new Dictionary<string, decimal>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                revenue[row["InvoiceType"].ToString()] = Convert.ToDecimal(row["Total"]);
            }

            return revenue;
        }

        public List<Invoice> GetInvoicesByDateRange(DateTime fromDate, DateTime toDate)
        {
            string query = @"
                SELECT * FROM Invoices 
                WHERE InvoiceDate BETWEEN @FromDate AND @ToDate
                ORDER BY InvoiceDate DESC";

            var parameters = new[]
            {
                new SQLiteParameter("@FromDate", fromDate.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@ToDate", toDate.ToString("yyyy-MM-dd 23:59:59"))
            };

            var table = _db.ExecuteQuery(query, parameters);
            var invoices = new List<Invoice>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                invoices.Add(MapToInvoice(row));
            }

            return invoices;
        }

        // ====================================
        // Helper Methods
        // ====================================
        private Invoice MapToInvoice(System.Data.DataRow row)
        {
            return new Invoice
            {
                InvoiceID = Convert.ToInt32(row["InvoiceID"]),
                InvoiceNumber = row["InvoiceNumber"].ToString(),
                PatientID = Convert.ToInt32(row["PatientID"]),
                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                DiscountPercentage = Convert.ToDecimal(row["DiscountPercentage"]),
                NetAmount = Convert.ToDecimal(row["NetAmount"]),
                PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                RemainingAmount = Convert.ToDecimal(row["RemainingAmount"]),
                InvoiceType = row["InvoiceType"].ToString(),
                PaymentStatus = row["PaymentStatus"].ToString(),
                Notes = row["Notes"]?.ToString()
            };
        }

        private string GenerateInvoiceNumber()
        {
            string query = "SELECT COUNT(*) FROM Invoices WHERE DATE(InvoiceDate) = DATE('now')";
            int count = Convert.ToInt32(_db.ExecuteScalar(query));
            return $"INV-{DateTime.Now:yyyyMMdd}-{(count + 1):D4}";
        }
    }
}