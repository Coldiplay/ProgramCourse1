using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    internal class MainVM : BaseVM
    {
        private ObservableCollection<Operation>? operations;
        private ObservableCollection<Debt> debts;
        private ObservableCollection<Deposit>? deposits;
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
            private set
            {
                operations = value;
                Signal();
            }
        }
        public ObservableCollection<Debt> Debts
        {
            get => debts;
            private set
            {
                debts = value;
                Signal();
            }
        }
        public ObservableCollection<Deposit>? Deposits
        {
            get => deposits;
            private set
            {
                deposits = value;
                Signal();
            }
        }
        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            private set
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
        private bool isSalaryNeededForNDFL;
        private string incomesMode;
        private string expensesMode;
        private Visibility miscSP;
        private PlotModel categoriesIncomesPlot;
        private PlotModel categoriesExpensesPlot;

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
            private set
            {
                categories = value;
                Signal();
            }
        }
        public List<Account>? AccountsForFilter
        {
            get => accountsForFilter;
            private set
            {
                accountsForFilter = value;
                Signal();
            }
        }
        public string[] FilterModes { get; private set; } = ["Дата открытия", "Дата закрытия"];
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
                if (value == filterCategory)
                    return;
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
                if (value == filterAccount)
                    return;
                filterAccount = value;
                Signal();
                UpdateFilter();
            }
        }
        

        public string TitleOfList
        {
            get => titleOfList;
            private set
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
                Signal(nameof(AllIncomesFromDeposits));
                ChangePlotModels();
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
                ChangePlotModels();
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
             * 6 - Главная страница
             */
            get => codeOper;
            set
            {
                codeOper = value;
                ClearFilter();
                UpdateLists(codeOper);
            }
        }
        public List<string> Ranges { get; private set; } =
        [
            "Предыдущий месяц",
            "Предыдущий год",
            "Предыдущию неделю",
            "Предыдущий квартал",
            "Текущий месяц",
            "Текущую неделю",
            "Текущий год",
            "Текущий квартал"
        ];
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
            }
        }
        private decimal GetSumm(bool income)
        {
            GetRange(income ? (byte)Ranges.IndexOf(IncomesMode) : (byte)Ranges.IndexOf(ExpensesMode), out var lowerDate, out var upperDate);
            return GetSumm(lowerDate, upperDate, income);
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
            // Для НДФЛ с 1 по 22, Для НДФЛ с 23 по 31/30/28/29
            decimal summ1 = 0, summ2 = 0;
            DateTime today = DateTime.Today;
            DateTime date1lower = new(today.Year, today.Month, 1);
            DateTime date1upper = new(today.Year, today.Month, 22);
            DateTime date2lower = today.AddMonths(-1);
            date2lower = new DateTime(date2lower.Year, date2lower.Month, 23);
            DateTime date2upper = new(date2lower.Year, date2lower.Month, DateTime.DaysInMonth(date2lower.Year, date2lower.Month));

            summ1 += GetSummForNDFL(date1lower, date1upper, IsSalaryNeededForNDFL);
            summ2 += GetSummForNDFL(date2lower, date2upper, IsSalaryNeededForNDFL);

            result1 = Math.Round(GetNdflForSumm(summ1), 2).ToString();
            result2 = Math.Round(GetNdflForSumm(summ2), 2).ToString();

            GC.Collect();
            return;
        }
        private decimal GetNdflForSumm(decimal summ)
        {
            decimal tax = 0;
            bool _continue = true;
            byte count = 1;
            while (_continue)
            {
                switch (count)
                {
                    case 1:
                        if (summ >= 2400000)
                        {
                            tax += 3120000;
                            count++;
                        }
                        else
                        {
                            tax = summ * 0.13M;
                            _continue = false;
                        }
                        break;
                    case 2:
                        if (summ >= 5000000)
                        {
                            tax += 390000;
                            count++;
                        }
                        else
                        {
                            tax += (summ - 2400000) * 0.15M;
                            _continue = false;
                        }
                        break;
                    case 3:
                        if (summ >= 20000000)
                        {
                            tax += 2700000;
                            count++;
                        }
                        else
                        {
                            tax += (summ - 5000000) * 0.18M;
                            _continue = false;
                        }
                        break;
                    case 4:
                        if (summ >= 50000000)
                        {
                            tax += 6000000 + (summ - 50000000) * 0.22M;
                        }
                        else
                        {
                            tax += (summ - 20000000) * 0.2M;
                        }
                        _continue = false;
                        break;
                }                
            }
            return tax;
        }
        private decimal IncomeFromDeposits(DateTime lowerDate, DateTime upperDate)
        {
            decimal summ = 0;
            for (int i = 0; i < Deposits?.Count; i++)
            {
                Deposit deposit = Deposits[i];
                try
                {
                    decimal dep1 = decimal.Parse(deposit.GetProbableSumm(deposit.OpenDate, lowerDate, deposit.Capitalization));
                    decimal dep2 = decimal.Parse(deposit.GetProbableSumm(deposit.OpenDate, upperDate, deposit.Capitalization));
                    summ += dep2 - dep1;
                }
                catch (Exception)
                {
                }
            }
            return summ;
        }
        public decimal AllIncomesFromDeposits 
        {
            get 
            {
                GetRange((byte)Ranges.IndexOf(IncomesMode), out var lowerDate, out var upperDate);
                return IncomeFromDeposits(lowerDate, upperDate);
            }
        }
        private void GetRange(byte oper ,out DateTime lowerDate, out DateTime upperDate)
        {
            /*  За
             * 0 - Предыдущий месяц
             * 1 - Предыдущий год
             * 2 - Пред неделю
             * 3 - Пред квартал
             * 4 - Текущий месяц
             * 5 - Текущую неделю
             * 6 - Текущий год
             * 7 - Текущий квартал
             */
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
                    upperDate = lowerDate.AddDays(6);
                    break;
                
                //Оставить также или заменить на вариант подобный case 7?
                case 3:
                    upperDate = today;
                    while (upperDate.Month % 3 != 0)
                        upperDate = upperDate.AddMonths(1);

                    upperDate = upperDate.AddMonths(-5);
                    lowerDate = new(upperDate.Year, upperDate.Month, 1);
                    upperDate = lowerDate.AddMonths(2);
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
                    lowerDate = today.Month switch
                    {
                        <= 3 => new(today.Year, 1, 1),
                        <= 6 => new(today.Year, 4, 1),
                        <= 9 => new(today.Year, 7, 1),
                        _ => new(today.Year, 10, 1)
                    };
                    upperDate = today;
                    break;

                default:
                    upperDate = new(1,1,1);
                    lowerDate = upperDate;
                    break;
            }
        }
        private void GetCategories(out List<Categor> incomesCats, out List<Categor> expensesCats)
        {
            incomesCats = [];
            expensesCats = [];
            GetRange((byte)Ranges.IndexOf(IncomesMode), out var lowerIncomes, out var upperIncomes);
            GetRange((byte)Ranges.IndexOf(ExpensesMode), out var lowerExpenses, out var upperExpenses);
            for (int i = 0; i < Operations?.Count; i++)
            {
                Operation oper = Operations[i];

                List<Categor> list;
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

                if (oper.TransactDate > upperDate || oper.TransactDate < lowerDate)
                    continue;


                var cat = list.FirstOrDefault(c => c.Link?.ID == oper.Category.ID);
                if (cat == null)
                {
                    list.Add(new Categor { Link = oper.Category, TotalAmount = oper.Cost });
                }
                else
                {
                    cat.TotalAmount += oper.Cost;
                }
            }
        }
        private class Categor 
        {
            internal Category? Link { get; set; }
            internal decimal TotalAmount { get; set; }
        }
        public PlotModel CategoriesIncomesPlot
        {
            get => categoriesIncomesPlot;
            private set
            {
                categoriesIncomesPlot = value;
                Signal();
            }
        }
        public PlotModel CategoriesExpensesPlot
        {
            get => categoriesExpensesPlot;
            private set
            {
                categoriesExpensesPlot = value;
                Signal();
            }
        }
        private void ChangePlotModels()
        {
            PlotModel incomesPlot = new()
            {
                EdgeRenderingMode = EdgeRenderingMode.PreferGeometricAccuracy,
                Title = "Категории доходов"
            };
            PlotModel expensesPlot = new()
            {
                EdgeRenderingMode = EdgeRenderingMode.PreferGeometricAccuracy,
                Title = "Категории расходов"
            };

            FixedPieSeries incomesCategories = new();
            PieSeries expensesCategories = new();
            GetCategories(out List<Categor> incomesCats, out List<Categor> expensesCats);
            for (int i = 0; i < incomesCats.Count; i++)
            {
                Categor? item = incomesCats[i];
                var slice = new PieSlice($"{item.Link?.Title}", (double)item.TotalAmount);
                incomesCategories.Slices.Add(slice);
            }
            for (int i = 0; i < expensesCats.Count; i++)
            {
                Categor? item = expensesCats[i];
                var slice = new PieSlice($"{item.Link?.Title}", (double)item.TotalAmount);
                expensesCategories.Slices.Add(slice);
            }
            if (AllIncomesFromDeposits > 0)
                incomesCategories.Slices.Add(new PieSlice($"Вклады", (double)AllIncomesFromDeposits));

            incomesCategories.TickDistance = -32;
            incomesCategories.TickHorizontalLength = 0;
            incomesCategories.TickRadialLength = 0;
            incomesCategories.TickLabelDistance = 1;
            incomesCategories.StrokeThickness = 0;

            expensesCategories.TickDistance = -32;
            expensesCategories.TickHorizontalLength = 0;
            expensesCategories.TickRadialLength = 0;
            expensesCategories.TickLabelDistance = 1;
            expensesCategories.StrokeThickness = 0;

            //incomesCategories.InsideLabelColor = OxyColors.White;
            incomesCategories.TextColor = OxyColors.White;
            expensesCategories.TextColor = OxyColors.White;
            incomesPlot.Series.Add(incomesCategories);
            expensesPlot.Series.Add(expensesCategories);

            CategoriesIncomesPlot = incomesPlot;
            CategoriesExpensesPlot = expensesPlot;

            GC.Collect();
        }

        public ICommand AddEntry { get; private set; }
        public ICommand EditEntry { get; private set; }
        public ICommand RemoveEntry { get; private set; }
        public ICommand OpenCategories { get; private set; }
        public ICommand OpenBanks { get; private set; }
        public ICommand OpenCurrencies { get; private set; }
        public ICommand ClearFilterCMD { get; private set; }

        public ICommand SetIncomes { get; private set; }
        public ICommand SetExpenses { get; private set; }
        public ICommand SetOperations { get; private set; }
        public ICommand SetDebts { get; private set; }
        public ICommand SetDeposits { get; private set; }
        public ICommand SetAccounts { get; private set; }
        public ICommand SetToMain { get; private set; }

        public MainVM()
        {
            IncomesMode = Ranges[4];
            ExpensesMode = Ranges[4];

            UpdateListsForFilter();
            CodeOper = 5;

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
                UpdateListsForFilter();
                UpdateLists(CodeOper);
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
            private set 
            { 
                filterSP = value;
                Signal(); 
            } 
        }
        public Visibility FilterModeSP
        {
            get => filterModeSP;
            private set
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
            private set
            {
                filterDatesSP = value;
                Signal();
            }
        }
        public Visibility FilterAccountSP
        {
            get => filterAccountSP;
            private set
            {
                filterAccountSP = value;
                Signal();
            }
        }
        public Visibility FilterCategorySP
        {
            get => filterCategorySP;
            private set
            {
                filterCategorySP = value;
                Signal();
            }
        }
        public Visibility MiscSP
        {
            get => miscSP;
            private set
            {
                miscSP = value;
                Signal();
            }
        }
        public Visibility MainSp
        {
            get => mainSp;
            private set
            {
                mainSp = value;
                Signal();
            }
        }
        public Visibility EntriesSp
        {
            get => entriesSp;
            private set
            {
                entriesSp = value;
                Signal();
            }
        }
        public Visibility OperationsSP
        {
            get => operationsSP;
            private set
            {
                operationsSP = value;
                Signal();
            }
        }
        public Visibility DebtsSP
        {
            get => debtsSP;
            private set
            {
                debtsSP = value;
                Signal();
            }
        }
        public Visibility IncomesSP
        {
            get => incomesSP;
            private set
            {
                incomesSP = value;
                Signal();
            }
        }
        public Visibility ExpensesSP
        {
            get => expensesSP;
            private set
            {
                expensesSP = value;
                Signal();
            }
        }
        public Visibility DepositsSP
        {
            get => depositsSP;
            private set
            {
                depositsSP = value;
                Signal();
            }
        }
        public Visibility AccountsSP
        {
            get => accountsSP;
            private set
            {
                accountsSP = value;
                Signal();
            }
        }

        private void UpdateLists(byte code)
        {
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
                    MiscSP = Visibility.Visible;
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
                    FilterSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    FilterDatesSP = Visibility.Visible;
                    FilterModeSP = Visibility.Visible;
                    TitleOfList = "Вклады";
                    Deposits = new(DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderByDescending(d => d.OpenDate));
                    break;

                case 5:
                    MiscSP = Visibility.Visible;
                    AccountsSP = Visibility.Visible;
                    FilterSP = Visibility.Visible;
                    FilterAmountSP = Visibility.Visible;
                    TitleOfList = "Счета";
                    Accounts = new(DB.GetDb(typeof(AccountsDB)).GetEntries(Search, Filter).Select(a => (Account)a).OrderByDescending(a => a.Title));
                    AccountsForFilter = [.. Accounts];
                    break;

                case 6:
                    EntriesSp = Visibility.Collapsed;
                    MainSp = Visibility.Visible;
                    Operations = [.. DB.GetDb(typeof(OperationsDB)).GetEntries("", []).Select(o => (Operation)o).OrderByDescending(o => o.TransactDate)];
                    Deposits = [.. DB.GetDb(typeof(DepositsDB)).GetEntries(Search, Filter).Select(d => (Deposit)d).OrderByDescending(d => d.OpenDate)];
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
            filterLowerDate = null;
            Signal(nameof(FilterLowerDate));
            filterUpperDate = null;
            Signal(nameof(FilterUpperDate));
            filterMinAmount = null;
            Signal(nameof(FilterMinAmount));
            filterMaxAmount = null;
            Signal(nameof(FilterMaxAmount));
            filterCategory = null;
            Signal(nameof(FilterCategory));
            filterAccount = null;
            Signal(nameof(FilterAccount));
            UpdateFilter();
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
            AccountsForFilter = [..DB.GetDb(typeof(AccountsDB)).GetEntries("", []).Select(a => (Account)a)];
            categories = [.. DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c)];
            Signal(nameof(Categories));

            filterAccount = AccountsForFilter?.FirstOrDefault(acc => acc.ID == accountId);
            filterCategory = Categories?.FirstOrDefault(c => c.ID == categoryId);
        }
    }
}
