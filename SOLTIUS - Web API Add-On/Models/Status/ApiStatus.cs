namespace SOLTIUS_Web_API_Add_On.Models.Status
{
    public class ApiStatus
    {
        public bool Configured { get; set; }
        public bool DatabaseConnection { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
