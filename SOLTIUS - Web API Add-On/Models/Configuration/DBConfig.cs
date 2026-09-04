namespace SOLTIUS_Web_API_Add_On.Models.Configuration
{
    public class DBConfig
    {
        public DatabaseType DBType { get; set; } = 0;
        public string Server { get; set; } = "";
        public int Port { get; set; } = 0;
        public string DatabaseName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
