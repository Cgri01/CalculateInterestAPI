using Microsoft.AspNetCore.Mvc.Abstractions;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FaizHesaplamaAPI.Services;
using OfficeOpenXml;
using System.Text.Json;

namespace FaizHesaplamaAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class InformationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly FaizHesaplamaService _faizHesaplamaService;
        private readonly DynamicExcelProcessor _excelProcessor;


        public InformationController(AppDbContext context, IConfiguration configuration , Services.FaizHesaplamaService faizHesaplamaService, DynamicExcelProcessor excelProcessor)
        {
            _context = context;
            _configuration = configuration;
            _faizHesaplamaService = faizHesaplamaService;
            _excelProcessor = excelProcessor;
        }

        //GET: api/information

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Information>>> GetAllInformations()
        {
            var informations = await _context.Information.ToListAsync();
            return Ok(informations);
        }

        // GET: api/information/{id}


        // POST
        [HttpPost("count")]
        public IActionResult HesaplaFaiz([FromBody] FaizHesaplamaModel model)
        {
            try
            {
                var (faizTutari, ortalamaFaiz) = _faizHesaplamaService.HesaplaFaiz(model.ilkTarih, model.sonTarih, model.bsmv);

                return Ok(new
                {
                    SiraNo = model.SiraNo,
                    Banka = model.Banka,
                    ilkTarih = model.ilkTarih,
                    bsmv = model.bsmv,
                    faizOrani = ortalamaFaiz.ToString("F2") + "%",
                    faizTutari = faizTutari.ToString("F2")

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hata Olustu:" + ex.Message });
            }

        }

        // EXCEL OKUMA-YAZMA:
        [HttpPost("excel")]
        public IActionResult ProcessExcel(IFormFile file)
        {
            try
            {
                //Excel dosyasını işle
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);

                    //ERPPlus ile excel dosyasını acmak:
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension?.Rows ?? 0;

                        var mapper = new ExcelColumnMapper();
                        mapper.mapColumns(worksheet);

                       

                        //Başlıkların indexlerinin olabilecek değerlerini yazma:
                        int? siraNoCol = mapper.GetColumnIndex(new[] { "SiraNo", "sıra no", "SıraNo", "sirano", "numara", "no", "number" });
                        int? BankaCol = mapper.GetColumnIndex(new[] { "Banka", "banka adı", "banka adi", "bank", "Banka Adı", "BankaAdı" });
                        int? ilkTarihCol = mapper.GetColumnIndex(new[] { "tutarın hesabımızdan çıktığı tarih", "ilk tarih", "Başlangıç Tarihi", "Baslangic Tarihi", "tutarın hesabimizdan ciktigi tarih" });
                        int? bsmvCol = mapper.GetColumnIndex(new[] {"bsmv" , "bsmv tutarı", "bsmv tutari", "Bsmv", "Bsmv Tutarı", "Bsmv Tutarı" , "vergi" , "tax" , "Vergi" , "bsmv vergisi" });
                        int? sonTarihCol = mapper.GetColumnIndex(new[] { "hesaba iade geldiği tarih", "son tarih", "Son Tarih", "Bitiş Tarihi", "Bitis Tarihi", "Bitme Tarihi", "End Date", "Ending Date", "hesaba iade geldigi tarih" });

                        if (!siraNoCol.HasValue || !BankaCol.HasValue || !ilkTarihCol.HasValue || !bsmvCol.HasValue || !sonTarihCol.HasValue)
                        {
                            return BadRequest("Excel dosyasında gerekli başlıklar bulunamadı.");
                        }

                        WriteHeaders(worksheet);
                        //Her satırı islemek:
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var model = new FaizHesaplamaModel
                                {
                                    SiraNo = worksheet.Cells[row, siraNoCol.Value].GetValue<int>(),
                                    Banka = worksheet.Cells[row, BankaCol.Value].GetValue<string>(),
                                    ilkTarih = DateOnly.FromDateTime(worksheet.Cells[row , ilkTarihCol.Value].GetValue<DateTime>()),
                                    bsmv = worksheet.Cells[row , bsmvCol.Value].Text,
                                    sonTarih = DateOnly.FromDateTime(worksheet.Cells[row , sonTarihCol.Value].GetValue<DateTime>())
                                };


                                
                             //   //Verileri oku;
                             //   var model = new FaizHesaplamaModel
                             //   {
                             //       SiraNo = worksheet.Cells[row, 1].GetValue<int>(),
                             //       Banka = worksheet.Cells[row, 2].GetValue<string>(),
                             //       ilkTarih = DateOnly.FromDateTime(worksheet.Cells[row, 3].GetValue<DateTime>()),
                             //       sonTarih = DateOnly.FromDateTime(worksheet.Cells[row, 5].GetValue<DateTime>()),
                             //       bsmv = worksheet.Cells[row, 4].Text
                             //       //bsmv = worksheet.Cells[row, 4].GetValue<string>()
                             //   };

                                //Faiz hesapla:
                                var (faizTutari , ortalamaFaiz) = _faizHesaplamaService.HesaplaFaiz(model.ilkTarih, model.sonTarih, model.bsmv);


                                //Sonucları excele yazma:
                                WriteResult(worksheet , row , ortalamaFaiz, faizTutari);


                                //   worksheet.Cells[row, 6].Value = ortalamaFaiz.ToString("F2") + "%";
                                //   worksheet.Cells[row, 7].Value = faizTutari.ToString("F2");

                                //   //Başlık verme:
                                //   worksheet.Cells[1, 6].Value = "Ortalama Faiz Oranı";
                                //   worksheet.Cells[1, 7].Value = "Faiz Tutarı";
                            }
                            catch (Exception ex)
                            {
                                //Hata olursa, satırın sonuna hata mesajını yaz:
                                //worksheet.Cells[row, 8].Value = "Hata: " + ex.Message;
                                worksheet.Cells[row, worksheet.Dimension.Columns + 1 ].Value = "Hata: " + ex.Message;

                            }

                        }

                        //Basliklari Yazma:

                        //WriteHeaders(worksheet);



                        var outputStream = new MemoryStream(package.GetAsByteArray());
                        return File(outputStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" , "HesaplanmısFaizler.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private int? _resultStartCol = null;
        private bool _headersWrittern = false;
        private void WriteHeaders(ExcelWorksheet worksheet)
        {
            if (_headersWrittern) return;

            //İlk Boş sütunu bulma:
            int lastCol = worksheet.Dimension?.Columns ?? 0;
            _resultStartCol = lastCol + 1;

            worksheet.Cells[1, _resultStartCol.Value].Value = "Ortalama Faiz Oranı";
            worksheet.Cells[1, _resultStartCol.Value + 1].Value = "Faiz Tutarı";

            _headersWrittern = true;

        }

        private void WriteResult(ExcelWorksheet workSheet, int row, decimal ortalamaFaiz, decimal faizTutari)
        {
            if (!_resultStartCol.HasValue)
            {
                throw new InvalidOperationException("Başlıklar yazılmadan sonuc alınamaz");
            }

            workSheet.Cells[row, _resultStartCol.Value].Value = ortalamaFaiz.ToString("F2") + "%";
            workSheet.Cells[row, _resultStartCol.Value + 1].Value = faizTutari.ToString("F2");
        }

        //private bool _headersWritten = false;

        //private void WriteHeaders(ExcelWorksheet worksheet)
        //{
        //    if (_headersWritten) return;

        //    int lastCol = worksheet.Dimension?.Columns ?? 0;
        //    worksheet.Cells[1, lastCol + 1].Value = "Ortalama Faiz Oranı"; // 1. satır , 1. sütün [1 , lastCol]
        //    worksheet.Cells[1, lastCol + 2].Value = "Faiz Tutarı";
        //    _headersWritten = true;
        //}



        //private void WriteResult(ExcelWorksheet worksheet, int row, decimal ortalamaFaiz, decimal faizTutari)
        //{
        //    int lastCol = worksheet.Dimension?.Columns ?? 0;


        //    worksheet.Cells[row , lastCol + 1].Value = ortalamaFaiz.ToString("F2") + "%";
        //    worksheet.Cells[row, lastCol + 2].Value = faizTutari.ToString("F2");

        //}



        //Başlıkların Excel'de nerede olduğunu belirleme:
        public class ExcelColumnMapper
        {
            private readonly Dictionary<string, int> _columnMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public void mapColumns(ExcelWorksheet worksheet)
            {
                int columnCount = worksheet.Dimension?.Columns ?? 0;

                for (int col = 1; col <= columnCount; col ++)
                {
                    string header = worksheet.Cells[ 1 , col].Text?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(header))
                    {
                        _columnMappings[header] = col;
                    }
                }

            }

            public int? GetColumnIndex(string[] possibleNames)
            {
                foreach (var name in possibleNames)
                {
                    if (_columnMappings.TryGetValue(name, out int columnIndex))
                    {
                        return columnIndex;
                    }
                }
                return null;

            }

        }

        //DINAMIK EXCEL ISLEMLERI:
        [HttpPost("dynamic-excel")]
        public IActionResult ProcessExcelWithUserRules([FromForm] DynamicExcelRequest request)
        {
            try
            {
                var rules = JsonSerializer.Deserialize<List<UserDefinedRule>>(request.RulesJson);
                var resultStream = _excelProcessor.ProcessExcelWithUserRules(request.File, rules);

                return File(resultStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Sonuclar.xlsx");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }





        public class FaizHesaplamaModel
        {
            public int SiraNo { get; set; }
            public string Banka { get; set; }
            public DateOnly ilkTarih { get; set; }
            public DateOnly sonTarih { get; set; }
            public string bsmv { get; set; }
            public string faizOrani { get; set; }
        }
    }
}
