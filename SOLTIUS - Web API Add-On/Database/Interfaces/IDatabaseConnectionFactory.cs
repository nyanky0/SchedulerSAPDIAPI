using SOLTIUS_Web_API_Add_On.Models.Configuration;
using System.Data.Common;

namespace SOLTIUS_Web_API_Add_On.Database.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        DbConnection CreateConnection(DBConfig config);
    }
}
