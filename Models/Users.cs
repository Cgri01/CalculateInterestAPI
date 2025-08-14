namespace FaizHesaplamaAPI.Models
{
    public class Users
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string passwordHash { get; set; }
        public string role { get; set; } = "User"; //Default role is User, can be changed to Admin later
        public bool? emailVerified { get; set; } //bool? null değer alabilir
    }
}
