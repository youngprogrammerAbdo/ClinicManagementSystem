// ====================================
// PDF Helper - لإنشاء التقارير
// Install: iTextSharp via NuGet
// ====================================

using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClinicManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace ClinicManagementSystem.Helpers
{
    public class PDFHelper
    {
        private readonly string _fontPath;
        private BaseFont _arabicFont;

        public PDFHelper()
        {
            // تحميل خط عربي - يجب أن يكون موجود في المشروع
            _fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Arial.ttf");

            try
            {
                _arabicFont = BaseFont.CreateFont(_fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                // استخدام خط افتراضي إذا لم يتم العثور على الخط
                _arabicFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }
        }

        // ====================================
        // Patient Report - تقرير بيانات المريض
        // ====================================
        public bool GeneratePatientReport(Patient patient, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                // Fonts
                Font titleFont = new Font(_arabicFont, 20, Font.BOLD, BaseColor.BLUE);
                Font headerFont = new Font(_arabicFont, 14, Font.BOLD);
                Font normalFont = new Font(_arabicFont, 12);

                // العنوان الرئيسي
                Paragraph title = new Paragraph("تقرير بيانات المريض", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // معلومات العيادة
                PdfPTable clinicTable = new PdfPTable(1);
                clinicTable.WidthPercentage = 100;
                clinicTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                PdfPCell clinicCell = new PdfPCell(new Phrase("عيادة الشفاء - نظام إدارة متكامل", headerFont));
                clinicCell.BackgroundColor = new BaseColor(33, 150, 243);
                clinicCell.Padding = 10;
                clinicCell.HorizontalAlignment = Element.ALIGN_CENTER;
                clinicCell.Border = Rectangle.NO_BORDER;
                clinicTable.AddCell(clinicCell);

                document.Add(clinicTable);
                document.Add(new Paragraph(" "));

                // البيانات الأساسية
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                infoTable.SetWidths(new float[] { 1, 2 });
                infoTable.SpacingBefore = 10;

                AddTableRow(infoTable, "كود المريض:", patient.PatientCode, headerFont, normalFont);
                AddTableRow(infoTable, "الاسم الكامل:", patient.FullName, headerFont, normalFont);
                AddTableRow(infoTable, "تاريخ الميلاد:", patient.DateOfBirth.ToString("dd/MM/yyyy"), headerFont, normalFont);
                AddTableRow(infoTable, "العمر:", patient.Age.ToString() + " سنة", headerFont, normalFont);
                AddTableRow(infoTable, "الجنس:", patient.Gender, headerFont, normalFont);
                AddTableRow(infoTable, "رقم الهاتف:", patient.PhoneNumber, headerFont, normalFont);

                if (!string.IsNullOrEmpty(patient.PhoneNumber2))
                    AddTableRow(infoTable, "هاتف بديل:", patient.PhoneNumber2, headerFont, normalFont);

                if (!string.IsNullOrEmpty(patient.Email))
                    AddTableRow(infoTable, "البريد الإلكتروني:", patient.Email, headerFont, normalFont);

                if (!string.IsNullOrEmpty(patient.Address))
                    AddTableRow(infoTable, "العنوان:", patient.Address, headerFont, normalFont);

                if (!string.IsNullOrEmpty(patient.BloodType))
                    AddTableRow(infoTable, "فصيلة الدم:", patient.BloodType, headerFont, normalFont);

                document.Add(infoTable);

                // التاريخ المرضي
                if (patient.MedicalHistory != null)
                {
                    document.Add(new Paragraph(" "));
                    Paragraph historyTitle = new Paragraph("التاريخ المرضي", headerFont);
                    historyTitle.SpacingBefore = 15;
                    historyTitle.SpacingAfter = 10;
                    document.Add(historyTitle);

                    PdfPTable historyTable = new PdfPTable(2);
                    historyTable.WidthPercentage = 100;
                    historyTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    historyTable.SetWidths(new float[] { 1, 2 });

                    if (!string.IsNullOrEmpty(patient.MedicalHistory.ChronicDiseases))
                        AddTableRow(historyTable, "الأمراض المزمنة:", patient.MedicalHistory.ChronicDiseases, headerFont, normalFont);

                    if (!string.IsNullOrEmpty(patient.MedicalHistory.Allergies))
                        AddTableRow(historyTable, "الحساسية:", patient.MedicalHistory.Allergies, headerFont, normalFont);

                    if (!string.IsNullOrEmpty(patient.MedicalHistory.CurrentMedications))
                        AddTableRow(historyTable, "الأدوية الحالية:", patient.MedicalHistory.CurrentMedications, headerFont, normalFont);

                    if (!string.IsNullOrEmpty(patient.MedicalHistory.PreviousSurgeries))
                        AddTableRow(historyTable, "العمليات السابقة:", patient.MedicalHistory.PreviousSurgeries, headerFont, normalFont);

                    document.Add(historyTable);
                }

                // Footer
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph($"تاريخ التقرير: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 30;
                document.Add(footer);

                document.Close();
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Invoice Report - فاتورة
        // ====================================
        public bool GenerateInvoiceReport(Invoice invoice, Patient patient, List<InvoiceItem> items, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                Font titleFont = new Font(_arabicFont, 22, Font.BOLD, BaseColor.BLUE);
                Font headerFont = new Font(_arabicFont, 14, Font.BOLD);
                Font normalFont = new Font(_arabicFont, 12);
                Font boldFont = new Font(_arabicFont, 12, Font.BOLD);

                // Header
                Paragraph title = new Paragraph("فاتورة", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // معلومات العيادة والفاتورة
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                headerTable.SetWidths(new float[] { 1, 1 });

                // معلومات العيادة
                PdfPCell clinicCell = new PdfPCell();
                clinicCell.Border = Rectangle.NO_BORDER;
                clinicCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                clinicCell.AddElement(new Phrase("عيادة الشفاء", headerFont));
                clinicCell.AddElement(new Phrase("القاهرة، مصر", normalFont));
                clinicCell.AddElement(new Phrase("تليفون: 0123456789", normalFont));
                headerTable.AddCell(clinicCell);

                // معلومات الفاتورة
                PdfPCell invoiceCell = new PdfPCell();
                invoiceCell.Border = Rectangle.NO_BORDER;
                invoiceCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                invoiceCell.HorizontalAlignment = Element.ALIGN_LEFT;
                invoiceCell.AddElement(new Phrase($"رقم الفاتورة: {invoice.InvoiceNumber}", boldFont));
                invoiceCell.AddElement(new Phrase($"التاريخ: {invoice.InvoiceDate:dd/MM/yyyy}", normalFont));
                invoiceCell.AddElement(new Phrase($"نوع الفاتورة: {invoice.InvoiceType}", normalFont));
                headerTable.AddCell(invoiceCell);

                document.Add(headerTable);
                document.Add(new Paragraph(" "));

                // بيانات المريض
                Paragraph patientTitle = new Paragraph("بيانات المريض", headerFont);
                patientTitle.SpacingBefore = 10;
                patientTitle.SpacingAfter = 10;
                document.Add(patientTitle);

                PdfPTable patientTable = new PdfPTable(2);
                patientTable.WidthPercentage = 100;
                patientTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                patientTable.SetWidths(new float[] { 1, 2 });

                AddTableRow(patientTable, "كود المريض:", patient.PatientCode, boldFont, normalFont);
                AddTableRow(patientTable, "الاسم:", patient.FullName, boldFont, normalFont);
                AddTableRow(patientTable, "الهاتف:", patient.PhoneNumber, boldFont, normalFont);

                document.Add(patientTable);
                document.Add(new Paragraph(" "));

                // بنود الفاتورة
                Paragraph itemsTitle = new Paragraph("بنود الفاتورة", headerFont);
                itemsTitle.SpacingBefore = 10;
                itemsTitle.SpacingAfter = 10;
                document.Add(itemsTitle);

                PdfPTable itemsTable = new PdfPTable(4);
                itemsTable.WidthPercentage = 100;
                itemsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                itemsTable.SetWidths(new float[] { 3, 1, 1, 1 });

                // Headers
                AddHeaderCell(itemsTable, "الوصف", headerFont);
                AddHeaderCell(itemsTable, "الكمية", headerFont);
                AddHeaderCell(itemsTable, "السعر", headerFont);
                AddHeaderCell(itemsTable, "الإجمالي", headerFont);

                // Items
                foreach (var item in items)
                {
                    AddCell(itemsTable, item.Description, normalFont);
                    AddCell(itemsTable, item.Quantity.ToString(), normalFont);
                    AddCell(itemsTable, item.UnitPrice.ToString("N2"), normalFont);
                    AddCell(itemsTable, item.TotalPrice.ToString("N2"), normalFont);
                }

                document.Add(itemsTable);
                document.Add(new Paragraph(" "));

                // المجاميع
                PdfPTable totalsTable = new PdfPTable(2);
                totalsTable.WidthPercentage = 60;
                totalsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                totalsTable.HorizontalAlignment = Element.ALIGN_LEFT;
                totalsTable.SetWidths(new float[] { 1, 1 });

                AddTableRow(totalsTable, "الإجمالي:", invoice.TotalAmount.ToString("N2") + " جنيه", boldFont, normalFont);

                if (invoice.DiscountAmount > 0)
                {
                    AddTableRow(totalsTable, "الخصم:", invoice.DiscountAmount.ToString("N2") + " جنيه", boldFont, normalFont);
                }

                AddTableRow(totalsTable, "الصافي:", invoice.NetAmount.ToString("N2") + " جنيه", headerFont, boldFont);
                AddTableRow(totalsTable, "المدفوع:", invoice.PaidAmount.ToString("N2") + " جنيه", boldFont, normalFont);
                AddTableRow(totalsTable, "المتبقي:", invoice.RemainingAmount.ToString("N2") + " جنيه", boldFont, normalFont);

                document.Add(totalsTable);

                // حالة الدفع
                document.Add(new Paragraph(" "));
                Paragraph statusPara = new Paragraph($"حالة الدفع: {invoice.PaymentStatus}", headerFont);
                statusPara.Alignment = Element.ALIGN_CENTER;
                document.Add(statusPara);

                // Footer
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph("شكراً لزيارتكم - نتمنى لكم الشفاء العاجل", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 30;
                document.Add(footer);

                document.Close();
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Prescription Report - روشتة طبية
        // ====================================
        public bool GeneratePrescriptionReport(Prescription prescription, Patient patient, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                Font titleFont = new Font(_arabicFont, 22, Font.BOLD, new BaseColor(0, 128, 0));
                Font headerFont = new Font(_arabicFont, 14, Font.BOLD);
                Font normalFont = new Font(_arabicFont, 12);

                // Header
                Paragraph title = new Paragraph("روشتة طبية", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // معلومات العيادة
                Paragraph clinicInfo = new Paragraph("عيادة الشفاء - د. أحمد محمد", headerFont);
                clinicInfo.Alignment = Element.ALIGN_CENTER;
                clinicInfo.SpacingAfter = 15;
                document.Add(clinicInfo);

                // معلومات المريض
                PdfPTable patientTable = new PdfPTable(2);
                patientTable.WidthPercentage = 100;
                patientTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                patientTable.SpacingBefore = 10;

                AddTableRow(patientTable, "اسم المريض:", patient.FullName, headerFont, normalFont);
                AddTableRow(patientTable, "العمر:", patient.Age + " سنة", headerFont, normalFont);
                AddTableRow(patientTable, "التاريخ:", prescription.PrescriptionDate.ToString("dd/MM/yyyy"), headerFont, normalFont);

                document.Add(patientTable);
                document.Add(new Paragraph(" "));

                // الأدوية
                Paragraph medsTitle = new Paragraph("الأدوية الموصوفة", headerFont);
                medsTitle.SpacingBefore = 15;
                medsTitle.SpacingAfter = 10;
                document.Add(medsTitle);

                PdfPTable medsTable = new PdfPTable(4);
                medsTable.WidthPercentage = 100;
                medsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                medsTable.SetWidths(new float[] { 2, 1.5f, 1.5f, 1 });

                AddHeaderCell(medsTable, "الدواء", headerFont);
                AddHeaderCell(medsTable, "الجرعة", headerFont);
                AddHeaderCell(medsTable, "عدد المرات", headerFont);
                AddHeaderCell(medsTable, "المدة", headerFont);

                foreach (var detail in prescription.Details)
                {
                    AddCell(medsTable, detail.MedicineName, normalFont);
                    AddCell(medsTable, detail.Dosage, normalFont);
                    AddCell(medsTable, detail.Frequency, normalFont);
                    AddCell(medsTable, detail.Duration, normalFont);
                }

                document.Add(medsTable);

                // التعليمات
                if (!string.IsNullOrEmpty(prescription.Notes))
                {
                    document.Add(new Paragraph(" "));
                    Paragraph notesTitle = new Paragraph("تعليمات:", headerFont);
                    notesTitle.SpacingBefore = 15;
                    notesTitle.SpacingAfter = 5;
                    document.Add(notesTitle);

                    Paragraph notes = new Paragraph(prescription.Notes, normalFont);
                    notes.Alignment = PdfWriter.RUN_DIRECTION_RTL;
                    document.Add(notes);
                }

                // Footer
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph("تمنياتنا بالشفاء العاجل", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 40;
                document.Add(footer);

                document.Close();
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Daily Report - التقرير اليومي
        // ====================================
        public bool GenerateDailyReport(DateTime date, List<Visit> visits, decimal totalRevenue, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                Font titleFont = new Font(_arabicFont, 20, Font.BOLD, BaseColor.BLUE);
                Font headerFont = new Font(_arabicFont, 14, Font.BOLD);
                Font normalFont = new Font(_arabicFont, 12);

                // العنوان
                Paragraph title = new Paragraph("التقرير اليومي", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                Paragraph datePara = new Paragraph($"التاريخ: {date:dd/MM/yyyy}", headerFont);
                datePara.Alignment = Element.ALIGN_CENTER;
                datePara.SpacingAfter = 20;
                document.Add(datePara);

                // الإحصائيات
                PdfPTable statsTable = new PdfPTable(2);
                statsTable.WidthPercentage = 100;
                statsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                AddTableRow(statsTable, "عدد الزيارات:", visits.Count.ToString(), headerFont, normalFont);
                AddTableRow(statsTable, "الزيارات المكتملة:", visits.Count(v => v.VisitStatus == "مكتمل").ToString(), headerFont, normalFont);
                AddTableRow(statsTable, "الزيارات الملغاة:", visits.Count(v => v.VisitStatus == "ملغي").ToString(), headerFont, normalFont);
                AddTableRow(statsTable, "إجمالي الإيرادات:", totalRevenue.ToString("N2") + " جنيه", headerFont, normalFont);

                document.Add(statsTable);
                document.Add(new Paragraph(" "));

                // قائمة الزيارات
                Paragraph visitsTitle = new Paragraph("تفاصيل الزيارات", headerFont);
                visitsTitle.SpacingBefore = 15;
                visitsTitle.SpacingAfter = 10;
                document.Add(visitsTitle);

                PdfPTable visitsTable = new PdfPTable(5);
                visitsTable.WidthPercentage = 100;
                visitsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                visitsTable.SetWidths(new float[] { 1, 2, 2, 1.5f, 1 });

                AddHeaderCell(visitsTable, "الدور", headerFont);
                AddHeaderCell(visitsTable, "المريض", headerFont);
                AddHeaderCell(visitsTable, "نوع الكشف", headerFont);
                AddHeaderCell(visitsTable, "الحالة", headerFont);
                AddHeaderCell(visitsTable, "الرسوم", headerFont);

                foreach (var visit in visits)
                {
                    AddCell(visitsTable, visit.QueueNumber?.ToString() ?? "-", normalFont);
                    AddCell(visitsTable, visit.Patient?.FullName ?? "غير محدد", normalFont);
                    AddCell(visitsTable, visit.VisitType, normalFont);
                    AddCell(visitsTable, visit.VisitStatus, normalFont);
                    AddCell(visitsTable, visit.ExaminationFee.ToString("N2"), normalFont);
                }

                document.Add(visitsTable);

                // Footer
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph($"تم إنشاء التقرير في: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 30;
                document.Add(footer);

                document.Close();
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Monthly Report - التقرير الشهري
        // ====================================
        public bool GenerateMonthlyReport(int year, int month, Dictionary<string, object> stats, List<Visit> visits, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                Font titleFont = new Font(_arabicFont, 20, Font.BOLD, BaseColor.BLUE);
                Font headerFont = new Font(_arabicFont, 14, Font.BOLD);
                Font normalFont = new Font(_arabicFont, 12);

                // العنوان
                Paragraph title = new Paragraph($"التقرير الشهري - {month}/{year}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // الإحصائيات
                Paragraph statsTitle = new Paragraph("الإحصائيات العامة", headerFont);
                statsTitle.SpacingBefore = 10;
                statsTitle.SpacingAfter = 10;
                document.Add(statsTitle);

                PdfPTable statsTable = new PdfPTable(2);
                statsTable.WidthPercentage = 100;
                statsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                foreach (var stat in stats)
                {
                    AddTableRow(statsTable, stat.Key + ":", stat.Value.ToString(), headerFont, normalFont);
                }

                document.Add(statsTable);

                // Footer
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph($"تم إنشاء التقرير في: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 30;
                document.Add(footer);

                document.Close();
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Log error: ex.Message
                return false;
            }
        }

        // ====================================
        // Helper Methods
        // ====================================
        private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.BOTTOM_BORDER;
            labelCell.BorderColor = BaseColor.LIGHT_GRAY;
            labelCell.Padding = 8;
            labelCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            labelCell.BackgroundColor = new BaseColor(245, 245, 245);
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.BOTTOM_BORDER;
            valueCell.BorderColor = BaseColor.LIGHT_GRAY;
            valueCell.Padding = 8;
            valueCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            table.AddCell(valueCell);
        }

        private void AddHeaderCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = new BaseColor(33, 150, 243);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 10;
            cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            table.AddCell(cell);
        }

        private void AddCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Border = Rectangle.BOTTOM_BORDER;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            cell.Padding = 8;
            cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cell);
        }
    }
}