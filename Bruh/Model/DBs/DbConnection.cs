using Bruh.VMTools;
using MySqlConnector;

namespace Bruh.Model.DBs
{
    internal class DbConnection
    {
        MySqlConnection _connection;

        static DbConnection dbConnection;
        private DbConnection() 
        {
            Config();
        }
        internal static DbConnection GetDbConnection()
        {
            if (dbConnection == null)
            {
                dbConnection = new DbConnection();
            }
            return dbConnection;
        }

        internal void Config()
        {
            MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
            sb.UserID = "student";
            sb.Password = "student";
            sb.Server = "192.168.200.13";
            //sb.Server = "95.154.107.102";
            sb.Database = "Bruhgalter";
            sb.CharacterSet = "utf8mb4";
            sb.ConnectionTimeout = 2;

            _connection = new MySqlConnection(sb.ToString());

            while (true)
            {
                try
                {
                    _connection.Open();
                    break;
                }
                catch (MySqlException)
                {
                    sb.Server = "95.154.107.102";
                    _connection = new MySqlConnection(sb.ToString());
                }
            }
            _connection.Close();

        }

        internal bool OpenConnection()
        {
            if (_connection == null)
                Config();

            if (_connection?.State == System.Data.ConnectionState.Open)
                return true;
            return ExceptionHandler.Try(_connection.Open);
        }

        internal void CloseConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
                return;

            ExceptionHandler.Try(_connection.Close);
        }

        internal MySqlCommand CreateCommand(string sql)
        {
            return new MySqlCommand(sql, _connection);
        }
    }
}
