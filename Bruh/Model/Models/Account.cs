using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(AccountsDB))]
    public class Account : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int? BankID { get; set; }
        public Bank? Bank { get; set; }

        public string GetBalance => $"{Balance} рублей";

        public bool AllFieldsAreCorrect => !(string.IsNullOrWhiteSpace(Title));
    }
}
