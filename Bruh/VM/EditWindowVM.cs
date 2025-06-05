using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.VMTools;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    internal class EditWindowVM : BaseVM
    {
        private Action? close;
        private IModel? entry;
        private string durationType = "";
        private byte hours;
        private byte minutes;
        private bool changeCorrespondingEntries = true;
        private Visibility forIncome;

        public IModel? Entry
        {
            get => entry;
            private set
            {
                entry = value;
                Signal();
            }
        }
        public bool Income
        {
            get => (Entry as Operation).Income;
            set
            {
                ((Operation?)Entry).Income = value;
                ForIncome = ((Operation)Entry).Income ? Visibility.Visible : Visibility.Collapsed;
                Signal();
            }
        }
        public bool Expense 
        {
            get => !Income;
            set
            {
                Income = !value;
                Signal();
            }
        }
        public bool ChangeCorrespondingEntries
        {
            get => changeCorrespondingEntries;
            set
            {
                changeCorrespondingEntries = value;
                Signal();
            }
        }
        public string DurationType
        {
            get => durationType;
            private set
            {
                durationType = value;
                Signal();
            }
        }
        public string Hours
        {
            get => hours < 10 ? $"0{hours}" : hours.ToString();
            set
            {
                if (!byte.TryParse(value, out hours))
                    hours = 0;
                if (hours >= 24)
                    minutes = 0;
                Signal();
            }
        }
        public string Minutes
        {
            get => minutes < 10 ? $"0{minutes}" : minutes.ToString();
            set
            {
                if (!byte.TryParse(value, out minutes))
                    minutes = 0;
                if (minutes >= 60 && hours == 23)
                    minutes = 60;
                Signal();
            }
        }

        public List<Bank> Banks { get; private set; } = [.. DB.GetDb(typeof(BanksDB)).GetEntries("", []).Select(b => (Bank)b)];
        public List<Account> Accounts { get; private set; } = [.. DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a)];
        public List<Category> Categories { get; private set; } = [.. DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c)];
        public List<Debt> Debts { get; private set; } = [.. DB.GetDb(typeof(DebtsDB)).GetEntries("", []).Select(d => (Debt)d)];
        public List<PeriodicityOfPayment> PeriodicitiesOfPayment { get; private set; } = [.. DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetEntries("", []).Select(c => (PeriodicityOfPayment)c)];

        public ICommand Save { get; private set; }
        public ICommand Cancel { get; private set; }
        public ICommand ChangeDurationType { get; private set; }

        public Visibility ForIncome
        {
            get => forIncome;
            private set
            {
                forIncome = value;
                Signal();
            }
        }

        public EditWindowVM()
        {
            Save = new CommandVM(() =>
            {
                if (Entry is Operation operation)
                {
                    operation.TransactDate = operation.TransactDate.AddMinutes(byte.Parse(Minutes) - operation.TransactDate.Minute);
                    operation.TransactDate = operation.TransactDate.AddHours(byte.Parse(Hours) - operation.TransactDate.Hour);

                    try
                    {
                        if (operation.Debt != null && operation.Cost > decimal.Parse(operation.Debt.GetApproximateMonthlyPayment[..^1]))
                        {
                            string message = "Сумма операции больше, чем месячный платёж по долгу, вы хотите продолжить?";
                            if (operation.Cost > decimal.Parse(operation.Debt.GetApproximateFullSumm[..^1]) - operation.Debt.PaidSumm)
                                message = "Сумма операции больше, чем нужная сумма для покрытия долга, вы точно хотите продолжить?";

                            if (MessageBox.Show(message, "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return;
                        }
                    }
                    catch (Exception)
                    { }
                }

                if (Entry?.ID != 0)
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Update(Entry, ChangeCorrespondingEntries);
                else
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Insert(Entry, ChangeCorrespondingEntries);

                close?.Invoke();
            },
            () => Entry?.AllFieldsAreCorrect ?? false);
            Cancel = new CommandVM(() => close?.Invoke(), () => true);
            ChangeDurationType = new CommandVM(() =>
            {
                if (Entry is Deposit deposit)
                {
                    deposit.Code = deposit.Code switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 0,
                        _ => throw new NotImplementedException()
                    };
                    SetDurationType(deposit.Code);
                }
                else if (Entry is Debt debt)
                {
                    debt.Code = debt.Code switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 0,
                        _ => throw new NotImplementedException()
                    };
                    SetDurationType(debt.Code);
                }
            }, () => true);
            
        }
        private void SetDurationType(byte i)
        {
            DurationType = i switch
            {
                0 => "Дни",
                1 => "Месяцы",
                2 => "Года",
                _ => throw new NotImplementedException()
                };
        }
        internal void Set(IModel? obj, Action close)
        {
            Entry = obj;
            this.close = close;
            switch (Entry)
            {
                case Account account:
                    Banks.Insert(0 ,new Bank { ID = 0, Title = "Без привязки к банку" });
                    if (account.ID == 0)
                        break;
                    break;

                case Operation operation:
                    Debts.Insert(0, new Debt { ID = 0, Title = "Нет" });
                    Minutes = ((byte)operation.TransactDate.Minute).ToString();
                    Hours = ((byte)operation.TransactDate.Hour).ToString();
                    ForIncome = operation.Income ? Visibility.Visible : Visibility.Collapsed;
                    if (operation.ID == 0)
                        break;
                    operation.Account = Accounts.First(a => a.ID == operation.AccountID);
                    operation.Category = Categories.First(c => c.ID == operation.CategoryID);
                    operation.Debt = Debts.FirstOrDefault(d => d.ID == operation.DebtID);
                    break;

                case Debt debt:
                    SetDurationType(debt.Code);
                    if (debt.ID == 0)
                        break;
                    break;

                case Deposit deposit:
                    SetDurationType(deposit.Code);
                    if (deposit.ID == 0)
                        break;
                    deposit.Bank = Banks.First(b => b.ID == deposit.BankID);
                    deposit.PeriodicityOfPayment = PeriodicitiesOfPayment.First(p => p.ID == deposit.PeriodicityOfPaymentID);
                    break;

                case null:
                    MessageBox.Show("Как так-то");
                    close();
                    break;

            }
        }
    }
}
