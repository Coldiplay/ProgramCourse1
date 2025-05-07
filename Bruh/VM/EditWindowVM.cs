using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.VMTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    public class EditWindowVM : BaseVM
    {
        private Action close;
        private IModel entry;
        public IModel Entry
        {
            get => entry;
            set
            {
                entry = value;
                Signal();
            }
        }

        public List<Currency> Currencies { get; set; } = new(DB.GetDb(typeof(CurrencyDB)).GetEntries("").Select(c => (Currency)c));
        public List<Bank> Banks { get; set; } = new(DB.GetDb(typeof(BanksDB)).GetEntries("").Select(b => (Bank)b));
        public List<Account> Accounts { get; set; } = new(DB.GetDb(typeof(AccountsDB)).GetEntries("").Select(a => (Account)a));
        public List<Category> Categories { get; set; } = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("").Select(c => (Category)c));
        public List<Debt> Debts { get; set; } = new(DB.GetDb(typeof(DebtsDB)).GetEntries("").Select(d => (Debt)d));
        public List<Periodicity> Periodicities { get; set; } = new(DB.GetDb(typeof(PeriodicitiesDB)).GetEntries("").Select(p => (Periodicity)p));
        public List<PeriodicityOfPayment> PeriodicitiesOfPayment { get; set; } = new(DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetEntries("").Select(c => (PeriodicityOfPayment)c));
        public List<TypeOfDeposit> TypesOfDeposits { get; set; } = new(DB.GetDb(typeof(TypesOfDepositDB)).GetEntries("").Select(t => (TypeOfDeposit)t));

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }

        public EditWindowVM()
        {

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
                if (MessageBox.Show("Хотите ли вы изменить соответствующие записи?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    change = true;
                if (Entry.ID != 0)
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Update(Entry, change);
                else
                    DB.GetDb(Entry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Insert(Entry, change);
                close();
            },
            
            () =>
            {
                switch (Entry)
                {
                    case Operation:
                        if (((Operation)Entry).Category != null && !string.IsNullOrEmpty(((Operation)Entry).Title) && ((Operation)Entry).Cost > 0 && ((Operation)Entry).Account != null) //&& ((Operation)Entry).Income != null)
                            return true;
                        break;

                    case Account:
                        if (!string.IsNullOrEmpty(((Account)Entry).Title) && ((Account)Entry).Currency != null)
                            return true;
                        break;

                    case Debt:
                        if (((Debt)Entry).DateOfReturn != null && ((Debt)Entry).DateOfPick < ((Debt)Entry).DateOfReturn && ((Debt)Entry).Currency != null && ((Debt)Entry).Summ > 0)
                            return true;
                        break;

                    case Deposit:
                    if (((Deposit)Entry).Currency != null && ((Deposit)Entry).Type != null && ((Deposit)Entry).Bank != null && ((Deposit)Entry).CloseDate > ((Deposit)Entry).OpenDate && ((Deposit)Entry).PeriodicityOfPayment != null && !string.IsNullOrEmpty(((Deposit)Entry).Title) && ((Deposit)Entry).InitalSumm > 0 && ((Deposit)Entry).InterestRate > 0)
                            return true;
                        break;

                    case Category:
                        if (!string.IsNullOrEmpty(((Category)Entry).Title))
                            return true;
                        break;

                    case Bank:
                        if (!string.IsNullOrEmpty(((Bank)Entry).Title))
                            return true;
                        break;

                    /*
                    case Periodicity:
                        if (!string.IsNullOrEmpty(((Periodicity)Entry).Name))
                            return true;
                        break;
                    */

                    case Currency:
                        if (!string.IsNullOrEmpty(((Currency)Entry).Title) && !char.IsWhiteSpace(((Currency)Entry).Symbol))
                            return true;
                        break;

                    default:
                        return false;
                }
                return false;
            });
            Cancel = new CommandVM(() => close(), () => true);
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
                case Account:
                    ((Account)Entry).Currency = Currencies.First(c => c.ID == ((Account)Entry).CurrencyID);
                    ((Account)Entry).Bank = Banks.First(b => b.ID == ((Account)Entry).CurrencyID);
                    break;

                case Operation:
                    ((Operation)Entry).Account = Accounts.First(a => a.ID == ((Operation)Entry).AccountID);
                    ((Operation)Entry).Category = Categories.First(c => c.ID == ((Operation)Entry).CategoryID);
                    ((Operation)Entry).Debt = Debts.FirstOrDefault(d => d.ID == ((Operation)Entry).DebtID);
                    ((Operation)Entry).Periodicity = Periodicities.FirstOrDefault(p => p.ID == ((Operation)Entry).PeriodicityID);
                    break;

                case Debt:
                    ((Debt)Entry).Currency = Currencies.First(c => c.ID == ((Debt)Entry).CurrencyID);
                    break;

                case Deposit:
                    ((Deposit)Entry).Bank = Banks.First(b => b.ID == ((Deposit)Entry).BankID);
                    ((Deposit)Entry).PeriodicityOfPayment = PeriodicitiesOfPayment.First(p => p.ID == ((Deposit)Entry).PeriodicityOfPaymentID);
                    ((Deposit)Entry).Type = TypesOfDeposits.First(t => t.ID == ((Deposit)Entry).TypeOfDepositID);
                    ((Deposit)Entry).Currency = Currencies.First(c => c.ID == ((Deposit)Entry).CurrencyID);
                    break;

                //default:
                //    break;
            }
        }
    }
}
