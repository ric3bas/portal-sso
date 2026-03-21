using Npgsql;
using System.Data;

namespace Portal.Infra
{
    public class DapperDatabaseProvider
    {
        private readonly Dictionary<string, string> _connections;

        public DapperDatabaseProvider(IConfiguration config)
        {
            _connections = new Dictionary<string, string>
            {
                { "SSO_POSTGRES", config.GetConnectionString("DefaultConnection") ?? string.Empty }
            };
        }

        public IDbConnection CreateConnection(string context)
        {
            var cs = _connections[context];
            return new NpgsqlConnection(cs);
        }
    }
}