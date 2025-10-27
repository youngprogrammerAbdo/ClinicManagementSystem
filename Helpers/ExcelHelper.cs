// ====================================
// Excel Helper - لتصدير البيانات إلى Excel
// Install: ClosedXML via NuGet
// ====================================

using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Helpers
{
    public class ExcelHelper
    {
        // ====================================
        // Export Patients - تصدير المرضى
        // ====================================
        public bool ExportPatients(List<Patient> patients, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("المرضى");

                    // العناوين
                    worksheet.Cell(1, 1).Value = "كود المريض";
                    worksheet.Cell(1, 2).Value = "الاسم الكامل";
                    worksheet.Cell(1, 3).Value = "العمر";
                    worksheet.Cell(1, 4).Value = "الجنس";
                    worksheet.Cell(1, 5).Value = "رقم الهاتف";
                    worksheet.Cell(1, 6).Value = "العنوان";
                    worksheet.Cell(1, 7).Value = "فصيلة الدم";
                    worksheet.Cell(1, 8).Value = "تاريخ التسجيل";

                    // تنسيق العناوين
                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // البيانات
                    int row = 2;
                    foreach (var patient in patients)
                    {
                        worksheet.Cell(row, 1).Value = patient.PatientCode;
                        worksheet.Cell(row, 2).Value = patient.FullName;
                        worksheet.Cell(row, 3).Value = patient.Age;
                        worksheet.Cell(row, 4).Value = patient.Gender;
                        worksheet.Cell(row, 5).Value = patient.PhoneNumber;
                        worksheet.Cell(row, 6).Value = patient.Address;
                        worksheet.Cell(row, 7).Value = patient.BloodType;
                        worksheet.Cell(row, 8).Value = patient.RegistrationDate.ToString("dd/MM/yyyy");
                        row++;
                    }

                    // ضبط عرض الأعمدة
                    worksheet.Columns().AdjustToContents();

                    // إضافة حدود للجدول
                    var dataRange = worksheet.Range(1, 1, row - 1, 8);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // حفظ الملف
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Daily Report - التقرير اليومي
        // ====================================
        public bool ExportDailyReport(DateTime date, List<Visit> visits, decimal revenue, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("التقرير اليومي");

                    // العنوان الرئيسي
                    worksheet.Cell(1, 1).Value = $"التقرير اليومي - {date:dd/MM/yyyy}";
                    worksheet.Range(1, 1, 1, 6).Merge();
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // الإحصائيات
                    worksheet.Cell(3, 1).Value = "إجمالي الزيارات:";
                    worksheet.Cell(3, 2).Value = visits.Count;
                    worksheet.Cell(3, 1).Style.Font.Bold = true;

                    worksheet.Cell(4, 1).Value = "الزيارات المكتملة:";
                    worksheet.Cell(4, 2).Value = visits.Count(v => v.VisitStatus == "مكتمل");
                    worksheet.Cell(4, 1).Style.Font.Bold = true;

                    worksheet.Cell(5, 1).Value = "إجمالي الإيرادات:";
                    worksheet.Cell(5, 2).Value = revenue;
                    worksheet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00 \"جنيه\"";
                    worksheet.Cell(5, 1).Style.Font.Bold = true;

                    // تفاصيل الزيارات
                    worksheet.Cell(7, 1).Value = "الدور";
                    worksheet.Cell(7, 2).Value = "المريض";
                    worksheet.Cell(7, 3).Value = "نوع الكشف";
                    worksheet.Cell(7, 4).Value = "التشخيص";
                    worksheet.Cell(7, 5).Value = "الحالة";
                    worksheet.Cell(7, 6).Value = "الرسوم";

                    var headerRange = worksheet.Range(7, 1, 7, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    int row = 8;
                    foreach (var visit in visits)
                    {
                        worksheet.Cell(row, 1).Value = visit.QueueNumber ?? 0;
                        worksheet.Cell(row, 2).Value = visit.Patient?.FullName ?? "";
                        worksheet.Cell(row, 3).Value = visit.VisitType;
                        worksheet.Cell(row, 4).Value = visit.Diagnosis ?? "";
                        worksheet.Cell(row, 5).Value = visit.VisitStatus;
                        worksheet.Cell(row, 6).Value = visit.ExaminationFee;
                        row++;
                    }

                    worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";

                    // إضافة حدود
                    if (visits.Count > 0)
                    {
                        var dataRange = worksheet.Range(7, 1, row - 1, 6);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Monthly Report - التقرير الشهري
        // ====================================
        public bool ExportMonthlyReport(int year, int month, Dictionary<string, object> stats, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("التقرير الشهري");

                    // العنوان
                    worksheet.Cell(1, 1).Value = $"التقرير الشهري - {month}/{year}";
                    worksheet.Range(1, 1, 1, 4).Merge();
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

                    // الإحصائيات
                    int row = 3;
                    worksheet.Cell(row, 1).Value = "البيان";
                    worksheet.Cell(row, 2).Value = "القيمة";
                    worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
                    worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Range(row, 1, row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    row++;
                    foreach (var stat in stats)
                    {
                        worksheet.Cell(row, 1).Value = stat.Key;
                        worksheet.Cell(row, 2).Value = stat.Value.ToString();
                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                        row++;
                    }

                    // إضافة حدود
                    var dataRange = worksheet.Range(3, 1, row - 1, 2);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Invoices - تصدير الفواتير
        // ====================================
        public bool ExportInvoices(List<Invoice> invoices, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("الفواتير");

                    // العناوين
                    worksheet.Cell(1, 1).Value = "رقم الفاتورة";
                    worksheet.Cell(1, 2).Value = "التاريخ";
                    worksheet.Cell(1, 3).Value = "المريض";
                    worksheet.Cell(1, 4).Value = "النوع";
                    worksheet.Cell(1, 5).Value = "الإجمالي";
                    worksheet.Cell(1, 6).Value = "الخصم";
                    worksheet.Cell(1, 7).Value = "الصافي";
                    worksheet.Cell(1, 8).Value = "المدفوع";
                    worksheet.Cell(1, 9).Value = "المتبقي";
                    worksheet.Cell(1, 10).Value = "الحالة";

                    // تنسيق العناوين
                    var headerRange = worksheet.Range(1, 1, 1, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // البيانات
                    int row = 2;
                    foreach (var invoice in invoices)
                    {
                        worksheet.Cell(row, 1).Value = invoice.InvoiceNumber;
                        worksheet.Cell(row, 2).Value = invoice.InvoiceDate.ToString("dd/MM/yyyy");
                        worksheet.Cell(row, 3).Value = invoice.Patient?.FullName ?? "";
                        worksheet.Cell(row, 4).Value = invoice.InvoiceType;
                        worksheet.Cell(row, 5).Value = invoice.TotalAmount;
                        worksheet.Cell(row, 6).Value = invoice.DiscountAmount;
                        worksheet.Cell(row, 7).Value = invoice.NetAmount;
                        worksheet.Cell(row, 8).Value = invoice.PaidAmount;
                        worksheet.Cell(row, 9).Value = invoice.RemainingAmount;
                        worksheet.Cell(row, 10).Value = invoice.PaymentStatus;

                        // تلوين الصفوف حسب حالة الدفع
                        if (invoice.PaymentStatus == "مدفوع بالكامل")
                        {
                            worksheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else if (invoice.PaymentStatus == "غير مدفوع")
                        {
                            worksheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.LightPink;
                        }
                        else if (invoice.PaymentStatus == "دفع جزئي")
                        {
                            worksheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.LightYellow;
                        }

                        row++;
                    }

                    // تنسيق الأعمدة المالية
                    worksheet.Column(5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Column(9).Style.NumberFormat.Format = "#,##0.00";

                    // إضافة حدود للجدول
                    if (invoices.Count > 0)
                    {
                        var dataRange = worksheet.Range(1, 1, row - 1, 10);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    // ضبط عرض الأعمدة
                    worksheet.Columns().AdjustToContents();

                    // حفظ الملف
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Inventory - تصدير المخزون
        // ====================================
        public bool ExportInventory(List<InventoryItem> items, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("المخزون");

                    // العناوين
                    worksheet.Cell(1, 1).Value = "الكود";
                    worksheet.Cell(1, 2).Value = "اسم الصنف";
                    worksheet.Cell(1, 3).Value = "الفئة";
                    worksheet.Cell(1, 4).Value = "الكمية";
                    worksheet.Cell(1, 5).Value = "الحد الأدنى";
                    worksheet.Cell(1, 6).Value = "السعر";
                    worksheet.Cell(1, 7).Value = "الإجمالي";
                    worksheet.Cell(1, 8).Value = "المورد";
                    worksheet.Cell(1, 9).Value = "تاريخ الصلاحية";
                    worksheet.Cell(1, 10).Value = "الحالة";

                    // تنسيق العناوين
                    var headerRange = worksheet.Range(1, 1, 1, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // البيانات
                    int row = 2;
                    foreach (var item in items)
                    {
                        worksheet.Cell(row, 1).Value = item.ItemCode;
                        worksheet.Cell(row, 2).Value = item.ItemName;
                        worksheet.Cell(row, 3).Value = item.Category;
                        worksheet.Cell(row, 4).Value = item.Quantity;
                        worksheet.Cell(row, 5).Value = item.MinimumStock;
                        worksheet.Cell(row, 6).Value = item.UnitPrice;
                        worksheet.Cell(row, 7).Value = item.Quantity * item.UnitPrice;
                        worksheet.Cell(row, 8).Value = item.SupplierName;
                        worksheet.Cell(row, 9).Value = item.ExpiryDate?.ToString("dd/MM/yyyy") ?? "";
                        worksheet.Cell(row, 10).Value = item.IsLowStock ? "منخفض" : "جيد";

                        // تلوين الصفوف حسب الحالة
                        if (item.IsLowStock)
                        {
                            worksheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.LightPink;
                        }
                        else if (item.ExpiryDate.HasValue && item.ExpiryDate.Value <= DateTime.Now.AddMonths(3))
                        {
                            worksheet.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.LightOrange;
                        }

                        row++;
                    }

                    // تنسيق الأعمدة المالية
                    worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00";

                    // إضافة حدود للجدول
                    if (items.Count > 0)
                    {
                        var dataRange = worksheet.Range(1, 1, row - 1, 10);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    // إضافة ملخص
                    row += 2;
                    worksheet.Cell(row, 1).Value = "إجمالي عدد الأصناف:";
                    worksheet.Cell(row, 2).Value = items.Count;
                    worksheet.Cell(row, 1).Style.Font.Bold = true;

                    row++;
                    worksheet.Cell(row, 1).Value = "الأصناف المنخفضة:";
                    worksheet.Cell(row, 2).Value = items.Count(i => i.IsLowStock);
                    worksheet.Cell(row, 1).Style.Font.Bold = true;

                    row++;
                    worksheet.Cell(row, 1).Value = "إجمالي قيمة المخزون:";
                    worksheet.Cell(row, 2).Value = items.Sum(i => i.Quantity * i.UnitPrice);
                    worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00 \"جنيه\"";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;

                    // ضبط عرض الأعمدة
                    worksheet.Columns().AdjustToContents();

                    // حفظ الملف
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Visits - تصدير الزيارات
        // ====================================
        public bool ExportVisits(List<Visit> visits, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("الزيارات");

                    // العناوين
                    worksheet.Cell(1, 1).Value = "رقم الزيارة";
                    worksheet.Cell(1, 2).Value = "التاريخ";
                    worksheet.Cell(1, 3).Value = "المريض";
                    worksheet.Cell(1, 4).Value = "نوع الكشف";
                    worksheet.Cell(1, 5).Value = "التشخيص";
                    worksheet.Cell(1, 6).Value = "الحالة";
                    worksheet.Cell(1, 7).Value = "الرسوم";
                    worksheet.Cell(1, 8).Value = "الدور";

                    // تنسيق العناوين
                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightCyan;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // البيانات
                    int row = 2;
                    foreach (var visit in visits)
                    {
                        worksheet.Cell(row, 1).Value = visit.VisitId;
                        worksheet.Cell(row, 2).Value = visit.VisitDate.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell(row, 3).Value = visit.Patient?.FullName ?? "";
                        worksheet.Cell(row, 4).Value = visit.VisitType;
                        worksheet.Cell(row, 5).Value = visit.Diagnosis ?? "";
                        worksheet.Cell(row, 6).Value = visit.VisitStatus;
                        worksheet.Cell(row, 7).Value = visit.ExaminationFee;
                        worksheet.Cell(row, 8).Value = visit.QueueNumber ?? 0;
                        row++;
                    }

                    // تنسيق عمود الرسوم
                    worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00";

                    // إضافة حدود للجدول
                    if (visits.Count > 0)
                    {
                        var dataRange = worksheet.Range(1, 1, row - 1, 8);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    // ضبط عرض الأعمدة
                    worksheet.Columns().AdjustToContents();

                    // حفظ الملف
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Export Prescriptions - تصدير الروشتات
        // ====================================
        public bool ExportPrescriptions(List<Prescription> prescriptions, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("الروشتات");

                    // العناوين
                    worksheet.Cell(1, 1).Value = "رقم الروشتة";
                    worksheet.Cell(1, 2).Value = "التاريخ";
                    worksheet.Cell(1, 3).Value = "المريض";
                    worksheet.Cell(1, 4).Value = "عدد الأدوية";
                    worksheet.Cell(1, 5).Value = "الطبيب";
                    worksheet.Cell(1, 6).Value = "ملاحظات";

                    // تنسيق العناوين
                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // البيانات
                    int row = 2;
                    foreach (var prescription in prescriptions)
                    {
                        worksheet.Cell(row, 1).Value = prescription.PrescriptionId;
                        worksheet.Cell(row, 2).Value = prescription.PrescriptionDate.ToString("dd/MM/yyyy");
                        worksheet.Cell(row, 3).Value = prescription.Patient?.FullName ?? "";
                        worksheet.Cell(row, 4).Value = prescription.Details?.Count ?? 0;
                        worksheet.Cell(row, 5).Value = prescription.DoctorName ?? "";
                        worksheet.Cell(row, 6).Value = prescription.Notes ?? "";
                        row++;
                    }

                    // إضافة حدود للجدول
                    if (prescriptions.Count > 0)
                    {
                        var dataRange = worksheet.Range(1, 1, row - 1, 6);
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    // ضبط عرض الأعمدة
                    worksheet.Columns().AdjustToContents();

                    // حفظ الملف
                    workbook.SaveAs(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }
    }
}