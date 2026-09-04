using System.ComponentModel.DataAnnotations;

namespace SOLTIUS_Web_API_Add_On.Models.Configuration
{
    public class LogDatabaseConfig
    {
        [Required]
        public string Server { get; set; } = "";

        public int Port { get; set; } = 0;

        [Required]
        public string DatabaseName { get; set; } = "";

        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
