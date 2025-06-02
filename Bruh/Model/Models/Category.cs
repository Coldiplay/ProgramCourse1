using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(CategoriesDB))]
    public class Category : IModel
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;

        public bool AllFieldsAreCorrect => !string.IsNullOrWhiteSpace(Title);
    }
}
