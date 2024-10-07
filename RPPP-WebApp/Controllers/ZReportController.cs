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
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Controllers
{
    public class ZReportController : Controller
    {

        private readonly Rppp08Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
		private readonly ILogger<ZReportController> logger;

		public ZReportController(Rppp08Context ctx, IWebHostEnvironment environment, ILogger<ZReportController> logger)
        {
            this.ctx = ctx;
            this.environment = environment;
			this.logger = logger;
        }

        #region Jednostavni export u Excel

        public async Task<IActionResult> ZzadaciExcel()
        {
            var zadaci = await ctx.Zadataks
                                    .Include(z => z.Zahtjev)
                                    .Include(z => z.StatusZadatka)
                                    .AsNoTracking()
                                    .OrderBy(z => z.ZadatakId)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zadataka";
                excel.Workbook.Properties.Author = "RPPP8 - Zvonimir Mabić";
                var worksheet = excel.Workbook.Worksheets.Add("Zadaci");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Opis";
                worksheet.Cells[1, 3].Value = "Status zadatka";
                worksheet.Cells[1, 4].Value = "Zahtjev";

                for (int i = 0; i < zadaci.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zadaci[i].ZadatakId;
                    worksheet.Cells[i + 2, 2].Value = zadaci[i].Opis ?? "Opis nije definiran";
                    worksheet.Cells[i + 2, 3].Value = zadaci[i].StatusZadatka.NazivStatusaZadatka ?? "Nema naziv statusa";
                    worksheet.Cells[i + 2, 4].Value = zadaci[i].Zahtjev.NazivZahtjeva ?? "Nema naziva zahtjeva";
                }

                worksheet.Cells[1, 1, zadaci.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "zadaci.xlsx");
        }

        public async Task<IActionResult> ZsuradniciExcel()
        {
            var suradnici = await ctx.Suradniks
                                    .AsNoTracking()
                                    .OrderBy(s => s.SuradnikId)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis suradnika";
                excel.Workbook.Properties.Author = "RPPP8 - Zvonimir Mabić";
                var worksheet = excel.Workbook.Worksheets.Add("Suradnici");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Ime";
                worksheet.Cells[1, 3].Value = "Prezime";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Broj mobitela";

                for (int i = 0; i < suradnici.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = suradnici[i].SuradnikId;
                    worksheet.Cells[i + 2, 2].Value = suradnici[i].Ime ?? "Ime nije postavljeno";
                    worksheet.Cells[i + 2, 3].Value = suradnici[i].Prezime ?? "Prezime nije postavljeno";
                    worksheet.Cells[i + 2, 4].Value = suradnici[i].Email ?? "Email nije postavljen";
                    worksheet.Cells[i + 2, 5].Value = suradnici[i].BrojMobitela ?? "Broj mobitela nije postavljen";
                }

                worksheet.Cells[1, 1, suradnici.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "suradnici.xlsx");
        }

		#endregion

		#region Import iz Excela

		public List<Zadatak> ReadZadatakExcel(string filePath)
		{
			var result = new List<Zadatak>();

			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var worksheet = package.Workbook.Worksheets[0];

				if (worksheet.Cells[1, 1].Value.ToString() != "Id"
					|| worksheet.Cells[1, 2].Value.ToString() != "Opis"
					|| worksheet.Cells[1, 3].Value.ToString() != "Status zadatka"
					|| worksheet.Cells[1, 4].Value.ToString() != "Zahtjev")
				{
					throw new InvalidDataException("Format datoteke nije ispravan");
				}

				for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
				{
					#region Provjera podataka
					var zadatakIdCellValue = worksheet.Cells[row, 1].Value;
					if (zadatakIdCellValue == null || !int.TryParse(zadatakIdCellValue.ToString(), out int zadatakId))
					{
						// Neuspjela konverzija ili prazna ćelija
						result.Add(null);
						continue;
					}

					var existingZadatak = ctx.Zadataks.Find(zadatakId);
					if (existingZadatak != null)
					{
						// zadatak sa ovim id-jem već postoji u bazi
						result.Add(null);
						continue;
					}

					var statusZadatkaCellValue = worksheet.Cells[row, 3].Value?.ToString();
					if (string.IsNullOrEmpty(statusZadatkaCellValue))
					{
						// Neuspjela konverzija ili prazna ćelija
						result.Add(null);
						continue;
					}

					var statusZadatka = ctx.StatusZadatkas.FirstOrDefault(sz => sz.NazivStatusaZadatka == statusZadatkaCellValue);
					if (statusZadatka == null)
					{
						// Status zadatka sa tim nazivom ne postoji
						result.Add(null);
						continue;
					}
					var statusZadatkaId = statusZadatka.StatusZadatkaId;

					var zahtjevCellValue = worksheet.Cells[row, 4].Value?.ToString();
					if (string.IsNullOrEmpty(zahtjevCellValue))
					{
						// Neuspjela konverzija ili prazna ćelija
						result.Add(null);
						continue;
					}

					var zahtjev = ctx.Zahtjevs.Where(z => z.NazivZahtjeva == zahtjevCellValue).FirstOrDefault();
					if (zahtjev == null)
					{
						// Zahtjev sa tim nazivom ne postoji
						result.Add(null);
						continue;
					}
					var zahtjevId = zahtjev.ZahtjevId;

					if (worksheet.Cells[row, 2].Value.ToString() == "" || worksheet.Cells[row, 2].Value.ToString() == null)
					{
						// Opis nije definiran
						result.Add(null);
						continue;
					}
					#endregion

					Zadatak myModel = new Zadatak
					{
						Opis = worksheet.Cells[row, 2].Value.ToString(),
						StatusZadatkaId = statusZadatkaId,
						ZahtjevId = zahtjevId
					};
					ctx.Entry(myModel).State = EntityState.Added;
					ctx.SaveChanges();
					logger.LogInformation("Zahtjev " + myModel.Opis + " kreiran.");
					result.Add(myModel);
				}
			}

			return result;
		}


		[HttpPost]
		public IActionResult UploadExcel(IFormFile excelFile)
		{
			if (excelFile != null && excelFile.Length > 0)
			{
				var filePath = Path.Combine(environment.ContentRootPath, "wwwroot/", excelFile.FileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					excelFile.CopyTo(stream);
				}

                
                try {
					var result = ReadZadatakExcel(filePath);

					AddNewColumnToExcel(filePath, result);

					return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "rezultatDodavanja.xlsx");
				}
                catch (InvalidDataException e)
                { 
					return BadRequest(e.Message);
				}
                catch (Exception e)
                {
                    return BadRequest("Pogreška servera." + e.Message);
                }
			}

			return BadRequest("Nije odabrana datoteka za prijenos.");
		}

		private void AddNewColumnToExcel(string filePath, List<Zadatak> result)
		{
			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var worksheet = package.Workbook.Worksheets[0];

				worksheet.Cells[1, 5].Value = "Dodan u bazu";

				for (int i = 0; i < result.Count; i++)
				{
					worksheet.Cells[i + 2, 5].Value = (result[i] != null) ? "Da" : "Ne";
					worksheet.Cells[i + 2, 1].Value = result[i] == null ? worksheet.Cells[i + 2, 1].Value.ToString() : result[i].ZadatakId;
				}

				package.Save();
			}
		}



		#endregion

		#region Složeni export u Excel

		public async Task<IActionResult> ZadatakMaster(int id)
		{
			var zadatak = await ctx.Zadataks.Where(z => z.ZadatakId == id).Include(z => z.StatusZadatka).Include(z => z.Zahtjev).FirstOrDefaultAsync();
			if (zadatak == null) return BadRequest("Zadatak sa ovim id-jem ne postoji");
			var suradnici = await ctx.ZadatakSuradniks
										.Where(zs => zs.ZadatakId == zadatak.ZadatakId)
										.Select(zs => zs.Suradnik)
										.OrderBy(s => s.SuradnikId)
										.ToListAsync();
			byte[] content;
			using (ExcelPackage excel = new ExcelPackage())
			{
				excel.Workbook.Properties.Title = "Zadatak Master-detail";
				excel.Workbook.Properties.Author = "RPPP8 - Zvonimir Mabić";
				var worksheet = excel.Workbook.Worksheets.Add("Zadaci");

				//First add the headers
				worksheet.Cells[1, 1].Value = "Id";
				worksheet.Cells[1, 2].Value = "Opis";
				worksheet.Cells[1, 3].Value = "Status zadatka";
				worksheet.Cells[1, 4].Value = "Zahtjev";

				worksheet.Cells[2, 1].Value = zadatak.ZadatakId;
				worksheet.Cells[2, 2].Value = zadatak.Opis;
				worksheet.Cells[2, 3].Value = zadatak.StatusZadatka.NazivStatusaZadatka;
				worksheet.Cells[2, 4].Value = zadatak.Zahtjev.NazivZahtjeva;

				worksheet.Cells[4, 1].Value = "Id";
				worksheet.Cells[4, 2].Value = "Ime";
				worksheet.Cells[4, 3].Value = "Prezime";
				worksheet.Cells[4, 4].Value = "Email";
				worksheet.Cells[4, 5].Value = "Broj mobitela";

				for (int i = 0; i < suradnici.Count; i++)
				{
					worksheet.Cells[i + 5, 1].Value = suradnici[i].SuradnikId;
					worksheet.Cells[i + 5, 2].Value = suradnici[i].Ime ?? "Ime nije postavljeno";
					worksheet.Cells[i + 5, 3].Value = suradnici[i].Prezime ?? "Prezime nije postavljeno";
					worksheet.Cells[i + 5, 4].Value = suradnici[i].Email ?? "Email nije postavljen";
					worksheet.Cells[i + 5, 5].Value = suradnici[i].BrojMobitela ?? "Broj mobitela nije postavljen";
				}

				worksheet.Cells[1, 1, suradnici.Count + 1, 4].AutoFitColumns();

				content = excel.GetAsByteArray();
			}
			return File(content, ExcelContentType, $"zadatak{zadatak.ZadatakId}Master.xlsx");
		}


		#endregion

		#region PDF 

		private PdfReport CreateReport(string naslov)
		{
			var pdf = new PdfReport();

			pdf.DocumentPreferences(doc =>
			{
				doc.Orientation(PageOrientation.Portrait);
				doc.PageSize(PdfPageSize.A4);
				doc.DocumentMetadata(new DocumentMetadata
				{
					Author = "RPPP8 - Zvonimir Mabić",
					Application = "RPPP8 Projekt",
					Title = naslov
				});
				doc.Compression(new CompressionSettings
				{
					EnableCompression = true,
					EnableFullCompression = true
				});
			})
			.DefaultFonts(fonts => {
				fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
								Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"));
				fonts.Size(9);
				fonts.Color(System.Drawing.Color.Black);
			})
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


		public async Task<IActionResult> SuradniciPdf()
		{
			string naslov = "Popis suradnika";
			var suradnici = await ctx.Suradniks
								  .AsNoTracking()
								  .OrderBy(s => s.SuradnikId)
								  .ToListAsync();
			PdfReport report = CreateReport(naslov);
			#region Podnožje i zaglavlje
			report.PagesFooter(footer =>
			{
				footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
			})
			.PagesHeader(header =>
			{
				header.CacheHeader(cache: true);
				header.DefaultHeader(defaultHeader =>
				{
					defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
					defaultHeader.Message(naslov);
				});
			});

			#endregion
			#region Postavljanje izvora podataka i stupaca
			report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(suradnici));

			report.MainTableColumns(columns =>
			{
				columns.AddColumn(column =>
				{
					column.IsRowNumber(true);
					column.CellsHorizontalAlignment(HorizontalAlignment.Right);
					column.IsVisible(true);
					column.Order(0);
					column.Width(1);
					column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName(nameof(Suradnik.SuradnikId));
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(1);
					column.Width(1);
					column.HeaderCell("Id");
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Suradnik>(s => s.Ime);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(2);
					column.Width(2);
					column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Suradnik>(s => s.Prezime);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(3);
					column.Width(2);
					column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Suradnik>(s => s.Email);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(4);
					column.Width(4);
					column.HeaderCell("Email", horizontalAlignment: HorizontalAlignment.Center);
				});
				columns.AddColumn(column =>
				{
					column.PropertyName<Suradnik>(s => s.BrojMobitela);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(4);
					column.Width(4);
					column.HeaderCell("Broj mobitela", horizontalAlignment: HorizontalAlignment.Center);
				});
			});

			#endregion
			byte[] pdf = report.GenerateAsByteArray();

			if (pdf != null)
			{
				Response.Headers.Add("content-disposition", "inline; filename=suradnici.pdf");
				//return File(pdf, "application/pdf");
				return File(pdf, "application/pdf", "suradnici.pdf"); //Otvara save as dialog
			}
			else
			{
				return NotFound();
			}
		}

		public async Task<IActionResult> ZadaciPdf()
		{
			string naslov = "Popis zadataka";
			var zadaci = await ctx.Zadataks
								  .AsNoTracking()
								  .Include(z => z.Zahtjev)
								  .Include(z => z.StatusZadatka)
								  .OrderBy(z => z.ZadatakId)
								  .ToListAsync();
			PdfReport report = CreateReport(naslov);
			#region Podnožje i zaglavlje
			report.PagesFooter(footer =>
			{
				footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
			})
			.PagesHeader(header =>
			{
				header.CacheHeader(cache: true);
				header.DefaultHeader(defaultHeader =>
				{
					defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
					defaultHeader.Message(naslov);
				});
			});
			#endregion
			#region Postavljanje izvora podataka i stupaca
			report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zadaci));

			report.MainTableColumns(columns =>
			{
				columns.AddColumn(column =>
				{
					column.IsRowNumber(true);
					column.CellsHorizontalAlignment(HorizontalAlignment.Right);
					column.IsVisible(true);
					column.Order(0);
					column.Width(1);
					column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName(nameof(Zadatak.ZadatakId));
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(1);
					column.Width(1);
					column.HeaderCell("Id");
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Zadatak>(z => z.Opis);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(2);
					column.Width(2);
					column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Zadatak>(z => z.StatusZadatka.NazivStatusaZadatka);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(3);
					column.Width(2);
					column.HeaderCell("Status zadatka", horizontalAlignment: HorizontalAlignment.Center);
				});

				columns.AddColumn(column =>
				{
					column.PropertyName<Zadatak>(z => z.Zahtjev.NazivZahtjeva);
					column.CellsHorizontalAlignment(HorizontalAlignment.Center);
					column.IsVisible(true);
					column.Order(4);
					column.Width(4);
					column.HeaderCell("Zahtjev", horizontalAlignment: HorizontalAlignment.Center);
				});
			});

			#endregion
			byte[] pdf = report.GenerateAsByteArray();

			if (pdf != null)
			{
				Response.Headers.Add("content-disposition", "inline; filename=zadaci.pdf");
				//return File(pdf, "application/pdf");
				return File(pdf, "application/pdf", "zadaci.pdf"); //Otvara save as dialog
			}
			else
			{
				return NotFound();
			}
		}

		#endregion
	}
}
