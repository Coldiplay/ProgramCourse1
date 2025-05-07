using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Bruh.Model.Models
{
    [DBContext(typeof(DebtsDB))]
    public class Debt : IModel
    {
        public int ID { get; set; }
        public decimal Summ { get; set; }
        public short AnnualInterest { get; set; }
        public int CurrencyID { get; set; }
        public string? Title { get; set; }
        public DateTime DateOfPick { get; set; }
        public DateTime DateOfReturn { get; set; }
        public Currency Currency { get; set; }

        //
        public decimal PaidSumm { get; set; }
        //

        public string GetApproximateMonthlyPayment
        {
            get
            {
                decimal approximateMonthlyPayment;
                //if (ID < 0)
                //{
                    if (DateOfReturn < DateOfPick)
                        return "Дата взятия долга назачена позже, чем дата выплаты";
                    decimal rate = (decimal)AnnualInterest / 12 / 100;
                decimal help = (decimal)(Math.Pow(1 + (double)rate, GetMonths) - 1);
                rate += rate / help;
                approximateMonthlyPayment = Summ * rate;
                    return $"{Math.Round(approximateMonthlyPayment, MidpointRounding.ToEven)} {Currency.Symbol}";
                //}

                //return "Ошибка";
            }
        }
        public string GetApproximateFullSumm => $"{Convert.ToDecimal(GetApproximateMonthlyPayment.Remove(GetApproximateMonthlyPayment.Length - 1))*GetMonths} {Currency.Symbol}";
        public string GetSumm => $"{Summ} {Currency.Symbol}";
        public string GetPaidSumm => $"{PaidSumm} {Currency.Symbol}";

        private int GetMonths => ((DateOfReturn.Year - DateOfPick.Year) * 12) + (DateOfReturn.Month - DateOfPick.Month) - (DateOfReturn.Day < DateOfPick.Day ? 1 : 0);
    }
}
