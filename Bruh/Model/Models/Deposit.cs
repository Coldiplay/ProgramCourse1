﻿using Bruh.Model.DBs;
using Bruh.VMTools;
using System.Windows;

namespace Bruh.Model.Models
{
    [DBContext(typeof(DepositsDB))]
    // Почему он наследуется от BaseVM? Потому что оконное приложение не видит изменения в свойстве Duration (и CloseDate тоже)
    public class Deposit : BaseVM, IModel
    {
        private int duration;
        private byte code;
        private DateTime closeDate = DateTime.Today;
        private bool capitalization;
        private decimal interestRate;
        private decimal initalSumm;
        private DateTime openDate = DateTime.Today;

        /**/
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal InitalSumm
        {
            get => initalSumm;
            set
            {
                initalSumm = value;
                Signal();
                Signal(nameof(GetProbSumm));
            }
        }
        public DateTime OpenDate
        {
            get => openDate;
            set
            {
                openDate = value;
                ChangeCloseDate(Duration, Code);
            }
        }
        public DateTime CloseDate
        {
            get => closeDate;
            set
            {
                closeDate = value;
                Signal();
            }
        }
        public bool Capitalization
        {
            get => capitalization;
            set
            {
                capitalization = value;
                Signal();
                Signal(nameof(GetProbSumm));
            }
        }
        public decimal InterestRate
        {
            get => interestRate;
            set
            {
                interestRate = value;
                Signal();
                Signal(nameof(GetProbSumm));
            }
        }
        public int PeriodicityOfPaymentID { get; set; }
        public int BankID { get; set; }

        //
        public Bank Bank { get; set; }
        public PeriodicityOfPayment PeriodicityOfPayment { get; set; }
        //
        public byte Code
        {
            get => code;
            internal set
            {
                code = value;
                Duration = code switch
                {
                    0 => (OpenDate.AddYears(Duration) - OpenDate).Days,
                    1 => Duration / 30,
                    2 => Duration / 12,
                    _ => throw new NotImplementedException()
                };
            }
        }
        public int Duration
        {
            get => duration;
            set
            {
                duration = value;
                ChangeCloseDate(Duration, code);
                Signal();
                Signal(nameof(GetProbSumm));
            }
        }
        //

        public string GetCurrentSumm => GetProbableSumm(OpenDate, DateTime.Today, Capitalization);
        public string GetProbSumm => GetProbableSumm(OpenDate, CloseDate, Capitalization);

        public bool AllFieldsAreCorrect => !(
            Bank == null ||
            CloseDate <= OpenDate ||
            PeriodicityOfPayment == null ||
            string.IsNullOrWhiteSpace(Title) ||
            InitalSumm <= 0 ||
            InterestRate <= 0);

        internal void ChangeCloseDate(int duration, byte code)
        {
            try 
            {
                CloseDate = code switch
                {

                    0 => OpenDate.AddDays(duration),
                    1 => OpenDate.AddMonths(duration),
                    2 => OpenDate.AddYears(duration),
                    _ => throw new NotImplementedException()
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Ошибка, неверно введёный срок вклада. Пожалуйста, введите реальный срок вклада.");
                Duration = 0;
            }
        }
        internal void SetCodeForChangeInPeriodicity()
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
        internal string GetProbableSumm(DateTime startTime, DateTime endTime, bool capitilize)        
        {
            if (PeriodicityOfPayment == null)
                return "Не все нужные поля веденны";
            try
            {
                if (endTime > CloseDate)
                    endTime = CloseDate;
                if (startTime < OpenDate)
                    startTime = OpenDate;
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
                            for (int i = 0; i < monthsPassed; i++)
                            {
                                intRate = (double)(1 + (InterestRate / 100 / (DateTime.IsLeapYear(startTime.Year) ? 366 : 365)));
                                days = DateTime.DaysInMonth(startTime.Year, startTime.Month);
                                result = (decimal)((double)initSumm * Math.Pow(intRate, days));
                                initSumm = result;
                                startTime = startTime.AddMonths(1);
                            }
                            return Math.Round(initSumm, 2).ToString();

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
                }
            }
            catch (OverflowException)
            {
                MessageBox.Show("Вы ввели слишком большое число. Пожалуйста, введите число поменьше.");
                return "";
            }
            catch (NullReferenceException)
            {
                return "Не все нужные поля выведены";
            }
        }
    }
}
