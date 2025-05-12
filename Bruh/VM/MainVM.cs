using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bruh.VM
{
    public class MainVM : BaseVM
    {
        /*
        private Operation? selectedOperation;
        private Debt? selectedDebt;
        private Deposit? selectedDeposit;
        private Account? selectedAccount;
        */
        private ObservableCollection<Operation> operations;
        private ObservableCollection<Debt> debts;
        private ObservableCollection<Deposit> deposits;
        private ObservableCollection<Account> accounts;

        private List<StackPanel> StackPanels = new();
        private string search;
        private string titleOfList = "";
        private uint codeOper;
        private IModel? selectedEntry;
        private List<string>? parametres = [];

        //private object? filter;

        public ObservableCollection<Operation> Operations
        {
            get => operations;
            set
            {
                operations = value;
                Signal();
            }
        }
        public ObservableCollection<Debt> Debts
        {
            get => debts;
            set
            {
                debts = value;
                Signal();
            }
        }
        public ObservableCollection<Deposit> Deposits
        {
            get => deposits;
            set
            {
                deposits = value;
                Signal();
            }
        }
        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set
            {
                accounts = value;
                Signal();
            }
        }

        public IModel? SelectedEntry
        {
            get => selectedEntry;
            set
            {
                selectedEntry = value;
                Signal();
            }
        }
        /*
        public Operation? SelectedOperation
        {
            get => selectedOperation;
            set
            {
                selectedOperation = value;// as Operation;
                Signal();
            }
        }

        public Debt? SelectedDebt
        {
            get => selectedDebt;
            set
            {
                selectedDebt = value;
                Signal();
            }
        }

        public Deposit? SelectedDeposit
        {
            get => selectedDeposit;
            set
            {
                selectedDeposit = value;
                Signal();
            }
        }

        public Account? SelectedAccount
        {
            get => selectedAccount;
            set
            {
                selectedAccount = value;
                Signal();
            }
        }
        */
        public string Search
        {
            get => search;
            set
            {
                search = value;
                Signal();
                UpdateLists(CodeOper);
            }
        }

        public string TitleOfList
        {
            get => titleOfList;
            set
            {
                titleOfList = value;
                Signal();
            }
        }

        private uint CodeOper
        {
            /*
             * 0 - Все операции
             * 1 - Доходы
             * 2 - Расходы
             * 3 - Долги
             * 4 - Вклады
             * 5 - Счета
             */
            get => codeOper;
            set
            {
                codeOper = value;
                Parametres = null;
                UpdateLists(codeOper);
            }
        }
        public List<string>? Parametres
        {
            get => parametres;
            set
            {
                parametres = value;
                Filter = "";
                parametres?.ForEach(p => 
                {
                    Filter = $"{Filter} {p}";
                });
                Signal();
                Signal(nameof(Filter));
            }
        }
        public string Filter { get; set; }

        public ICommand AddEntry { get; set; }
        public ICommand EditEntry { get; set; }
        public ICommand RemoveEntry { get; set; }
        public ICommand OpenCategories { get; set; }

        public ICommand SetIncomes { get; set; }
        public ICommand SetExpenses { get; set; }
        public ICommand SetOperations { get; set; }
        public ICommand SetDebts { get; set; }
        public ICommand SetDeposits { get; set; }
        public ICommand SetAccounts { get; set; }


        public MainVM()
        {
            CodeOper = 0;

            SetOperations = new CommandVM(() => CodeOper = 0, () => true);
            SetIncomes = new CommandVM(() => CodeOper = 1, () => true);
            SetExpenses = new CommandVM(() => CodeOper = 2, () => true);
            SetDebts = new CommandVM(() => CodeOper = 3, () => true);
            SetDeposits = new CommandVM(() => CodeOper = 4, () => true);
            SetAccounts = new CommandVM(() => CodeOper = 5, () => true);
            
            AddEntry = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(CodeOper switch 
                {
                    0 => new Operation(),
                    1 => new Operation {Income = true },
                    2 => new Operation(),
                    3 => new Debt(),
                    4 => new Deposit(),
                    5 => new Account(),
                    _ => new Operation()
                }, add.Close);
                add.ShowDialog();

                UpdateLists(CodeOper);
            }, () => true);
            EditEntry = new CommandVM(() =>
            {
                var edit = new EditWindow();
                
                ((EditWindowVM)edit.DataContext).Set(SelectedEntry, edit.Close);
                edit.ShowDialog();

                UpdateLists(CodeOper);

            }, () => SelectedEntry != null);
            RemoveEntry = new CommandVM(() =>
            {
                if (MessageBox.Show("Вы точно хотите удалить запись?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    bool change = MessageBox.Show("Хотите ли вы изменить соответствующие записи?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    DB.GetDb(SelectedEntry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Remove(SelectedEntry, change);
                    UpdateLists(CodeOper);
                }
            }, () => SelectedEntry != null);
            OpenCategories = new CommandVM(() => 
            {
                new CategoriesWindow().ShowDialog();
            }, () => true);
        }

        public void UpdateLists(uint i)
        {
            if (StackPanels.Count == 0)
                return;

            SelectedEntry = null;
            /*
            SelectedOperation = null;
            SelectedDebt = null;
            SelectedDeposit = null;
            SelectedAccount = null;
            */
            switch (i)
            {
                case 0:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Operations").Visibility = Visibility.Visible;
                    TitleOfList = "Все операции";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).OrderBy(oper => oper.TransactDate));
                    break;

                case 1:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Incomes").Visibility = Visibility.Visible;
                    TitleOfList = "Доходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == true).OrderBy(oper => oper.TransactDate));
                    break;

                case 2:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Expenses").Visibility = Visibility.Visible;
                    TitleOfList = "Расходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == false).OrderBy(oper => oper.TransactDate));
                    break;

                case 3:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Debts").Visibility = Visibility.Visible;
                    TitleOfList = "Долги";
                    Debts = new(DB.GetDb(typeof(DebtsDB)).GetEntries(Search, Filter).Select(d => (Debt)d).OrderBy(d => d.DateOfPick));
                    break;

                case 4:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Deposits").Visibility = Visibility.Visible;
                    TitleOfList = "Вклады";
                    Deposits = new(DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderBy(d => d.OpenDate));
                    break;

                case 5:
                    StackPanels.ForEach(sp => sp.Visibility = Visibility.Collapsed);
                    StackPanels.First(sp => sp.Name == "Accounts").Visibility = Visibility.Visible;
                    TitleOfList = "Счета";
                    Accounts = new(DB.GetDb(typeof(AccountsDB)).GetEntries(Search, Filter).Select(a => (Account)a).OrderBy(a => a.Title));
                    break;
            }
        }

        public void Set(List<StackPanel> stackPanels)
        {
            StackPanels = stackPanels;
            UpdateLists(CodeOper);
        }
    }
}
