using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(PeriodicitiesOfPaymentDB))]
    public class PeriodicityOfPayment : IModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;

        public bool AllFieldsAreCorrect => !string.IsNullOrWhiteSpace(Name);
    }
}
