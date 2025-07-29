using System.Text.Json.Serialization;
using FaizHesaplamaAPI.Converters;
using Microsoft.EntityFrameworkCore;

namespace FaizHesaplamaAPI.Models
{
    [Keyless]
    public class Information
    {
        public int SiraNo { get; set; }

        public string Banka { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly ilkTarih { get; set; }

        public string bsmv { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly sonTarih { get; set; }
        public string faizOrani { get; set; }
        public string faizTutari { get; set; }
    }
}
