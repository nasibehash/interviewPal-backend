using Npgsql;

namespace InterviewPal.Infrastructure.Persistence;

public static class PostgresConnection
{
    /// <summary>
    /// Hosted databases hand out a URL (postgres://user:password@host:5432/db). Npgsql wants key=value pairs,
    /// so a URL is converted; a connection string that already is key=value is returned unchanged.
    /// </summary>
    public static string Normalize(string connection)
    {
        if (!connection.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connection.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return connection;

        var uri = new Uri(connection);
        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port < 0 ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
            SslMode = SslMode.Require
        };

        // ?sslmode=disable and similar options from the URL
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length != 2) continue;
            var key = Uri.UnescapeDataString(kv[0]);
            // Neon adds channel_binding=require; Npgsql spells that keyword "Channel Binding"
            if (key.Equals("channel_binding", StringComparison.OrdinalIgnoreCase)) key = "Channel Binding";
            builder[key] = Uri.UnescapeDataString(kv[1]);
        }

        return builder.ConnectionString;
    }
}
