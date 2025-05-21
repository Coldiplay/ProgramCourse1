using Bruh.VMTools;
using MySqlConnector;

namespace Bruh.Model.DBs
{
    public class DbConnection
    {
        MySqlConnection _connection;

        static DbConnection dbConnection;
        private DbConnection() 
        {
            Config();
        }
        public static DbConnection GetDbConnection()
        {
            if (dbConnection == null)
            {
                dbConnection = new DbConnection();
            }
            return dbConnection;
        }

        public void Config()
        {
            MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
            sb.UserID = "student";
            sb.Password = "student";
            //sb.Server = "95.154.107.102";
            sb.Server = "192.168.200.13";
            sb.Database = "Bruhgalter";
            sb.CharacterSet = "utf8mb4";

            _connection = new MySqlConnection(sb.ToString());
        }

        public bool OpenConnection()
        {
            if (_connection == null)
                Config();

            if (_connection.State == System.Data.ConnectionState.Open)
                return true;
            return ExeptionHandler.Try(_connection.Open);
        }

        internal void CloseConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
                return;

            ExeptionHandler.Try(_connection.Close);
        }

        internal MySqlCommand CreateCommand(string sql)
        {
            return new MySqlCommand(sql, _connection);
        }
    }
}
