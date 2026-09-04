using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;
using System.Data.Common;

namespace SOLTIUS_Web_API_Add_On.Database.Factories
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        public DbConnection CreateConnection(DBConfig config)
        {
            switch ((DatabaseType)config.DBType)
            {
                case DatabaseType.MySql:

                    if (config.Port == 0)
                    {
                        var mysql = new MySqlConnectionStringBuilder
                        {
                            Server = config.Server,
                            Database = config.DatabaseName,
                            UserID = config.UserName,
                            Password = config.Password
                        };

                        return new MySqlConnection(mysql.ConnectionString);
                    }
                    else
                    {
                        var mysql = new MySqlConnectionStringBuilder
                        {
                            Server = config.Server,
                            Port = (uint)config.Port,
                            Database = config.DatabaseName,
                            UserID = config.UserName,
                            Password = config.Password
                        };

                        return new MySqlConnection(mysql.ConnectionString);
                    }

                case DatabaseType.SqlServer:

                    if (config.Port == 0)
                    {
                        var sql = new SqlConnectionStringBuilder
                        {
                            DataSource = $"{config.Server}",
                            InitialCatalog = config.DatabaseName,
                            UserID = config.UserName,
                            Password = config.Password,
                            TrustServerCertificate = true
                        };

                        return new SqlConnection(sql.ConnectionString);
                    }
                    else
                    {
                        var sql = new SqlConnectionStringBuilder
                        {
                            DataSource = $"{config.Server},{config.Port}",
                            InitialCatalog = config.DatabaseName,
                            UserID = config.UserName,
                            Password = config.Password,
                            TrustServerCertificate = true
                        };

                        return new SqlConnection(sql.ConnectionString);
                    }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
