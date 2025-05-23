using MySqlConnector;
using System.Data.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace test
{
    internal class Program
    {
        //enum Sides
        //{
        //    Left, Right, Top, Bottom
        //}
        static void Main(string[] args)
        {
            /*
            decimal a = 1;
            decimal help = 0;
            while ((a / (decimal)3) % 10 != 0)
            {
                help = a % 3;
                Console.WriteLine(help);
                Console.WriteLine(help % 3);
                a++;
            }
            */
            //DateTime date = new(2024, 10, 23);
            //Console.WriteLine(date.AddMonths(6));
            Console.WriteLine(string.Format("{0:F2}", (decimal)1));
        }
    }
}
