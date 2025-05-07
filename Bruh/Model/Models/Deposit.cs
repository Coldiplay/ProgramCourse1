using Bruh.Model.DBs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bruh.Model.Models
{
    [DBContext(typeof(DepositsDB))]
    public class Deposit : IModel
    {

        public int ID { get; set; }
        public string Title { get; set; }
        public decimal InitalSumm { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }
        public bool Capitalization { get; set; }
        public decimal InterestRate { get; set; }
        public int PeriodicityOfPaymentID { get; set; }
        public int BankID { get; set; }
        public int TypeOfDepositID { get; set; }
        public int CurrencyID { get; set; }

        //
        public Bank Bank { get; set; }
        public TypeOfDeposit Type { get; set; }
        public Currency Currency { get; set; }
        public PeriodicityOfPayment PeriodicityOfPayment { get; set; }
        //
        private int duration;
        private int code;
        public int Code
        {
            get => code;
            set
            {
                code = value;
                Duration = code switch
                {
                    0 => DateTime.IsLeapYear(CloseDate.Year) ? Duration * 366 : Duration * 365,
                    1 => Duration / 30,
                    2 => Duration / 12
                };
            }
        }
        public int Duration 
        { 
            get => duration;
            set
            {
                duration = value;
                ChangeCloseDate(duration, Code);
            }
        }
        //

        // Придумай сам
        public string GetCurrentSumm => GetProbableSumm(OpenDate, DateTime.Today, Capitalization);
        public void ChangeCloseDate(int duration, int code)
        {
            CloseDate = code switch
            {
                0 => OpenDate.AddDays(duration),
                1 => OpenDate.AddMonths(duration),
                2 => OpenDate.AddYears(duration),
                _ => throw new NotImplementedException()
            };
        }
        public void SetCodeForChangeInPeriodicity()
        {
            DateTime help = CloseDate;
            switch (PeriodicityOfPayment.Name)
            {
                case "Каждый день":
                    Code = 0;
                    Duration = (help - OpenDate).Days;
                    CloseDate = help;
                    break;

                case "Каждый квартал":
                case "Каждые полгода":
                case "Каждый месяц":
                    Code = 1;
                    Duration = ((help.Year - OpenDate.Year) * 12) + (help.Month - OpenDate.Month) - (help.Day < OpenDate.Day ? 1 : 0);
                    CloseDate = help;
                    break;

                case "Каждый год":
                    Code = 2;
                    Duration = (((help.Year - OpenDate.Year) * 12) + (help.Month - OpenDate.Month) - (help.Day < OpenDate.Day ? 1 : 0)) / 12;
                    CloseDate = help;
                    break;
            }
        }
        public string GetProbableSumm(DateTime startTime, DateTime endTime, bool capitilize)        
        {
            decimal initSumm = InitalSumm;
            int monthsPassed = ((endTime.Year - startTime.Year) * 12) + (endTime.Month - startTime.Month) - (endTime.Day < startTime.Day ? 1 : 0);
            int days;
            double intRate;
            decimal result = 0;

            if (capitilize)
            {
                switch (PeriodicityOfPayment.Name)
                {
                    case "Каждый день":
                        days = (endTime - startTime).Days;
                        intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                        result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                        initSumm = result;
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждый месяц":
                        //int monthsPassed = ((endTime.Year - startTime.Year) * 12) + (endTime.Month - startTime.Month) - (endTime.Day < startTime.Day ? 1 : 0);
                        for (int i = 0; i < monthsPassed; i++)
                        {
                            intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                            days = DateTime.DaysInMonth(startTime.Year, startTime.Month);
                            result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                            initSumm = result;
                            startTime = startTime.AddMonths(1);
                        }
                        /* Old method
                        //while (startTime < DateTime.Now)
                        //{
                        //    if (startTime.Month == DateTime.Now.Month && startTime.Year == DateTime.Now.Year && startTime.Day < DateTime.Now.Day)
                        //        break;

                        //    if (startTime.Year < DateTime.Now.Year)
                        //        days = DateTime.DaysInMonth(startTime.Year, startTime.Month);
                        //    else 
                        //        days = startTime.Month < DateTime.Now.Month ? DateTime.DaysInMonth(startTime.Year, startTime.Month) : DateTime.Now.Day - startTime.Day;

                        //    intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                        //    result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                        //    initSumm = result;
                        //    startTime = startTime.AddDays(DateTime.DaysInMonth(startTime.Year, startTime.Month));
                        }*/
                        return Math.Round(initSumm, 2).ToString();

                    // Completed
                    case "Каждый квартал":
                        int quartersPassed = monthsPassed / 3;

                        for (int i = 0; i < quartersPassed; i++)
                        {
                            intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                            days = (startTime.AddMonths(3) - startTime).Days;
                            result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                            initSumm = result;
                            startTime = startTime.AddMonths(3);
                        }
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждые полгода":
                        int halfYearsPassed = monthsPassed / 6;
                        for (int i = 0; i < halfYearsPassed; i++)
                        {
                            intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                            days = (startTime.AddMonths(6) - startTime).Days;
                            result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                            initSumm = result;
                            startTime = startTime.AddMonths(6);
                        }
                        return Math.Round(initSumm, 2).ToString();
                    //return $"{Math.Round(InitalSumm * (decimal)Math.Pow((double)(1 + (InterestRate / 100 / 6)), Math.Round((DateTime.Now - OpenDate).TotalDays / 31 / 6, MidpointRounding.ToZero)), 2)} {Currency.Symbol}";
                    case "Каждый год":
                        int yearsPassed = monthsPassed / 12;
                        for (int i = 0; i < yearsPassed; i++)
                        {
                            days = (startTime.AddYears(1) - startTime).Days;
                            result = InterestRate * initSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100 + initSumm;
                            initSumm = result;
                            startTime = startTime.AddYears(1);
                        }
                        return Math.Round(initSumm, 2).ToString();

                    default:
                        return "Ошибка";
                }
            }
            else
            {
                switch (PeriodicityOfPayment.Name)
                {
                    case "Каждый день":
                        days = (DateTime.Now - startTime).Days;
                        result = InterestRate * initSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100 + initSumm;
                        initSumm = result;
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждый месяц":
                        for (int i = 0; i < monthsPassed; i++)
                        {
                            days = DateTime.DaysInMonth(startTime.Year, startTime.Month);
                            initSumm = InterestRate * InitalSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100;
                            result += initSumm;
                            startTime = startTime.AddMonths(1);
                        }
                        initSumm = InitalSumm + result;                        
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждый квартал":
                        int quartersPassed = monthsPassed / 3;
                        for (int i = 0; i < quartersPassed; i++)
                        {
                            days = (startTime.AddMonths(3) - startTime).Days;
                            initSumm = InterestRate * InitalSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100;
                            result += initSumm;
                            startTime = startTime.AddMonths(3);
                        }
                        initSumm = InitalSumm + result;
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждые полгода":
                        int halfYearsPassed = monthsPassed / 6;
                        for (int i = 0; i < halfYearsPassed; i++)
                        {
                            days = (startTime.AddMonths(6) - startTime).Days;
                            initSumm = InterestRate * InitalSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100;
                            result += initSumm;
                            startTime = startTime.AddMonths(6);
                        }
                        initSumm = InitalSumm + result;
                        return Math.Round(initSumm, 2).ToString();

                    case "Каждый год":
                        int yearsPassed = monthsPassed / 12;
                        for (int i = 0; i < yearsPassed; i++)
                        {
                            days = (startTime.AddYears(1) - startTime).Days;
                            initSumm = InterestRate * InitalSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100;
                            result += initSumm;
                            startTime = startTime.AddYears(1);
                        }
                        initSumm = InitalSumm + result;
                        return Math.Round(initSumm, 2).ToString();

                    default:
                        return "Ошибка";                
                }
                /*
                days = (endTime - startTime).Days;
                result = InterestRate * initSumm * days / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365) / 100 + initSumm;
                return Math.Round(result, 2).ToString();
                */
            }
        }
    }
}
