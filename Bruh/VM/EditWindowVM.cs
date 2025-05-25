using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.VMTools;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    public class EditWindowVM : BaseVM
    {
        private Action? close;
        private IModel? entry;
        private string durationType = "";
        private byte hours;
        private byte minutes;
        private bool changeCorrespondingEntries;

        public IModel? Entry
        {
            get => entry;
            set
            {
                entry = value;
                Signal();
                //if (Entry is Deposit deposit)
                //    SetDurationType(deposit.Code);
                //else if (Entry is Debt debt)
                //    SetDurationType(debt.Code);
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
            set
            {
                durationType = value;
                Signal();
            }
        }
        public byte Hours
        {
            get => hours;
            set
            {
                hours = value;
                Signal();
            }
        }
        public byte Minutes
        {
            get => minutes;
            set
            {
                minutes = value;
                Signal();
            }
        }

        public List<Currency> Currencies { get; set; } = [.. DB.GetDb(typeof(CurrencyDB)).GetEntries("", []).Select(c => (Currency)c)];
        public List<Bank> Banks { get; set; } = [.. DB.GetDb(typeof(BanksDB)).GetEntries("", []).Select(b => (Bank)b)];
        public List<Account> Accounts { get; set; } = [.. DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a)];
        public List<Category> Categories { get; set; } = [.. DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c)];
        public List<Debt> Debts { get; set; } = [.. DB.GetDb(typeof(DebtsDB)).GetEntries("", []).Select(d => (Debt)d)];
        public List<PeriodicityOfPayment> PeriodicitiesOfPayment { get; set; } = [.. DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetEntries("", []).Select(c => (PeriodicityOfPayment)c)];

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand ChangeDurationType { get; set; }

        public EditWindowVM()
        {
            Save = new CommandVM(() =>
            {
                if (Entry is Operation operation)
                {
                    operation.TransactDate = operation.TransactDate.AddMinutes(Minutes - operation.TransactDate.Minute);
                    operation.TransactDate = operation.TransactDate.AddHours(Hours - operation.TransactDate.Hour);
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
        public void Set(IModel? obj, Action close)
        {
            Entry = obj;
            this.close = close;
            switch (Entry)
            {
                case Account account:
                    Banks.Insert(0 ,new Bank { ID = 0, Title = "Без привязки к банку" });
                    if (account.ID == 0)
                        break;
                    account.Currency = Currencies.First(c => c.ID == account.CurrencyID);
                    account.Bank = Banks.First(b => b.ID == account.CurrencyID);
                    break;

                case Operation operation:
                    Debts.Insert(0, new Debt { ID = 0, Title = "Нет" });
                    if (operation.ID == 0)
                        break;
                    operation.Account = Accounts.First(a => a.ID == operation.AccountID);
                    operation.Category = Categories.First(c => c.ID == operation.CategoryID);
                    operation.Debt = Debts.FirstOrDefault(d => d.ID == operation.DebtID);
                    Minutes = (byte)operation.TransactDate.Minute;
                    Hours = (byte)operation.TransactDate.Hour;
                    break;

                case Debt debt:
                    SetDurationType(debt.Code);
                    if (debt.ID == 0)
                        break;
                    debt.Currency = Currencies.First(c => c.ID == debt.CurrencyID);
                    break;

                case Deposit deposit:
                    SetDurationType(deposit.Code);
                    if (deposit.ID == 0)
                        break;
                    deposit.Bank = Banks.First(b => b.ID == deposit.BankID);
                    deposit.PeriodicityOfPayment = PeriodicitiesOfPayment.First(p => p.ID == deposit.PeriodicityOfPaymentID);
                    deposit.Currency = Currencies.First(c => c.ID == deposit.CurrencyID);
                    break;

                case null:
                    MessageBox.Show("Как так-то");
                    close();
                    break;

            }
        }
    }
}
