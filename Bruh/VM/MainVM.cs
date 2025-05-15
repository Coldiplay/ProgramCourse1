using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    public class MainVM : BaseVM
    {
        private ObservableCollection<Operation> operations;
        private ObservableCollection<Debt> debts;
        private ObservableCollection<Deposit> deposits;
        private ObservableCollection<Account> accounts;

        private string search;
        private string titleOfList = "";
        private byte codeOper;
        private IModel? selectedEntry;
        private List<Category>? categories;
        private Category? filterCategory;
        private decimal? filterMaxAmount;
        private decimal? filterMinAmount;
        private DateTime? filterUpperDate;
        private DateTime? filterLowerDate;
        private Account? filterAccount;
        private List<Account>? accountsForFilter;

        public IModel? SelectedEntry
        {
            get => selectedEntry;
            set
            {
                selectedEntry = value;
                Signal();
            }
        }

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
        private Visibility filterSP;
        private Visibility filterAmountSP;
        private Visibility filterDatesSP;
        private Visibility filterAccountSP;
        private Visibility filterCategorySP;
        private Visibility operationsSP;
        private Visibility debtsSP;
        private Visibility incomesSP;
        private Visibility expensesSP;
        private Visibility depositsSP;
        private Visibility accountsSP;
        private Visibility filterModeSP;
        private string selectedMode;
        private List<string> filterModes;

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
        public List<Category>? Categories
        {
            get => categories;
            set
            {
                categories = value;
                Signal();
            }
        }
        public List<Account>? AccountsForFilter
        {
            get => accountsForFilter;
            set
            {
                accountsForFilter = value;
                Signal();
            }
        }
        public string[] FilterModes { get; set; } = ["Дата открытия", "Дата закрытия"];
        public string SelectedMode
        {
            get => selectedMode;
            set
            {
                selectedMode = value;
                Signal();
                UpdateFilter();
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
                ClearFilter();
                UpdateLists(codeOper);
            }
        }


        public ICommand AddEntry { get; set; }
        public ICommand EditEntry { get; set; }
        public ICommand RemoveEntry { get; set; }
        public ICommand OpenCategories { get; set; }
        public ICommand OpenBanks { get; set; }
        public ICommand OpenCurrencies { get; set; }
        public ICommand ClearFilterCMD { get; set; }

        public ICommand SetIncomes { get; set; }
        public ICommand SetExpenses { get; set; }
        public ICommand SetOperations { get; set; }
        public ICommand SetDebts { get; set; }
        public ICommand SetDeposits { get; set; }
        public ICommand SetAccounts { get; set; }


        public MainVM()
        {
            UpdateListsForFilter();
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
                UpdateLists(CodeOper);
                UpdateListsForFilter();
            }, () => true);
            OpenBanks = new CommandVM(() => 
            {
                new BanksWindow().ShowDialog();
                UpdateLists(CodeOper);
            }, () => true);
            OpenCurrencies = new CommandVM(() =>
                {
                    new CurrenciesWindow().ShowDialog();
                    UpdateLists(CodeOper);
                }, () => true);
            ClearFilterCMD = new CommandVM(ClearFilter, () => true);
        }


        public Visibility FilterSP 
        { 
            get => filterSP; 
            set 
            { 
                filterSP = value;
                Signal(); 
            } 
        }
        public Visibility FilterModeSP
        {
            get => filterModeSP;
            set
            {
                filterModeSP = value;
                Signal();
            }
        }
        public Visibility FilterAmountSP
        {
            get => filterAmountSP;
            set
            {
                filterAmountSP = value;
                Signal();
            }
        }
        public Visibility FilterDatesSP
        {
            get => filterDatesSP;
            set
            {
                filterDatesSP = value;
                Signal();
            }
        }
        public Visibility FilterAccountSP
        {
            get => filterAccountSP;
            set
            {
                filterAccountSP = value;
                Signal();
            }
        }
        public Visibility FilterCategorySP
        {
            get => filterCategorySP;
            set
            {
                filterCategorySP = value;
                Signal();
            }
        }

        public Visibility OperationsSP
        {
            get => operationsSP;
            set
            {
                operationsSP = value;
                Signal();
            }
        }
        public Visibility DebtsSP
        {
            get => debtsSP;
            set
            {
                debtsSP = value;
                Signal();
            }
        }
        public Visibility IncomesSP
        {
            get => incomesSP;
            set
            {
                incomesSP = value;
                Signal();
            }
        }
        public Visibility ExpensesSP
        {
            get => expensesSP;
            set
            {
                expensesSP = value;
                Signal();
            }
        }
        public Visibility DepositsSP
        {
            get => depositsSP;
            set
            {
                depositsSP = value;
                Signal();
            }
        }
        public Visibility AccountsSP
        {
            get => accountsSP;
            set
            {
                accountsSP = value;
                Signal();
            }
        }

        private void UpdateLists(byte i)
        {
            //UpdateListsForFilter();
            HideAllSps();
            SelectedEntry = null;

            switch (i)
            {
                case 0:
                    OperationsSP = Visibility.Visible;
                    ShowFilterForOpers();
                    TitleOfList = "Все операции";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).OrderByDescending(oper => oper.TransactDate));
                    break;                    

                case 1:
                    IncomesSP = Visibility.Visible;
                    ShowFilterForOpers();
                    TitleOfList = "Доходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == true).OrderByDescending(oper => oper.TransactDate));
                    break;

                case 2:
                    ExpensesSP = Visibility.Visible;
                    ShowFilterForOpers();
                    TitleOfList = "Расходы";
                    Operations = new(DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).Where(oper => oper.Income == false).OrderByDescending(oper => oper.TransactDate));
                    break;

                case 3:
                    DebtsSP = Visibility.Visible;
                    FilterSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    FilterDatesSP = Visibility.Visible;
                    FilterModeSP = Visibility.Visible;
                    TitleOfList = "Долги";
                    Debts = new(DB.GetDb(typeof(DebtsDB)).GetEntries(Search, Filter).Select(d => (Debt)d).OrderByDescending(d => d.DateOfPick));
                    break;

                case 4:
                    DepositsSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    FilterDatesSP = Visibility.Visible;
                    FilterModeSP = Visibility.Visible;
                    TitleOfList = "Вклады";
                    Deposits = new(DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderByDescending(d => d.OpenDate));
                    break;

                case 5:
                    AccountsSP = Visibility.Visible;
                    TitleOfList = "Счета";
                    Accounts = new(DB.GetDb(typeof(AccountsDB)).GetEntries(Search, Filter).Select(a => (Account)a).OrderByDescending(a => a.Title));
                    break;
            }            
        }
        private void UpdateFilter()
        {
            Filter.Clear();
            string amountString = "";
            switch (CodeOper)
            {
                case 0:
                case 1:
                case 2:
                    amountString = "Cost";
                    if (FilterCategory != null && FilterCategory.ID != 0)
                        Filter.Add($"CategoryID = {FilterCategory.ID}");
                    if (FilterAccount != null && FilterAccount.ID != 0)
                        Filter.Add($"AccountID = {FilterAccount.ID}");
                    if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                        Filter.Add($"DATE(`TransactDate`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                    if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                        Filter.Add($"DATE(`TransactDate`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    break;
                case 3:
                    amountString = "Summ";
                    if (SelectedMode == FilterModes[0])
                    {
                        if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                            Filter.Add($"DATE(`DateOfPick`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                        if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                            Filter.Add($"DATE(`DateOfPick`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    }
                    else
                    {
                        if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                            Filter.Add($"DATE(`DateOfReturn`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                        if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                            Filter.Add($"DATE(`DateOfReturn`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    }
                    break;
                case 4:
                    amountString = "InitalSumm";
                    if (SelectedMode == FilterModes[0])
                    {
                        if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                            Filter.Add($"DATE(`DateOfOpening`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                        if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                            Filter.Add($"DATE(`DateOfOpening`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    }
                    else
                    {
                        if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                            Filter.Add($"DATE(`DateOfClosing`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
                        if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                            Filter.Add($"DATE(`DateOfClosing`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
                    }
                    break;
                case 5:
                    amountString = "Balance";
                    break;
            }
            if (FilterMinAmount.HasValue && FilterMinAmount > 0)
                Filter.Add($"`{amountString}` >= {FilterMinAmount}");
            if (FilterMaxAmount.HasValue && FilterMaxAmount > 0)
                Filter.Add($"`{amountString}` <= {FilterMaxAmount}");
            UpdateLists(CodeOper);
        }

        private void ClearFilter()
        {
            Filter.Clear();
            FilterLowerDate = null;
            FilterUpperDate = null;
            FilterMinAmount = null;
            FilterMaxAmount = null;
            FilterCategory = null;
            FilterAccount = null;
        }
        private void HideAllSps()
        {
            OperationsSP = Visibility.Collapsed;
            IncomesSP = Visibility.Collapsed;
            ExpensesSP = Visibility.Collapsed;
            DebtsSP = Visibility.Collapsed;
            DepositsSP = Visibility.Collapsed;
            AccountsSP = Visibility.Collapsed;
            FilterSP = Visibility.Collapsed;
            FilterModeSP = Visibility.Collapsed;
            FilterAccountSP = Visibility.Collapsed;
            FilterAmountSP = Visibility.Collapsed;
            FilterCategorySP = Visibility.Collapsed;
            FilterDatesSP = Visibility.Collapsed;
        }
        private void ShowFilterForOpers()
        {
            FilterSP = Visibility.Visible;
            FilterAccountSP = Visibility.Visible;
            FilterAmountSP = Visibility.Visible;
            FilterCategorySP = Visibility.Visible;
            FilterDatesSP = Visibility.Visible;
        }
        private void UpdateListsForFilter()
        {
            int accountId = FilterAccount?.ID ?? 0;
            int categoryId = FilterCategory?.ID ?? 0;
            //filterAccount = null;
            //filterCategory = null;
            //AccountsForFilter?.Clear();
            //Categories?.Clear();
            AccountsForFilter = [..DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a)];
            Categories = [.. DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c)];
            filterAccount = AccountsForFilter.FirstOrDefault(acc => acc.ID == accountId);
            filterCategory = Categories.FirstOrDefault(c => c.ID == categoryId);
        }
    }
}
