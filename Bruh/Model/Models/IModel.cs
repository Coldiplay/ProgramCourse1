using Bruh.Model.DBs;

namespace Bruh.Model.Models
{
    [DBContext(typeof(ISampleDB))]
    public interface IModel
    {
        public int ID { get; internal set; }
        public bool AllFieldsAreCorrect { get; }
    }
}
