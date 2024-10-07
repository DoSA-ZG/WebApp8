using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RPPP_WebApp.Models;

using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using Microsoft.VisualBasic;


namespace RPPP_WebApp.Controllers
{
    public class ReportController : Controller
    {
        
        private readonly Rppp08Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportController(Rppp08Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }
        
            /// <summary>
            /// Generira Excel datoteku koja sadrži popis kartica projekta.
            /// </summary>
            /// <returns>Excel datoteka koja sadrži popis kartica projekta.</returns>
            public async Task<IActionResult> KarticeProjektaExcel()
            {
            var kartice = await ctx.KarticaProjekta.Include(k => k.Projekt)
                                    .AsNoTracking()
                                    .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis kartica";
                excel.Workbook.Properties.Author = "RPPP-08";
                var worksheet = excel.Workbook.Worksheets.Add("Kartice Projekata");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Virtualni Iban";
                worksheet.Cells[1, 2].Value = "Stanje";
                worksheet.Cells[1, 3].Value = "Projekt";
                worksheet.Cells[1, 4].Value = "Šifra kartice";

                for (int i = 0; i < kartice.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = kartice[i].VirtualniIban;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = kartice[i].StanjeKartice;
                    worksheet.Cells[i + 2, 3].Value = kartice[i].Projekt.NazivProjekta;
                    worksheet.Cells[i + 2, 4].Value = kartice[i].KarticaProjektaId;
                }

                worksheet.Cells[1, 1, kartice.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "kartice.xlsx");
            }

        /// <summary>
        /// Generira Excel datoteku koja sadrži detalje o kartici projekta i pripadajućim transakcijama.
        /// </summary>
        /// <param name="id">ID kartice projekta za koju se generira izvješće.</param>
        /// <returns>Excel datoteka koja sadrži detalje o kartici projekta i transakcijama.</returns>
        public async Task<IActionResult> KarticaProjektaExcel(int id)
            {
            var kartica = await ctx.KarticaProjekta.Include(k => k.Projekt).Include(k => k.Transakcijas).ThenInclude(t => t.VrstaTransakcije).Where(k => k.KarticaProjektaId == id)
                                    .AsNoTracking()
                                    .FirstAsync();

            var transakcije = await ctx.Transakcijas.Where(t => t.KarticaProjektaId == id).Include(t => t.VrstaTransakcije).ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Kartica" + kartica.VirtualniIban;
                excel.Workbook.Properties.Author = "RPPP-08";
                var worksheet = excel.Workbook.Worksheets.Add("Kartice Projekata");

                worksheet.Cells[1, 1].Value = "Virtualni IBAN:";
                worksheet.Cells[1, 2].Value = kartica.VirtualniIban;
                worksheet.Cells[2, 1].Value = "Stanje:";
                worksheet.Cells[2, 2].Value = kartica.StanjeKartice  + " €";
                worksheet.Cells[3, 1].Value = "Projekt";
                worksheet.Cells[3, 2].Value = kartica.Projekt.NazivProjekta;
                
                worksheet.Cells[4, 1].Value = "";
                worksheet.Cells[4, 2].Value = "";

                //First add the headers
                worksheet.Cells[5, 1].Value = "Subjektov Iban";
                worksheet.Cells[5, 2].Value = "Primateljov Iban";
                worksheet.Cells[5, 3].Value = "Iznos";
                worksheet.Cells[5, 4].Value = "Datum";
                worksheet.Cells[5, 5].Value = "Opis";
                worksheet.Cells[5, 6].Value = "Vrsta Transakcije";

                for (int i = 0; i < transakcije.Count; i++)
                {
                    worksheet.Cells[i + 6, 1].Value = transakcije[i].SubjektIban;
                    worksheet.Cells[i + 6, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 6, 2].Value = transakcije[i].PrimateljIban;
                    worksheet.Cells[i + 6, 3].Value = transakcije[i].Iznos + " €";
                    
                    worksheet.Cells[i + 6, 4].Value = transakcije[i].DatumTransakcije;
                     worksheet.Cells[i + 6, 4].Style.Numberformat.Format = "dd-mm-yyyy"; // or any other date format you prefer

                    worksheet.Cells[i + 6, 5].Value = transakcije[i].Opis;
                    worksheet.Cells[i + 6, 6].Value = transakcije[i].VrstaTransakcije.NazivVrste;
                }

                worksheet.Cells[1, 1, transakcije.Count + 1, 6].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Kartica.xlsx");
            }


