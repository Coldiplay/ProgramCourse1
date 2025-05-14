using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.VMTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bruh.VM
{
    public class EditWindowVM : BaseVM
    {
        private Action? close;
        private IModel entry;
        private string durationType;

        public IModel Entry
        {
            get => entry;
            set
            {
                entry = value;
                Signal();
                if (Entry is Deposit deposit)
                    SetDurationType(deposit);
                //else if (Entry is Debt debt)
                //  SetDurationType(debt);
            }
        }
        public string DurationType
        {
            get => durationType;
            set
            {
                durationType = value;
                Signal();
                //ProbableSumm = ((Deposit)Entry).GetProbSumm;
            }
        }

        public List<Currency> Currencies { get; set; } = new(DB.GetDb(typeof(CurrencyDB)).GetEntries("", []).Select(c => (Currency)c));
        public List<Bank> Banks { get; set; } = new(DB.GetDb(typeof(BanksDB)).GetEntries("", []).Select(b => (Bank)b));
        public List<Account> Accounts { get; set; } = new(DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a));
        public List<Category> Categories { get; set; } = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c));
        public List<Debt> Debts { get; set; } = new(DB.GetDb(typeof(DebtsDB)).GetEntries("", []).Select(d => (Debt)d));
        public List<Periodicity> Periodicities { get; set; } = new(DB.GetDb(typeof(PeriodicitiesDB)).GetEntries("", []).Select(p => (Periodicity)p));
        public List<PeriodicityOfPayment> PeriodicitiesOfPayment { get; set; } = new(DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetEntries("", []).Select(c => (PeriodicityOfPayment)c));
        public List<TypeOfDeposit> TypesOfDeposits { get; set; } = new(DB.GetDb(typeof(TypesOfDepositDB)).GetEntries("", []).Select(t => (TypeOfDeposit)t));

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand ChangeDurationType { get; set; }

        public EditWindowVM()
        {
            durationType = "Месяцев";
            Signal(nameof(DurationType));
            Save = new CommandVM(() =>
            {
                /*
                Type type = Entry.GetType();
                var test = type.GetCustomAttribute<DBContextAttribute>();
                var db = DB.GetDb(test.Type);
                if (((Operation)Entry).ID != 0)
                    db.Update(Entry);
                else
                    db.Insert(Entry);
                */
                bool change = false;
                if (Entry is Operation || Entry is Debt || Entry is Deposit || Entry is Operation)
                { 
                    if (MessageBox.Show("Хотите ли вы изменить соответствующие записи?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        change = true;
                }
                if (Entry.ID != 0)
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Update(Entry, change);
                else
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Insert(Entry, change);
                close?.Invoke();
            },
            
            () =>
            {
                switch (Entry)
                {
                    case Operation operation:
                        if (operation.Category != null && !string.IsNullOrEmpty(operation.Title) && operation.Cost > 0 && operation.Account != null) //&& operation.Income != null)
                            return true;
                        break;

                    case Account account:
                        if (!string.IsNullOrEmpty(account.Title) && account.Currency != null)
                            return true;
                        break;

                    case Debt debt:
                        if (/*(debt.DateOfReturn != null &&*/ debt.DateOfPick < debt.DateOfReturn && debt.Currency != null && debt.Summ > 0)
                            return true;
                        break;

                    case Deposit deposit:
                    if (deposit.Currency != null && deposit.Type != null && deposit.Bank != null && deposit.CloseDate > deposit.OpenDate && deposit.PeriodicityOfPayment != null && !string.IsNullOrEmpty(deposit.Title) && deposit.InitalSumm > 0 && deposit.InterestRate > 0)
                            return true;
                        break;

                    case Category category:
                        if (!string.IsNullOrEmpty(category.Title))
                            return true;
                        break;

                    case Bank bank:
                        if (!string.IsNullOrEmpty(bank.Title))
                            return true;
                        break;

                    // Зачем? я же не собирался добавлять, вроде.....
                    /*
                    case Periodicity:
                        if (!string.IsNullOrEmpty(((Periodicity)Entry).Name))
                            return true;
                        break;
                    */

                    case Currency currency:
                        if (!string.IsNullOrEmpty(currency.Title) && !char.IsWhiteSpace(currency.Symbol))
                            return true;
                        break;

                        /*
                    default:
                        return false;
                        */
                }
                return false;
            });
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
                    SetDurationType(deposit);
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
                    SetDurationType(debt);
                }
            }, () => true);

            if (Entry is Deposit)
                Banks.Add(new Bank { ID = 0});
        }
        private void SetDurationType(Deposit deposit)
        {
            DurationType = deposit.Code switch
            {
                0 => "Дни",
                1 => "Месяцы",
                2 => "Года",
                _ => throw new NotImplementedException()
                };
            //debt.Duration = debt.Duration;
        }
        private void SetDurationType(Debt debt)
        {
            DurationType = debt.Code switch
            {
                0 => "Дни",
                1 => "Месяцы",
                2 => "Года",
                _ => throw new NotImplementedException()
            };
            //debt.Duration = debt.Duration;
        }
        public void Set(IModel obj, Action close)
        {
            Entry = obj;
            this.close = close;
            switch (Entry)
            {
                /*
                 * Currencies
                 * Banks
                 * Accounts
                 * Categories
                 * Debts
                 * Periodicities
                 * PerOfPayment
                 * TypeOfDeposit
                 */
                case Account account:
                    if (account.ID == 0)
                        break;
                    account.Currency = Currencies.First(c => c.ID == account.CurrencyID);
                    account.Bank = Banks.First(b => b.ID == account.CurrencyID);
                    break;

                case Operation operation:
                    if (operation.ID == 0)
                        break;
                    operation.Account = Accounts.First(a => a.ID == operation.AccountID);
                    operation.Category = Categories.First(c => c.ID == operation.CategoryID);
                    operation.Debt = Debts.FirstOrDefault(d => d.ID == operation.DebtID);
                    operation.Periodicity = Periodicities.FirstOrDefault(p => p.ID == operation.PeriodicityID);
                    break;

                case Debt debt:
                    if (debt.ID == 0)
                        break;
                    debt.Currency = Currencies.First(c => c.ID == debt.CurrencyID);
                    break;

                case Deposit deposit:
                    if (deposit.ID == 0)
                        break;
                    deposit.Bank = Banks.First(b => b.ID == deposit.BankID);
                    deposit.PeriodicityOfPayment = PeriodicitiesOfPayment.First(p => p.ID == deposit.PeriodicityOfPaymentID);
                    deposit.Type = TypesOfDeposits.First(t => t.ID == deposit.TypeOfDepositID);
                    deposit.Currency = Currencies.First(c => c.ID == deposit.CurrencyID);
                    break;

                //default:
                //    break;
            }
        }
    }
}
