using Npgsql;

namespace AtlasAddressBook.Helpers
{
    public static class ConnectionHelper
    {
        //Every method in a static class must be static as well
        public static string? GetConnectionString(IConfiguration configuration)
        {
            //Local environment
            var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            //Identifies the deployment environment
            //Environment variable changes according to host
            //may need to change when deploying to RailWay
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");//Heroku Environment variable name

            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }
        //building connection string from the heroku environment
        private static string BuildConnectionString(string databaseUrl)
        {
            //postgres specific
            //converts URL to URI; URL locates a resource, URI identifies a resource
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,//may need to change when deploying to RailWay
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                //encrypts the data
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
    }
}
