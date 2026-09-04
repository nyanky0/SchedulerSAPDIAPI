using SOLTIUS_Web_API_Add_On.Models.Configuration;

namespace SOLTIUS_Web_API_Add_On.Database.Interfaces
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync(DBConfig config);
    }
}
