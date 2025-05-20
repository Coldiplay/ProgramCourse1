using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bruh.VM
{
    public class MainVM : BaseVM
    {
        private ObservableCollection<Operation>? operations;
        private ObservableCollection<Debt> debts;
        private ObservableCollection<Deposit>? deposits;
        private ObservableCollection<Account> accounts;

        private string search;
        private string titleOfList = "";
        private byte codeOper;
        private byte incomesCode;
        private IModel? selectedEntry;
        private List<Category>? categories;
        private Category? filterCategory;
        private decimal? filterMaxAmount;
        private decimal? filterMinAmount;
        private DateTime? filterUpperDate;
        private DateTime? filterLowerDate;
        private Account? filterAccount;
        private List<Account>? accountsForFilter;
        private string selectedMode;
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
        private Visibility entriesSp;
        private Visibility mainSp;

        public IModel? SelectedEntry
        {
            get => selectedEntry;
            set
            {
                selectedEntry = value;
                Signal();
            }
        }

        public ObservableCollection<Operation>? Operations
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
        public ObservableCollection<Deposit>? Deposits
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
        //private byte expensesCode;
        private bool isSalaryNeededForNDFL;
        private string incomesMode;
        private string expensesMode;
        private Visibility miscSP;

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
        public string IncomesMode
        {
            get => incomesMode;
            set
            {
                incomesMode = value;
                Signal();
                Signal(nameof(AllIncomes));
            }
        }
        public string ExpensesMode
        {
            get => expensesMode;
            set
            {
                expensesMode = value;
                Signal();
                Signal(nameof(AllExpenses));
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
             * 6 - Главная страница?
             */
            get => codeOper;
            set
            {
                codeOper = value;
                ClearFilter();
                UpdateLists(codeOper);
            }
        }
        /*
        //private byte IncomesCode
        //{
        //    get => incomesCode;
        //    set
        //    {
        //        incomesCode = value;
        //        Signal(nameof(AllIncomes));                
        //        Signal(nameof(IncomesMode));                
        //    }
        //}
        //private byte ExpensesCode
        //{
        //    get => expensesCode;
        //    set
        //    {
        //        expensesCode = value;
        //        Signal(nameof(AllExpenses));
        //        Signal(nameof(ExpensesMode));
        //    }
        //}
        */
        public List<string> Ranges { get; set; } = ["Предыдущий месяц", "Предыдущий год", "Предыдущию неделю", "Предыдущий квартал", "Текущий месяц", "Текущую неделю", "Текущий год", "Текущий квартал"];
        public decimal AllIncomes => GetSumm(true);
        public decimal AllExpenses => GetSumm(false);
        public bool IsSalaryNeededForNDFL
        {
            get => isSalaryNeededForNDFL;
            set
            {
                isSalaryNeededForNDFL = value;
                Signal();
                Signal(nameof(NDFL));
            }
        }
        public string NDFL 
        {
            get
            {
                GetAllIncomesForNDFL(out string case1, out string case2);
                switch (DateTime.Today.Day)
                {
                    case <= 28 and >= 5:
                        return $"Подоходный налог за ближайший отчетный период\n(С {DateTime.Today:01.MM.yyyy} по {DateTime.Today:22.MM.yyyy})\nсоставляет {case1} Рублей";
                    case > 5 or < 28:
                        DateTime date = DateTime.Today.AddMonths(-1);
                        return $"Подоходный налог за ближайший отчетный период \n(С {date:23.MM.yyyy} по {date.ToString($"{DateTime.DaysInMonth(date.Year, date.Month)}.MM.yyyy")})\nсоставляет {case2} Рублей";
                }
                return "Что-то пошло не так, проверб условия";
            }
        }
        private decimal GetSumm(bool income)
        {
            /*  За
             * 0 - Предыдущий месяц
             * 1 - Предыдущий год  --Квартал--
             * 2 - Пред неделю --Год--
             * 3 - Пред квартал
             * 4 - Текущий месяц
             * 5 - Текущую неделю
             * 6 - Текущий год
             * 7 - Текущий квартал
            */
            DateTime today = DateTime.Today;
            DateTime date;
            switch (income ? Ranges.IndexOf(IncomesMode) : Ranges.IndexOf(ExpensesMode))
            {
                case 0:
                    date = today.AddMonths(-1);
                    return GetSumm(new(date.Year, date.Month, 1), new(date.Year,date.Month, DateTime.DaysInMonth(date.Year, date.Month)), income);
                case 1:
                    date = today.AddYears(-1);
                    return GetSumm(new(date.Year, 1, 1), new(date.Year, 12, 31), income);
                case 2:
                    today = today.DayOfWeek != DayOfWeek.Sunday ? today.AddDays((((int)today.DayOfWeek - 1) * -1) - 7) : today.AddDays(-13);
                    return GetSumm(today, today.AddDays(6), income);
                case 3:
                    DateTime date2 = today;
                    // зДЕСЬ ХРЕНЬ ПОСОМТРИ // Вроде всё
                    while (date2.Month % 3 != 0)
                        date2 = date2.AddMonths(1);

                    date2 = date2.AddMonths(-5);
                    date = new(date2.Year, date2.Month, 1);
                    date2 = date.AddMonths(3);
                    date2 = new(date2.Year, date2.Month, DateTime.DaysInMonth(date2.Year, date2.Month));
                    return GetSumm(date, date2, income);

                case 4:
                    return GetSumm(new DateTime(today.Year, today.Month, 1), today, income);

                case 5:
                    return GetSumm(today.DayOfWeek != DayOfWeek.Sunday ? today.AddDays(((int)today.DayOfWeek - 1) * -1) : today.AddDays(-6), today, income);

                case 6:
                    return GetSumm(new DateTime(today.Year, 1, 1), today, income);

                case 7:
                    if (today.Month <= 3)
                        return GetSumm(new DateTime(today.Year, 1, 1), today, income);
                    else if (today.Month <= 6)
                        return GetSumm(new(today.Year, 4, 1), today, income);
                    else if (today.Month <= 9)
                        return GetSumm(new(today.Year, 7, 1), today, income);
                    else
                        return GetSumm(new(today.Year, 10, 1), today, income);
            }
            return 0;
        }
        private decimal GetSumm(DateTime lowerDate, DateTime upperDate, bool income)
        {
            decimal summ = 0;
            for (int i = 0; i < Operations?.Count; i++)
            {
                Operation oper = Operations[i];
                if (oper.TransactDate >= lowerDate && oper.TransactDate <= upperDate && oper.Income == income)
                    summ += oper.Cost;
            }
            return summ;
        }
        private decimal GetSummForNDFL(DateTime lowerDate, DateTime upperDate, bool isSalaryNeeded)
        {
            decimal summ = 0;
            List<string> salary = ["зп", "зарплата", "заработная плата", "премия"];
            for (int i = 0; i < Operations?.Count; i++)
            {
                Operation oper = Operations[i];
                if (salary.Contains(oper.Category.Title.ToLower()) && !isSalaryNeeded)
                    continue;

                if (oper.TransactDate >= lowerDate && oper.TransactDate <= upperDate && oper.Income)
                    summ += oper.Cost;
            }
            return summ;
        }
        private void GetAllIncomesForNDFL(out string result1, out string result2)
        {
            // Для НДФЛ с 1 по 22, Для НДФЛ с 22 по 5
            decimal summ1 = 0, summ2 = 0;
            DateTime today = DateTime.Today;
            DateTime date1lower = new(today.Year, today.Month, 1);
            DateTime date1upper = date1lower.AddDays(21);
            DateTime date2lower = today.AddMonths(-1);
            date2lower = new DateTime(date2lower.Year, date2lower.Month, 23);
            DateTime date2upper = new(date2lower.Year, date2lower.Month, DateTime.DaysInMonth(date2lower.Year, date2lower.Month));

            summ1 += GetSummForNDFL(date1lower, date1upper, IsSalaryNeededForNDFL) + IncomeFromDeposits(date1lower, date1upper);
            summ2 += GetSummForNDFL(date2lower, date2upper, IsSalaryNeededForNDFL) + IncomeFromDeposits(date2lower, date2upper);

            result1 = Math.Round(summ1 * 0.13M, 2).ToString();
            result2 = Math.Round(summ2 * 0.13M, 2).ToString();

            GC.Collect();
            return;
        }
        public decimal IncomeFromDeposits(DateTime lowerDate, DateTime upperDate)
        {
            decimal summ = 0;
            for (int i = 0; i < Deposits?.Count; i++)
            {
                Deposit deposit = Deposits[i];
                try
                {
                     summ += decimal.Parse(deposit.GetProbSumm) - decimal.Parse(deposit.GetProbableSumm(lowerDate, upperDate, deposit.Capitalization));
                }
                catch (Exception)
                {
                }
            }
            return summ;
        }
        private void GetRange(int oper ,out DateTime lowerDate, out DateTime upperDate)
        {
            DateTime today = DateTime.Today;
            switch (oper)
            {
                case 0:
                    lowerDate = today.AddMonths(-1);
                    lowerDate = new(lowerDate.Year, lowerDate.Month, 1);
                    upperDate = new(lowerDate.Year, lowerDate.Month, DateTime.DaysInMonth(lowerDate.Year, lowerDate.Month));
                    break;

                case 1:
                    lowerDate = today.AddYears(-1);
                    lowerDate = new(lowerDate.Year,1,1);
                    upperDate = new(lowerDate.Year,12,31);
                    break;
                case 2:
                    lowerDate = today.DayOfWeek != DayOfWeek.Sunday ? today.AddDays((((int)today.DayOfWeek - 1) * -1) - 7) : today.AddDays(-13);
                    upperDate = today.AddDays(6);
                    break;
                case 3:
                    upperDate = today;
                    while (upperDate.Month % 3 != 0)
                        upperDate = upperDate.AddMonths(1);

                    upperDate = upperDate.AddMonths(-5);
                    lowerDate = new(upperDate.Year, upperDate.Month, 1);
                    upperDate = lowerDate.AddMonths(3);
                    upperDate = new(upperDate.Year, upperDate.Month, DateTime.DaysInMonth(upperDate.Year, upperDate.Month));
                    break;

                case 4:
                    lowerDate = new(today.Year, today.Month, 1);
                    upperDate = today;
                    break;

                case 5:
                    lowerDate = today.DayOfWeek != DayOfWeek.Sunday ? today.AddDays(((int)today.DayOfWeek - 1) * -1) : today.AddDays(-6);
                    upperDate = today;
                    break;

                case 6:
                    lowerDate = new(today.Year, 1, 1);
                    upperDate = today;
                    break;

                case 7:
                    if (today.Month <= 3)
                        lowerDate = new(today.Year, 1, 1);
                    else if (today.Month <= 6)
                        lowerDate = new(today.Year, 4, 1);
                    else if (today.Month <= 9)
                        lowerDate = new(today.Year, 7, 1);
                    else
                        lowerDate = new(today.Year, 10, 1);

                    upperDate = today;
                    break;
                default:
                    upperDate = new(1,1,1);
                    lowerDate = upperDate;
                    break;
            }
        }
        private void GetCategories(out List<Cat> incomesCats, out List<Cat> expensesCats)
        {
            incomesCats = [];
            expensesCats = [];
            GetRange(Ranges.IndexOf(IncomesMode), out var lowerIncomes, out var upperIncomes);
            GetRange(Ranges.IndexOf(ExpensesMode), out var lowerExpenses, out var upperExpenses);
            for (int i = 0; i < Operations?.Count; i++)
            {
                Operation oper = Operations[i];

                List<Cat> list;
                DateTime lowerDate;
                DateTime upperDate;
                if (oper.Income)
                {
                    list = incomesCats;
                    lowerDate = lowerIncomes;
                    upperDate = upperIncomes;
                }
                else
                { 
                    list = expensesCats;
                    lowerDate = lowerExpenses;
                    upperDate = upperExpenses;
                }
                var cat = list.FirstOrDefault(c => c.Link?.ID == oper.Category.ID);
                if (cat == null)
                {
                    list.Add(new Cat { Link = oper.Category, TotalAmount = oper.Cost });
                }
                else
                {
                    cat.TotalAmount += oper.Cost;
                }
            }
        }
        protected class Cat 
        {
            public Category? Link { get; set; }
            public decimal TotalAmount { get; set; }
        }
        public PlotModel CategoriesIncomesPlot { get; private set; }
        public PlotModel CategoriesExpensesPlot { get; private set; }
        private void ChangePlotModels()
        {
            /*
            //if (CategoriesIncomesPlot == null || CategoriesExpensesPlot == null)
            //{
            //    CategoriesIncomesPlot = new()
            //    {
            //        EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            //        Title = "Категории доходов"

            //    };

            //    CategoriesExpensesPlot = new()
            //    {
            //        EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
            //        Title = "Категории расходов"
            //    };
            //}
            //CategoriesIncomesPlot.Series.Clear();
            //CategoriesExpensesPlot.Series.Clear();
            */
            PlotModel incomesPlot = new()
            {
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
                Title = "Категории расходов"
            };
            PlotModel expensesPlot = new()
            {
                EdgeRenderingMode = EdgeRenderingMode.PreferSpeed,
                Title = "Категории доходов"
            };


            PieSeries incomesCategories = new();
            PieSeries expensesCategories = new();
            GetCategories(out List<Cat> incomesCats, out List<Cat> expensesCats);
            for (int i = 0; i < incomesCats.Count; i++)
            {
                Cat? item = incomesCats[i];
                var slice = new PieSlice($"{item.Link?.Title} ({item.TotalAmount})", (double)item.TotalAmount);
                incomesCategories.Slices.Add(slice);
            }
            for (int i = 0; i < expensesCats.Count; i++)
            {
                Cat? item = expensesCats[i];
                var slice = new PieSlice($"{item.Link?.Title} ({item.TotalAmount})", (double)item.TotalAmount);
                expensesCategories.Slices.Add(slice);
            }

            incomesPlot.Series.Add(incomesCategories);
            //incomesPlot.Series[0].
            expensesPlot.Series.Add(expensesCategories);

            CategoriesIncomesPlot = incomesPlot;
            CategoriesExpensesPlot = expensesPlot;

            GC.Collect();
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
        public ICommand SetToMain { get; set; }
        //public ICommand ChangeIncomesMode { get; set; }
        //public ICommand ChangeExpensesMode { get; set; }


        public MainVM()
        {
            UpdateListsForFilter();
            CodeOper = 6;

            SetOperations = new CommandVM(() => CodeOper = 0, () => true);
            SetIncomes = new CommandVM(() => CodeOper = 1, () => true);
            SetExpenses = new CommandVM(() => CodeOper = 2, () => true);
            SetDebts = new CommandVM(() => CodeOper = 3, () => true);
            SetDeposits = new CommandVM(() => CodeOper = 4, () => true);
            SetAccounts = new CommandVM(() => CodeOper = 5, () => true);
            SetToMain = new CommandVM(() => CodeOper = 6, () => true);

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
                //UpdateListsForFilter();
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
        public Visibility MiscSP
        {
            get => miscSP;
            set
            {
                miscSP = value;
                Signal();
            }
        }
        public Visibility MainSp
        {
            get => mainSp;
            set
            {
                mainSp = value;
                Signal();
            }
        }
        public Visibility EntriesSp
        {
            get => entriesSp;
            set
            {
                entriesSp = value;
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

        private void UpdateLists(byte code)
        {
            //UpdateListsForFilter();
            HideAllSps();
            SelectedEntry = null;
            EntriesSp = Visibility.Visible;
            switch (code)
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
                    MiscSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    FilterDatesSP = Visibility.Visible;
                    FilterModeSP = Visibility.Visible;
                    TitleOfList = "Вклады";
                    Deposits = new(DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderByDescending(d => d.OpenDate));
                    break;

                case 5:
                    MiscSP = Visibility.Visible;
                    AccountsSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    TitleOfList = "Счета";
                    Accounts = new(DB.GetDb(typeof(AccountsDB)).GetEntries(Search, Filter).Select(a => (Account)a).OrderByDescending(a => a.Title));
                    break;

                case 6:
                    EntriesSp = Visibility.Collapsed;
                    MainSp = Visibility.Visible;
                    if (Operations == null || Deposits == null)
                    {
                        Operations = [.. DB.GetDb(typeof(OperationsDB)).GetEntries(Search, Filter).Select(s => (Operation)s).OrderByDescending(oper => oper.TransactDate)];
                        Deposits = [.. DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderByDescending(d => d.OpenDate)];
                    }
                    ChangePlotModels();
                    TitleOfList = "Главная";

                    break;
            }            
        }
        private void UpdateFilter()
        {
            Filter.Clear();
            string amountString = "";
            string dateString = "";
            switch (CodeOper)
            {
                case 0:
                case 1:
                case 2:
                    amountString = "Cost";
                    dateString = "TransactDate";
                    if (FilterCategory != null && FilterCategory.ID != 0)
                        Filter.Add($"CategoryID = {FilterCategory.ID}");
                    if (FilterAccount != null && FilterAccount.ID != 0)
                        Filter.Add($"AccountID = {FilterAccount.ID}");
                    break;
                case 3:
                    amountString = "Summ";
                    dateString = SelectedMode == FilterModes[0] ? "DateOfPick" : "DateOfReturn";
                    /*
                    //if (SelectedMode == FilterModes[0])
                    //    dateString = "DateOfPick";
                    //else
                    //    dateString = "DateOfReturn";
                    */
                    break;
                case 4:
                    amountString = "InitalSumm";
                    dateString = SelectedMode == FilterModes[0] ? "DateOfOpening" : "DateOfClosing";
                    break;
                case 5:
                    amountString = "Balance";
                    break;
            }
            if (FilterMinAmount.HasValue && FilterMinAmount > 0)
                Filter.Add($"`{amountString}` >= {FilterMinAmount}");
            if (FilterMaxAmount.HasValue && FilterMaxAmount > 0)
                Filter.Add($"`{amountString}` <= {FilterMaxAmount}");
            if (FilterLowerDate.HasValue && (FilterLowerDate < FilterUpperDate || FilterUpperDate == null))
                Filter.Add($"DATE(`{dateString}`) >= '{FilterLowerDate.Value:yyyy-MM-dd}'");
            if (FilterUpperDate.HasValue && (FilterUpperDate > FilterLowerDate || FilterLowerDate == null))
                Filter.Add($"DATE(`{dateString}`) <= '{FilterUpperDate.Value:yyyy-MM-dd}'");
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
            MiscSP = Visibility.Hidden;
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

            MainSp = Visibility.Collapsed;
            EntriesSp = Visibility.Collapsed;
        }
        private void ShowFilterForOpers()
        {
            MiscSP = Visibility.Visible;
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
