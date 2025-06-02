using Bruh.Model.DBs;
using Bruh.VMTools;

namespace Bruh.Model.Models
{
    [DBContext(typeof(OperationsDB))]
    public class Operation : BaseVM, IModel
    {
        private bool income;
        private DateTime transactDate = DateTime.Today;

        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public DateTime TransactDate
        {
            get => transactDate;
            set
            {
                transactDate = value;
                Signal();
            }
        }
        public DateTime DateOfCreate { get; set; }
        public bool Income
        {
            get => income;
            set
            {
                income = value;
            }
        }
        public int CategoryID { get; set; }
        public int AccountID { get; set; }
        public int? DebtID { get; set; }

        public Category Category { get; set; }
        public Account Account { get; set; }
        public Debt? Debt { get; set; }

        public bool IsExpense
        {
            get => !Income;
            set
            {
                Income = !value;
            }
        }
        public string GetCost => $"{Cost} ₽";
        public string IsIncome => Income ? "Доход" : "Расход";

        public bool AllFieldsAreCorrect => !(Category == null || string.IsNullOrWhiteSpace(Title) || Cost <= 0 || Account == null);
    }
}