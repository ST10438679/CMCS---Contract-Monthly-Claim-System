using PROG6212_POE.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PROG6212_POE.Services
{
    public class InvoiceService
    {
        // We are using QuestPDF to help generate a PDF document to download
        // Placeholder information has been used to simulate an Invoice
        public InvoiceService()
        {
            // Set QuestPDF license (Community license for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Generate a PDF invoice for a claim
        /// </summary>
        /// <param name="claim">The claim to generate an invoice for</param>
        /// <returns>PDF as byte array</returns>
        public byte[] GenerateInvoicePDF(Claim claim)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, claim));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Generate invoice and save to file
        /// </summary>
        public void GenerateInvoiceToFile(Claim claim, string filePath)
        {
            var pdfBytes = GenerateInvoicePDF(claim);
            File.WriteAllBytes(filePath, pdfBytes);
        }

        /// <summary>
        /// Compose the header section of the invoice
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INVOICE")
                        .FontSize(32)
                        .Bold()
                        .FontColor("#5DA29A");

                    column.Item().PaddingTop(10).Text("Contract Monthly Claim System")
                        .FontSize(14)
                        .Bold()
                        .FontColor("#333333");
                });

                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text("CMCS")
                        .FontSize(20)
                        .Bold()
                        .FontColor("#A25D65");

                    column.Item().PaddingTop(5).Text("123 University Avenue")
                        .FontSize(10)
                        .FontColor("#666666");

                    column.Item().Text("Cape Town, 8001")
                        .FontSize(10)
                        .FontColor("#666666");

                    column.Item().Text("South Africa")
                        .FontSize(10)
                        .FontColor("#666666");

                    column.Item().PaddingTop(5).Text("Tel: +27 21 123 4567")
                        .FontSize(10)
                        .FontColor("#666666");

                    column.Item().Text("Email: finance@cmcs.ac.za")
                        .FontSize(10)
                        .FontColor("#666666");
                });
            });
        }

        /// <summary>
        /// Compose the main content section of the invoice
        /// </summary>
        private void ComposeContent(IContainer container, Claim claim)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(15);

                // Invoice Information
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Invoice Number:")
                            .Bold()
                            .FontSize(10)
                            .FontColor("#5DA29A");
                        col.Item().Text($"INV-{claim.claimId:D6}")
                            .FontSize(12)
                            .Bold();
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Invoice Date:")
                            .Bold()
                            .FontSize(10)
                            .FontColor("#5DA29A");
                        col.Item().Text(DateTime.Now.ToString("dd MMMM yyyy"))
                            .FontSize(12);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Claim Date:")
                            .Bold()
                            .FontSize(10)
                            .FontColor("#5DA29A");
                        col.Item().Text(claim.dateOfClaim.ToString("dd MMMM yyyy"))
                            .FontSize(12);
                    });
                });

                column.Item().LineHorizontal(2).LineColor("#5DA29A");

                // Billing Information
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Background("#F8F9FA")
                            .Padding(15)
                            .Column(innerCol =>
                            {
                                innerCol.Item().Text("Bill To:")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor("#5DA29A");

                                innerCol.Item().PaddingTop(8).Text($"{claim.theUser.Name} {claim.theUser.Surname}")
                                    .Bold()
                                    .FontSize(11);

                                innerCol.Item().Text($"Email: {claim.theUser.Email}")
                                    .FontSize(10)
                                    .FontColor("#666666");

                                innerCol.Item().Text($"Role: {claim.theUser.Role}")
                                    .FontSize(10)
                                    .FontColor("#666666");

                                if (claim.theUser.HourlyRate != null)
                                {
                                    innerCol.Item().Text($"Standard Rate: R {claim.theUser.HourlyRate:N2}/hour")
                                        .FontSize(10)
                                        .FontColor("#666666");
                                }
                            });
                    });

                    row.ConstantItem(20);

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Background("#F8F9FA")
                            .Padding(15)
                            .Column(innerCol =>
                            {
                                innerCol.Item().Text("Claim Status:")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor("#5DA29A");

                                innerCol.Item().PaddingTop(8).Text($"Status: {claim.claimStatus}")
                                    .FontSize(11)
                                    .FontColor(GetStatusColor(claim.claimStatus));

                                innerCol.Item().Text($"First Approval: {(claim.firstApproval ? "Approved" : "Pending")}")
                                    .FontSize(10)
                                    .FontColor(claim.firstApproval ? "#28a745" : "#ffc107");

                                if (!string.IsNullOrEmpty(claim.documentName))
                                {
                                    innerCol.Item().Text("Supporting Document: Attached")
                                        .FontSize(10)
                                        .FontColor("#666666");
                                }
                            });
                    });
                });

                // Claim Details Table
                column.Item().PaddingTop(20).Text("Claim Details")
                    .Bold()
                    .FontSize(14)
                    .FontColor("#5DA29A");

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Description
                        columns.RelativeColumn(1); // Hours
                        columns.RelativeColumn(1); // Rate
                        columns.RelativeColumn(1); // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background("#5DA29A")
                            .Padding(8)
                            .Text("Description")
                            .Bold()
                            .FontColor(Colors.White);

                        header.Cell().Background("#5DA29A")
                            .Padding(8)
                            .AlignCenter()
                            .Text("Hours Worked")
                            .Bold()
                            .FontColor(Colors.White);

                        header.Cell().Background("#5DA29A")
                            .Padding(8)
                            .AlignRight()
                            .Text("Rate per Hour")
                            .Bold()
                            .FontColor(Colors.White);

                        header.Cell().Background("#5DA29A")
                            .Padding(8)
                            .AlignRight()
                            .Text("Total Amount")
                            .Bold()
                            .FontColor(Colors.White);
                    });

                    // Content
                    table.Cell().BorderBottom(1).BorderColor("#E9ECEF")
                        .Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Lecturer Services")
                                .Bold()
                                .FontSize(11);
                            col.Item().PaddingTop(5).Text(claim.claimNotes)
                                .FontSize(9)
                                .FontColor("#666666");
                        });

                    table.Cell().BorderBottom(1).BorderColor("#E9ECEF")
                        .Padding(10)
                        .AlignCenter()
                        .Text(claim.hoursWorked.ToString())
                        .FontSize(11);

                    table.Cell().BorderBottom(1).BorderColor("#E9ECEF")
                        .Padding(10)
                        .AlignRight()
                        .Text($"R {claim.hourlyRate:N2}")
                        .FontSize(11);

                    table.Cell().BorderBottom(1).BorderColor("#E9ECEF")
                        .Padding(10)
                        .AlignRight()
                        .Text($"R {claim.totalAmount:N2}")
                        .Bold()
                        .FontSize(11);
                });

                // Payment Summary
                column.Item().PaddingTop(20).AlignRight().Column(col =>
                {
                    col.Item().Background("#F8F9FA")
                        .Padding(15)
                        .Width(300)
                        .Column(summaryCol =>
                        {
                            summaryCol.Item().Row(summaryRow =>
                            {
                                summaryRow.RelativeItem().Text("Subtotal:")
                                    .Bold()
                                    .FontSize(11);
                                summaryRow.RelativeItem().AlignRight().Text($"R {claim.totalAmount:N2}")
                                    .FontSize(11);
                            });

                            summaryCol.Item().PaddingTop(5).Row(summaryRow =>
                            {
                                summaryRow.RelativeItem().Text("VAT (0%):")
                                    .Bold()
                                    .FontSize(11);
                                summaryRow.RelativeItem().AlignRight().Text("R 0.00")
                                    .FontSize(11);
                            });

                            summaryCol.Item().PaddingTop(10).LineHorizontal(2).LineColor("#5DA29A");

                            summaryCol.Item().PaddingTop(10).Row(summaryRow =>
                            {
                                summaryRow.RelativeItem().Text("Total Amount Due:")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor("#5DA29A");
                                summaryRow.RelativeItem().AlignRight().Text($"R {claim.totalAmount:N2}")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor("#5DA29A");
                            });
                        });
                });

                // Notes
                column.Item().PaddingTop(20).Column(col =>
                {
                    col.Item().Text("Notes:")
                        .Bold()
                        .FontSize(12)
                        .FontColor("#5DA29A");

                    col.Item().PaddingTop(5).Text(GetStatusNote(claim.claimStatus))
                        .FontSize(10)
                        .FontColor("#666666");
                });

                // Terms & Conditions
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("Terms & Conditions:")
                        .Bold()
                        .FontSize(12)
                        .FontColor("#5DA29A");

                    col.Item().PaddingTop(5).Column(termsCol =>
                    {
                        termsCol.Item().Text("• Payment is subject to final approval by the Academic Manager")
                            .FontSize(9)
                            .FontColor("#666666");
                        termsCol.Item().Text("• All claims must be supported by appropriate documentation")
                            .FontSize(9)
                            .FontColor("#666666");
                        termsCol.Item().Text("• Payment will be processed within 30 days of approval")
                            .FontSize(9)
                            .FontColor("#666666");
                        termsCol.Item().Text("• All amounts are in South African Rand (ZAR)")
                            .FontSize(9)
                            .FontColor("#666666");
                    });
                });
            });
        }

        /// <summary>
        /// Compose the footer section of the invoice
        /// </summary>
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor("#E9ECEF");

                column.Item().PaddingTop(10).Text("Contract Monthly Claim System (CMCS)")
                    .Bold()
                    .FontSize(10)
                    .FontColor("#999999");

                column.Item().Text($"Generated on {DateTime.Now:dd MMMM yyyy HH:mm}")
                    .FontSize(9)
                    .FontColor("#999999");
            });
        }

        /// <summary>
        /// Get color based on claim status
        /// </summary>
        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Approved" => "#28a745",
                "Pending" => "#ffc107",
                "Rejected" => "#dc3545",
                _ => "#666666"
            };
        }

        /// <summary>
        /// Get note text based on claim status
        /// </summary>
        private string GetStatusNote(string status)
        {
            return status switch
            {
                "Approved" => "This invoice has been approved and is ready for payment processing. Payment will be processed according to the standard payment schedule.",
                "Pending" => "This invoice is pending approval. Payment will be processed once the claim is fully approved by the relevant authorities.",
                "Rejected" => "This invoice has been rejected. Please contact the HR department for more information.",
                _ => "Please contact HR for more information regarding this claim."
            };
        }
    }
}