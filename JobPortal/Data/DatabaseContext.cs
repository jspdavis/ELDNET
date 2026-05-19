using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace JobPortal.Data
{
    /// <summary>
    /// Database connection factory.
    /// Provides an open IDbConnection backed by MySQL.
    /// All service classes inject this and call CreateConnection().
    /// </summary>
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(IConfiguration configuration)
        {
            // Read connection string from appsettings.json under "ConnectionStrings:DefaultConnection"
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException(
                                    "Connection string 'DefaultConnection' not found in appsettings.json.");
        }

        /// <summary>
        /// Returns a new, open MySQL connection.
        /// Use inside a 'using' block so it is properly disposed after use.
        /// </summary>
        public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
    }
}
