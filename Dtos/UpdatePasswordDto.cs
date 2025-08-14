using System.Security.Cryptography.X509Certificates;

namespace FaizHesaplamaAPI.Dtos
{
    public class UpdatePasswordDto
    {
        public int id { get; set; }
        public string email { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}
