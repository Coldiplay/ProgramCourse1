using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(BanksDB))]
    public class Bank : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;

        public bool AllFieldsAreCorrect => !string.IsNullOrWhiteSpace(Title);
    }
}
