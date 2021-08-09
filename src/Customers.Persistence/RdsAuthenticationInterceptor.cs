using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS.Util;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Customers.Persistence
{
    public class RdsAuthenticationInterceptor : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(ConnectionOpening(connection, eventData, result));
        }

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            if (connection is not NpgsqlConnection conn) return result;

            var builder = new NpgsqlConnectionStringBuilder(conn.ConnectionString);

            builder.Password = RDSAuthTokenGenerator.GenerateAuthToken(builder.Host, builder.Port, builder.Username);

            conn.ConnectionString = builder.ConnectionString;

            return result;
        }
    }
}