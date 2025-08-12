namespace FaizHesaplamaAPI.Models
{
    public class UserDefinedRule
    {

        public string ColumnName { get; set; }
        public string Operation { get; set; }
        public List<string> Values { get; set; }
    }
}
