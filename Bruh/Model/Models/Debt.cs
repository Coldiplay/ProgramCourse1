using Bruh.Model.DBs;
using Bruh.VMTools;
using System.Windows;

namespace Bruh.Model.Models
{
    [DBContext(typeof(DebtsDB))]
    // Наследуется от BaseVM по той же причине, что и Deposit
    public class Debt : BaseVM, IModel
    {
        private int duration;
        private byte code;
        private decimal summ;
        private short annualInterest;
        private DateTime dateOfPick = DateTime.Today;

        public int ID { get; set; }
        public decimal Summ
        {
            get => summ;
            set
            {
                summ = value;
                Signal();
                SignalAll();
            }
        }
        public short AnnualInterest
        {
            get => annualInterest;
            set
            {
                annualInterest = value;
                Signal();
                SignalAll();
            }
        }
        public string? Title { get; set; }
        public DateTime DateOfPick
        {
            get => dateOfPick;
            set
            {
                dateOfPick = value;
                Signal();
                ChangeCloseDate(Duration, Code);
                SignalAll();
            }
        }
        public DateTime DateOfReturn { get; set; } = DateTime.Today;

        //
        public decimal PaidSumm { get; set; }
        public int Duration
        {
            get => duration;
            set
            {
                duration = value;
                ChangeCloseDate(duration, Code);
                Signal();
                SignalAll();
            }
        }
        public byte Code
        {
            get => code;
            internal set
            {
                code = value;
                Duration = code switch
                {
                    0 => (DateOfPick.AddYears(Duration) - DateOfPick).Days,
                    1 => Duration / 30,
                    2 => Duration / 12,
                    _ => throw new NotImplementedException()
                };
            }
        }
        //

        public string GetApproximateMonthlyPayment
        {
            get
            {
                decimal approximateMonthlyPayment;
                try
                {
                    if (DateOfReturn < DateOfPick)
                        return "Дата взятия долга назачена позже, чем дата выплаты";
                    decimal rate = (decimal)AnnualInterest / 12 / 100;
                    decimal help = (decimal)(Math.Pow(1 + (double)rate, GetMonths) - (double)1);
                    if (help <= 0)
                        return $"0 ₽";
                    rate += rate / help;
                    approximateMonthlyPayment = Summ * rate;
                    return $"{Math.Round(approximateMonthlyPayment, 2)} ₽";
                }
                catch (OverflowException)
                {
                }
                catch (DivideByZeroException)
                {
                }
                return "Ошибка в вычислениях";
            }
        }
        public string GetApproximateFullSumm => decimal.TryParse(GetApproximateMonthlyPayment[..^1], out decimal result) ? $"{result * GetMonths} ₽" : "Ошибка в вычислениях";
        public string GetSumm => $"{Summ} ₽";
        public string GetPaidSumm 
        {
            get
            {
                if(decimal.TryParse(GetApproximateFullSumm[..^1], out decimal res) && PaidSumm == res)
                    return $"Полностью выплачен";
                return $"{PaidSumm} ₽";
            }
        }

        private int GetMonths => ((DateOfReturn.Year - DateOfPick.Year) * 12) + (DateOfReturn.Month - DateOfPick.Month) - (DateOfReturn.Day < DateOfPick.Day ? 1 : 0);

        private void SignalAll()
        {            
            Signal(nameof(GetApproximateMonthlyPayment));
            Signal(nameof(GetApproximateFullSumm));
            Signal(nameof(GetMonths));
            Signal(nameof(DateOfReturn));
        }
        internal void ChangeCloseDate(int duration, byte code)
        {
            try
            {
                DateOfReturn = code switch
                {

                    0 => DateOfPick.AddDays(duration),
                    1 => DateOfPick.AddMonths(duration),
                    2 => DateOfPick.AddYears(duration),
                    _ => throw new NotImplementedException()
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Ошибка, неверно введёный срок долга. Пожалуйста, введите реальный срок.");
                Duration = 0;
            }
        }
        internal void SetCode()
        {
            DateTime help = DateOfReturn;
            Code = 1;
            Duration = ((help.Year - DateOfPick.Year) * 12) + (help.Month - DateOfPick.Month) - (help.Day < DateOfPick.Day ? 1 : 0);
            DateOfReturn = help;
        }

        public bool AllFieldsAreCorrect => !(DateOfPick >= DateOfReturn || Summ <= 0);
    }
}
