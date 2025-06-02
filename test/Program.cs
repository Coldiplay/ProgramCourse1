using MySqlConnector;
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

            /*
            while (true) {
                decimal balance = 500;
                decimal summ = 300;
                decimal.TryParse(Console.ReadLine(), out decimal summ2);
                decimal difference = summ - summ2;
                decimal summForChangeAcc1 = (decimal)balance + (summ - difference);
                Console.WriteLine($"\n{summForChangeAcc1}\n");
                }
            */
            /*
            while (true)
            {
                Console.WriteLine("Старое");
                decimal.TryParse(Console.ReadLine(), out decimal oldNum);
                Console.WriteLine("Новое");
                decimal.TryParse(Console.ReadLine(), out decimal newNum);
                Console.WriteLine("Текущее Доход/Расход");
                bool newB = Console.ReadLine() == "y";
                Console.WriteLine("Бывшее Доход/Расход");
                bool oldB = Console.ReadLine() == "y";
                bool check = newB == oldB;
                ChangeSummOnAccount(oldB, oldNum, check ? newNum : -newNum);
            }
            */
            while (true)
            {
                Console.WriteLine("Текущее Доход/Расход");
                bool isInc = Console.ReadLine() == "y";
                Console.WriteLine("Бывшее Доход/Расход");
                bool prevInc = Console.ReadLine() == "y";
                Console.WriteLine("Старое");
                decimal.TryParse(Console.ReadLine(), out decimal prevSum);
                Console.WriteLine("Новое");
                decimal.TryParse(Console.ReadLine(), out decimal curSum);
                ChangeSummOnAccount(isInc, prevInc, prevSum, isInc == prevInc ? curSum : curSum+prevSum, curSum);
            }

        }
        private static bool ChangeSummOnAccount(bool isIncome, decimal prevSumm, decimal curSumm)
        {
            decimal? balance = 0;
            decimal? balance2 = 0;

            balance = 1000;
            balance2 = 2000;
            decimal summForChangeAcc1;
            decimal summForChangeAcc2;

            if (isIncome)
            {
                summForChangeAcc1 = -prevSumm;
                summForChangeAcc2 = curSumm;
                if (balance - summForChangeAcc1 < 0)
                {
                    Console.WriteLine("fdg");
                    return false;
                }
            }
            else
            {
                summForChangeAcc1 = prevSumm;
                summForChangeAcc2 = -curSumm;
                if (balance2 - summForChangeAcc2 < 0)
                {
                    Console.WriteLine("das");
                    return false;
                }
            }

            Console.WriteLine($"Баланс первого акк {balance + summForChangeAcc1} ({balance} | {summForChangeAcc1})");
            Console.WriteLine($"Баланс 2 акк {balance2 + summForChangeAcc2} ({balance2} | {summForChangeAcc2})");

            return true;
        }
        private static void ChangeSummOnAccount(bool isIncome, bool prevIncome , decimal prevSumm, decimal curSumm, decimal prevcurSumm)
        {
            decimal balance = 1000;
            decimal changedBalance = isIncome ? balance + (curSumm - prevSumm) : balance - (curSumm - prevSumm);
            Console.WriteLine($"\nБыло:" +
                $" {(prevIncome ? "Доход" : "Расход")} {prevSumm}" +
                $"\nСтало:" +
                $" {(isIncome ? "Доход" : "расход")} {prevcurSumm}" +
                $"\n Баланс был {balance}  ({(prevIncome ? balance-prevSumm : balance+prevSumm)})" +
                $"\n Баланс стал {changedBalance} ({(isIncome ? balance - curSumm : balance + curSumm)})\n");
        }
    }
}
