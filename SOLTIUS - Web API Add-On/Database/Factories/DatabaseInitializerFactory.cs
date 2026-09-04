using SOLTIUS_Web_API_Add_On.Database.Initializers;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;

namespace SOLTIUS_Web_API_Add_On.Database.Factories
{
    public class DatabaseInitializerFactory: IDatabaseInitializerFactory
    {
        private readonly MySqlDatabaseInitializer _mySql;
        private readonly SqlServerDatabaseInitializer _sqlServer;

        public DatabaseInitializerFactory(MySqlDatabaseInitializer mySql, SqlServerDatabaseInitializer sqlServer)
        {
            _mySql = mySql;
            _sqlServer = sqlServer;
        }

        public IDatabaseInitializer Create(DBConfig config)
        {
            return (DatabaseType)config.DBType switch
            {
                DatabaseType.MySql => _mySql,
                DatabaseType.SqlServer => _sqlServer,
                _ => throw new NotSupportedException()
            };
        }
    }
}
