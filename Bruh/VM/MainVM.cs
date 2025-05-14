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

        private string search;
        //private List<string> filter;
        //private List<string>? parametres = [];
        private string titleOfList = "";
        private byte codeOper;
        private IModel? selectedEntry;
        private List<Category> categories;
        private Category? filterCategory;
        private decimal? filterMaxAmount;
        private decimal? filterMinAmount;
        private DateTime? filterUpperDate;
        private DateTime? filterLowerDate;

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
        private List<string> Filter = [];
        private Account? filterAccount;
        private List<Account> accountsForFilter;

        /*
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
        */
        public DateTime? FilterUpperDate
        {
            get => filterUpperDate;
            set
            {
                filterUpperDate = value;
                Signal();
                UpdateFilter();
            }
        }
        public DateTime? FilterLowerDate
        {
            get => filterLowerDate;
            set
            {
                filterLowerDate = value;
                Signal();
                UpdateFilter();
            }
        }
        public decimal? FilterMaxAmount
        {
            get => filterMaxAmount;
            set
            {
                filterMaxAmount = value;
                Signal();
                UpdateFilter();
            }
        }
        public decimal? FilterMinAmount
        {
            get => filterMinAmount;
            set
            {
                filterMinAmount = value;
                Signal();
                UpdateFilter();
            }
        }
        public List<Category> Categories
        {
            get => categories;
            set
            {
                categories = value;
                Signal();
            }
        }
        public List<Account> AccountsForFilter
        {
            get => accountsForFilter;
            set
            {
                accountsForFilter = value;
                Signal();
            }
        }
        public Category? FilterCategory
        {
            get => filterCategory;
            set
            {
                filterCategory = value;
                Signal();
                UpdateFilter();
            }
        }
        public Account? FilterAccount
        {
            get => filterAccount;
            set
            {
                filterAccount = value;
                Signal();
                UpdateFilter();
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

        private byte CodeOper
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
                //Parametres = null;
                UpdateLists(codeOper);
            }
        }



        public ICommand AddEntry { get; set; }
        public ICommand EditEntry { get; set; }
        public ICommand RemoveEntry { get; set; }
        public ICommand OpenCategories { get; set; }
        public ICommand ClearFilter { get; set; }

        public ICommand SetIncomes { get; set; }
        public ICommand SetExpenses { get; set; }
        public ICommand SetOperations { get; set; }
        public ICommand SetDebts { get; set; }
        public ICommand SetDeposits { get; set; }
        public ICommand SetAccounts { get; set; }


        public MainVM()
        {
            AllSp = [OperationsSP, DebtsSP, IncomesSP, ExpensesSP, DepositsSP, AccountsSP, FilterSP, FilterAccountSP, FilterAmountSP, FilterCategorySP, FilterDatesSP];
            Help();
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
                    bool change = false;
                    if (SelectedEntry is Operation)
                        change = MessageBox.Show("Хотите ли вы изменить соответствующие записи?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    DB.GetDb(SelectedEntry.GetType().GetCustomAttribute<DBContextAttribute>().Type).Remove(SelectedEntry, change);
                    UpdateLists(CodeOper);
                }
            }, () => SelectedEntry != null);
            OpenCategories = new CommandVM(() => 
            {
                new CategoriesWindow().ShowDialog();
            }, () => true);
            ClearFilter = new CommandVM(() => 
            {
                FilterLowerDate = null;
                FilterUpperDate = null;
                FilterMinAmount = null;
                FilterMaxAmount = null;
                FilterCategory = null;
                FilterAccount = null;

            }, () => true);
        }


        public Visibility FilterSP { get; set; }
        public Visibility FilterAmountSP { get; set; }
        public Visibility FilterDatesSP { get; set; }
        public Visibility FilterAccountSP { get; set; }
        public Visibility FilterCategorySP { get; set; }

        public Visibility OperationsSP { get; set; }
        public Visibility DebtsSP { get; set; }
        public Visibility IncomesSP { get; set; }
        public Visibility ExpensesSP { get; set; }
        public Visibility DepositsSP { get; set; }
        public Visibility AccountsSP { get; set; }
        private List<Visibility> AllSp { get; set; }

        private void UpdateLists(byte i)
        {

            SelectedEntry = null;
            /*
            SelectedOperation = null;
            SelectedDebt = null;
            SelectedDeposit = null;
            SelectedAccount = null;
            */
            List<Visibility> neededPanels = [];
            AllSp.ForEach(sp => sp = Visibility.Collapsed);
            switch (i)
            {
                case 0:
                    neededPanels = [OperationsSP, FilterSP, FilterAccountSP, FilterAmountSP, FilterCategorySP, FilterDatesSP];
                    TitleOfList = "Все операции";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).OrderBy(oper => oper.TransactDate));
                    break;                    

                case 1:
                    neededPanels = [IncomesSP, FilterSP, FilterAccountSP, FilterAmountSP, FilterCategorySP, FilterDatesSP];
                    TitleOfList = "Доходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == true).OrderBy(oper => oper.TransactDate));
                    break;

                case 2:
                    neededPanels = [ExpensesSP, FilterSP, FilterAccountSP, FilterAmountSP, FilterCategorySP, FilterDatesSP];
                    TitleOfList = "Расходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == false).OrderBy(oper => oper.TransactDate));
                    break;

                case 3:
                    neededPanels = [DebtsSP];
                    TitleOfList = "Долги";
                    Debts = new(DB.GetDb(typeof(DebtsDB)).GetEntries(Search, Filter).Select(d => (Debt)d).OrderBy(d => d.DateOfPick));
                    break;

                case 4:
                    neededPanels = [DepositsSP];
                    TitleOfList = "Вклады";
                    Deposits = new(DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderBy(d => d.OpenDate));
                    break;

                case 5:
                    neededPanels = [AccountsSP];
                    TitleOfList = "Счета";
                    Accounts = new(DB.GetDb(typeof(AccountsDB)).GetEntries(Search, Filter).Select(a => (Account)a).OrderBy(a => a.Title));
                    break;
            }
            neededPanels.ForEach(sp => sp = Visibility.Visible);
            AllSp.ForEach(sp => Signal(nameof(sp)));
        }
        private void UpdateFilter()
        {
            Filter.Clear();
            switch (CodeOper)
            {
                case 0:
                case 1:
                case 2:
                    if (FilterMinAmount.HasValue && FilterMinAmount > 0)
                        Filter.Add($"`Cost` >= {FilterMinAmount}");
                    if (FilterMaxAmount.HasValue && FilterMaxAmount > 0)
                        Filter.Add($"`Cost` <= {FilterMaxAmount}");
                    if (FilterCategory != null && FilterCategory.ID != 0)
                        Filter.Add($"CategoryID = {FilterCategory.ID}");
                    if (FilterAccount != null && FilterAccount.ID != 0)
                        Filter.Add($"AccountID = {FilterAccount.ID}");
                    if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                        Filter.Add($"DATE(`TransactDate`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                    if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                        Filter.Add($"DATE(`TransactDate`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    break;
            }

            UpdateLists(CodeOper);
        }
        private void Help()
        {
            AccountsForFilter = [..DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a)];
            Categories = [.. DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c)];
        }

        /*
        public void Set(List<StackPanel> stackPanels)
        {
            StackPanels = stackPanels;
            UpdateLists(CodeOper);
        }
        */
    }
}
