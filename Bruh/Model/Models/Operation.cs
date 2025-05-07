using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(OperationsDB))]
    public class Operation : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public decimal Cost { get; set; }
        public DateTime TransactDate { get; set; }
        public DateTime DateOfCreate { get; set; }
        public bool Income { get; set; }
        public int CategoryID { get; set; }
        public int AccountID { get; set; }
        // Потом либо сделать баланс на момент операции через пересчёт всех операций до этой, либо бросить это дело
        //public decimal AccountBalance { get; set; }
        public string? Description { get; set; }
        public int? DebtID { get; set; }
        public int? PeriodicityID { get; set; }

        public Category Category { get; set; }
        public Account Account { get; set; }
        public Debt? Debt { get; set; }
        public Periodicity? Periodicity { get; set; }

        public string GetCost 
        {
            get
            {
                if (Account == null || Account.Currency == null)
                    return $"{Cost}";
                else
                    return $"{Cost} {Account.Currency.Symbol}";
                
            }
        }
        public string IsIncome
        {
            get
            {
                if (Income)
                    return "Доход";
                return "Расход";
            }
        }
        public string GetPeriodicity
        {
            get
            {
                if (Periodicity == null)
                    return "";
                else
                    return Periodicity.Name;
            }
        }
    }
}