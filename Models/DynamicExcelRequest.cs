using Microsoft.AspNetCore.Http;


namespace FaizHesaplamaAPI.Models
{
    public class DynamicExcelRequest
    {

        public IFormFile File { get; set; }
        public string RulesJson { get; set; }
    }
}
