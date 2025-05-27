using Microsoft.Data.SqlClient;

namespace SecurityAgencyApp
{
    public static class Database
    {
        public static string ConnectionString = "Server=172.17.6.18;Encrypt=False;Database=Derezhintseva;User ID=ip22;Password=ip22_1";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}