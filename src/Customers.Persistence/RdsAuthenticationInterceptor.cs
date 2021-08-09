using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS.Util;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace Customers.Persistence
{
    public class RdsAuthenticationInterceptor : DbConnectionInterceptor
    {
        private static readonly TimeSpan RefreshBeforeExpireSecs = TimeSpan.FromSeconds(-60);
        private static readonly string TokenCachKey = "RdsAuthToken";
        private readonly IMemoryCache _cache;

        public RdsAuthenticationInterceptor(IMemoryCache cache)
        {
            _cache = cache;
        }

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

            var token = _cache.Get<string>(TokenCachKey);

            if (token == null)
            {
                token = RDSAuthTokenGenerator.GenerateAuthToken(builder.Host, builder.Port, builder.Username);
                var expiryKeyValue = token.Split("&").Single(x => x.StartsWith("X-Amz-Expires"));
                var expiry = int.Parse(expiryKeyValue.Split("=")[1]);
                token = _cache.Set(TokenCachKey, token,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(expiry)
                        .Add(RefreshBeforeExpireSecs)));
            }

            builder.Password = token;

            conn.ConnectionString = builder.ConnectionString;

            return result;
        }
    }
}