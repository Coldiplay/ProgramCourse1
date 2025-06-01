using System.Net;
using System.Xml.Linq;

namespace test
{
    public class Program
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
            /*
            WebClient client = new WebClient();
            var xml = client.DownloadString("https://www.cbr-xml-daily.ru/daily.xml");
            XDocument xdoc = XDocument.Parse(xml);
            var el = xdoc.Element("ValCurs").Elements("Valute");
            string dollar = el.Where(x => x.Attribute("ID").Value == "R01235").Select(x => x.Element("Value").Value).FirstOrDefault();
            string eur = el.Where(x => x.Attribute("ID").Value == "R01239").Select(x => x.Element("Value").Value).FirstOrDefault();
            Console.WriteLine(($"Евро: {eur} Доллар: {dollar}"));
            */

            while (true) {
                decimal balance = 500;
                decimal summ = 300;
                decimal.TryParse(Console.ReadLine(), out decimal summ2);
                decimal difference = summ - summ2;
                decimal summForChangeAcc1 = (decimal)balance + (summ - difference);
                Console.WriteLine($"\n{summForChangeAcc1}\n");
                }
        }
    }
}
