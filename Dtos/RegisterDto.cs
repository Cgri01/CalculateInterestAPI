using System.Security;

namespace FaizHesaplamaAPI.Dtos
{
    public class RegisterDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string role { get; set; } = "User";
        public string VerificationCode { get; set; }


}
}
