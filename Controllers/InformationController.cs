using Microsoft.AspNetCore.Mvc.Abstractions;
using FaizHesaplamaAPI.Data;
using FaizHesaplamaAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FaizHesaplamaAPI.Services;
using OfficeOpenXml;

namespace FaizHesaplamaAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class InformationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly FaizHesaplamaService _faizHesaplamaService;


        public InformationController(AppDbContext context, IConfiguration configuration , Services.FaizHesaplamaService faizHesaplamaService)
        {
            _context = context;
            _configuration = configuration;
            _faizHesaplamaService = faizHesaplamaService;
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

                        //Her satırı islemek:
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                //Verileri oku;
                                var model = new FaizHesaplamaModel
                                {
                                    SiraNo = worksheet.Cells[row, 1].GetValue<int>(),
                                    Banka = worksheet.Cells[row, 2].GetValue<string>(),
                                    ilkTarih = DateOnly.FromDateTime(worksheet.Cells[row, 3].GetValue<DateTime>()),
                                    sonTarih = DateOnly.FromDateTime(worksheet.Cells[row, 5].GetValue<DateTime>()),
                                    bsmv = worksheet.Cells[row, 4].Text
                                    //bsmv = worksheet.Cells[row, 4].GetValue<string>()
                                };

                                //Faiz hesapla:
                                var(faizTutari , ortalamaFaiz) = _faizHesaplamaService.HesaplaFaiz(model.ilkTarih, model.sonTarih, model.bsmv);


                                //Sonucları excele yazma:
                                worksheet.Cells[row, 6].Value = ortalamaFaiz.ToString("F2") + "%";
                                worksheet.Cells[row, 7].Value = faizTutari.ToString("F2");

                                //Başlık verme:
                                worksheet.Cells[1, 6].Value = "Ortalama Faiz Oranı";
                                worksheet.Cells[1, 7].Value = "Faiz Tutarı";
                            }
                            catch (Exception ex)
                            {
                                //Hata olursa, satırın sonuna hata mesajını yaz:
                                worksheet.Cells[row, 8].Value = "Hata: " + ex.Message;
                            }
                            
                        }
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
