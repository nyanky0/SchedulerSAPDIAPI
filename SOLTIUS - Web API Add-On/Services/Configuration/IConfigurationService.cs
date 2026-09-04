using SOLTIUS_Web_API_Add_On.Models.Configuration;

namespace SOLTIUS_Web_API_Add_On.Services.Configuration
{
    public interface IConfigurationService
    {
        bool ConfigExists();
        DBConfig GetDatabaseConfig();
        Task ConfigureAsync(string xml);
    }
}
