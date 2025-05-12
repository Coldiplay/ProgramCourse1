using MySqlConnector;
using System.Data.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            DateTime now = DateTime.Today;
            //DateTime date = DateTime.Parse("30.12.2024");
            DateTime date = DateTime.Today.AddYears(5);

            //int months = ((date.AddDays(1).Year - now.AddDays(1).Year) * 12) + (date.AddDays(1).Month - now.AddDays(1).Month) - (date.AddDays(1).Day < now.AddDays(1).Day ? 1 : 0);

            decimal summ = 100000;
            int months = ((date.Year - now.Year) * 12) + (date.Month - now.Month) - (date.Day < now.Day ? 1 : 0);
            int days = (date - now).Days;
            //DateTime time = date;
            
            double Rate = (double)(1 + (5.0 / 100 / (DateTime.IsLeapYear(now.Year) ? 366 : 365)));
            decimal result = (decimal)(summ*Math.Pow(Rate, days));
            
            //months = 60;

            //decimal Rate = (1 + ((decimal)12 / 100));
            decimal Rate = 5;
            Rate /= 100;
            Rate /= 12;

            //Rate = Math.Pow(Rate, 5);
            decimal result = summ * (Rate + Rate / (decimal)(Math.Pow(1 + (double)Rate, months) - 1));
            //result -= (decimal)0.2;
            result = Math.Round(result, MidpointRounding.ToEven);
            //decimal help = summ * (months);




            Console.WriteLine(result);
            Console.WriteLine(result*months);
            //Console.WriteLine(result*months+result*3);
            */
            MySqlConnection _connection;
            MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
            sb.UserID = "student";
            sb.Password = "student";
            sb.Server = "95.154.107.102";
            //sb.Server = "192.168.200.13";
            sb.Database = "Bruhgalter";
            sb.CharacterSet = "utf8mb4";

            _connection = new MySqlConnection(sb.ToString());
            _connection.Open();
            Console.WriteLine(_connection.Ping());
            _connection.Close();

        }
    }
}