        /// <summary>
        /// Stvara PDF izvješće s nazivom naslova.
        /// </summary>
        /// <param name="naslov">Naziv izvješća.</param>
        /// <returns>Objekt za generiranje PDF izvješća.</returns
        private PdfReport CreateReport(string naslov)
            {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                Author = "FER-ZPR",
                Application = "Firma.MVC Core",
                Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                EnableCompression = true,
                EnableFullCompression = true
                });
            })
            //fix za linux https://github.com/VahidN/PdfReport.Core/issues/40
            .DefaultFonts(fonts => {
                fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                                Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"));
                fonts.Size(9);
                fonts.Color(System.Drawing.Color.Black);
            })
            //
            .MainTableTemplate(template =>
            {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                    //table.NumberOfDataRowsPerPage(20);
                    table.GroupsPreferences(new GroupsPreferences
                {
                GroupType = GroupType.HideGroupingColumns,
                RepeatHeaderRowPerGroup = true,
                ShowOneGroupPerPage = true,
                SpacingBeforeAllGroupsSummary = 5f,
                NewGroupAvailableSpacingThreshold = 150,
                SpacingAfterAllGroupsSummary = 5f
                });
                table.SpacingAfter(4f);
            });

            return pdf;
            }

              /// <summary>
            /// Generira PDF izvješće o kartici projekta i transakcijama.
            /// </summary>
            /// <param name="id">ID kartice projekta za koju se generira izvješće.</param>
            /// <returns>PDF izvješće o kartici projekta i transakcijama.</returns>
            public async Task<IActionResult> Kartica(int id) { 

                var kartica = await ctx.KarticaProjekta.Where(k => k.KarticaProjektaId == id).Include(k => k.Projekt).Include(k => k.Transakcijas).ThenInclude(t => t.VrstaTransakcije).FirstAsync();

                PdfReport report = CreateReport("Kartice Projekata");
                report.PagesFooter(footer => {
                    footer.DefaultFooter(DateTime.Now
                    .ToString("dd.MM.yyyy."));
                    });
                    
                report.PagesHeader( header =>
                        {
                            
                            header.CacheHeader(cache: true);
                            header.CustomHeader(new MasterDetailsHeaders("Transakcije na kartici projekta", kartica.VirtualniIban, kartica.StanjeKartice, kartica.Projekt.NazivProjekta)
                            {
                            PdfRptFont = header.PdfFont
                            });
                            
                        });


                report.MainTableSummarySettings(summarySettings => {
                    summarySettings.OverallSummarySettings("Ukupno");
                });



                report.MainTableDataSource(dataSource =>
                                dataSource.StronglyTypedList(kartica.Transakcijas));

                                report.MainTableColumns(columns => {
                                        columns.AddColumn(column => {
                                            column.IsRowNumber(true);
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                                            column.IsVisible(true); column.Order(0);column.Width(1);
                                            column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                                        });

                                        
                                        columns.AddColumn(column => {
                                            column.PropertyName("PrimateljIban");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.Order(1); column.Width(2);
                                            column.IsVisible(true);
                                            column.HeaderCell("Primatelj IBAN", horizontalAlignment: HorizontalAlignment.Left);
                                        });

                                        columns.AddColumn(column => {
                                            column.PropertyName("SubjektIban");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.IsVisible(true); column.Order(2); column.Width(2);
                                            column.HeaderCell("Subjekt IBAN", horizontalAlignment: HorizontalAlignment.Left);
                                        });


                                        columns.AddColumn(column => {
                                            column.PropertyName("Iznos");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.Order(3); column.Width(2);
                                            column.IsVisible(true);
                                            column.HeaderCell("Iznos", horizontalAlignment: HorizontalAlignment.Left);
                                            column.ColumnItemsTemplate(template => {
                                                template.TextBlock();
                                                template.DisplayFormatFormula(obj => obj != null ? string.Format("{0} €", obj) : string.Empty);
                                                });             
                                            column.AggregateFunction(aggregateFunction =>
                                                {
                                                    aggregateFunction.NumericAggregateFunction(AggregateFunction.Sum);
                                                    aggregateFunction.DisplayFormatFormula(obj => obj == null || string.IsNullOrEmpty(obj.ToString())
                                                                                        ? string.Empty : string.Format("{0:n0}", obj));
                                                });               
                                        });


                                        columns.AddColumn(column => {
                                            column.PropertyName("VrstaTransakcije.NazivVrste");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.IsVisible(true); column.Order(4); column.Width(1);
                                            column.HeaderCell("Vrsta Transakcije", horizontalAlignment: HorizontalAlignment.Left);
                                        });
                                        columns.AddColumn(column => {
                                            column.PropertyName("Opis");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.IsVisible(true); column.Order(5); column.Width(3);
                                            column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Left);
                                        });
                                        columns.AddColumn(column => {
                                            column.PropertyName("DatumTransakcije");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.IsVisible(true); column.Order(6); column.Width(2);
                                            column.HeaderCell("DatumTransakcije", horizontalAlignment: HorizontalAlignment.Left);
                                            column.ColumnItemsTemplate(template => {
                                            template.TextBlock();
                                            template.DisplayFormatFormula(obj => {
                                                if (obj == null || !(obj is DateTime))
                                                    return string.Empty;

                                                var date = (DateTime)obj;
                                                return date.ToString("dd.MM.yyyy."); // Formats the date to exclude time
                                            });
                                        });
                                        });

                                    });



                    byte[] pdf = report.GenerateAsByteArray();
                    if (pdf != null) {
                        Response.Headers.Add("content-disposition",
                        "inline; filename=drzave.pdf");
                        
                        return File(pdf, "application/pdf", "drzave.pdf");
                       
                    }
                    else return NotFound();

            }

            
            /// <summary>
            /// Generira PDF izvješće o svim karticama projekta.
            /// </summary>
            /// <returns>PDF izvješće o svim karticama projekta.</returns>
            public async Task<IActionResult> Kartice() { 

                var kartice = await ctx.KarticaProjekta.Include(k => k.Projekt).AsNoTracking().OrderBy(k => k.VirtualniIban).ToListAsync();

                PdfReport report = CreateReport("Kartice Projekata");
                report.PagesFooter(footer => {
                    footer.DefaultFooter(DateTime.Now
                    .ToString("dd.MM.yyyy."));
                    })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                        defaultHeader.RunDirection(
                        PdfRunDirection.LeftToRight);
                        defaultHeader.Message("Kartice Projekata");
                });
                });

                report.MainTableDataSource(dataSource =>
                                dataSource.StronglyTypedList(kartice));

                                report.MainTableColumns(columns => {
                                        columns.AddColumn(column => {
                                            column.IsRowNumber(true);
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                                            column.IsVisible(true); column.Order(0);column.Width(1);
                                            column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                                        });

                                        
                                        columns.AddColumn(column => {
                                            column.PropertyName("VirtualniIban");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.Order(1); column.Width(2);
                                            column.IsVisible(true);
                                            column.HeaderCell("Virtualni IBAN", horizontalAlignment: HorizontalAlignment.Left);
                                        });

                                        columns.AddColumn(column => {
                                            column.PropertyName("Projekt.NazivProjekta");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.IsVisible(true); column.Order(2); column.Width(3);
                                            column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Left);
                                        });


                                        columns.AddColumn(column => {
                                            column.PropertyName("StanjeKartice");
                                            column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                            column.Order(2); column.Width(1);
                                            column.IsVisible(true);
                                            column.HeaderCell("Stanje Kartice", horizontalAlignment: HorizontalAlignment.Left);
                                            column.ColumnItemsTemplate(template => {
                                                template.TextBlock();
                                                template.DisplayFormatFormula(obj => obj != null ? string.Format("{0} €", obj) : string.Empty);
                                                });                            
                                                   });
                                    });



                    byte[] pdf = report.GenerateAsByteArray();
                    if (pdf != null) {
                        Response.Headers.Add("content-disposition",
                        "inline; filename=drzave.pdf");
                        
                        return File(pdf, "application/pdf", "drzave.pdf");
                       
                    }
                    else return NotFound();

            }

            /// <summary>
            /// Implementacija IPageHeader za prikaz glave stranice u PDF izvješću s detaljima o kartici projekta.
            /// </summary>
            public class MasterDetailsHeaders : IPageHeader {

            private string naslov;
            private string virtualniIban;
            private int? stanje;
            private string projekt;


            public MasterDetailsHeaders(string naslov, string virtualniIban, int? stanje, string projekt) {
            this.naslov = naslov;
            this.virtualniIban = virtualniIban;
            this.stanje = stanje;
            this.projekt = projekt;
            }

             public IPdfFont PdfRptFont { set; get; }

            public PdfGrid RenderingReportHeader(Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData) {
                var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = naslov;
                        cellProperties.PdfFont = PdfRptFont; 
                        cellProperties.FixedHeight = 30;
                        cellProperties.FontColor = new BaseColor(0, 0, 0);
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    });
                table.AddSimpleRow( (cellData, cellProperties) => {
                    cellData.Value = "Virtualni IBAN:" + virtualniIban;
                    cellProperties.FontColor = new BaseColor(0, 0, 0);
                    cellProperties.PdfFont = PdfRptFont;
                });
                table.AddSimpleRow((cellData, cellProperties) => {
                    cellData.Value = "Stanje: " + stanje + "€";
                    cellProperties.FontColor = new BaseColor(0, 0, 0);
                    cellProperties.PdfFont = PdfRptFont;
                });
               
                table.AddSimpleRow( (cellData, cellProperties) => {
                    cellData.Value = "Projekt: " + projekt;
                    cellProperties.FontColor = new BaseColor(0, 0, 0);
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PaddingBottom = 12;
                });
                return table.AddBorderToTable();
            }

            public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData) {
                var projekt = newGroupInfo.GetSafeStringValueOf("Projekt.NazivProjekta");
                var stanje = (decimal) newGroupInfo.GetValueOf("Stanje");
                var iban = newGroupInfo.GetSafeStringValueOf("VirtualniIban");

                var table = new PdfGrid(relativeWidths: new[]{ 2f, 4f }) { WidthPercentage = 100 };
                table.AddSimpleRow(
                    (cellData, cellProperties) => {
                        cellData.Value = "Projekt:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = projekt;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    }
                );
                table.AddSimpleRow(
                    (cellData, cellProperties) => {
                        cellData.Value = "Stanje:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = stanje;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    }
                );
                table.AddSimpleRow(
                    (cellData, cellProperties) => {
                        cellData.Value = "Virtualni IBAN:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) => {
                        cellData.Value = iban;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment =
                        HorizontalAlignment.Left;
                    }
                );
                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }
            }
    }
}
