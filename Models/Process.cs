using Microsoft.EntityFrameworkCore;

namespace FaizHesaplamaAPI.Models
{
    //[Keyless]
    public class Process
    {
        public int id { get; set; }
        public DateOnly ilkTarih { get; set; }
        public DateOnly? sonTarih { get; set; } //Nullable
        public string faizOrani { get; set; }
    }
}
