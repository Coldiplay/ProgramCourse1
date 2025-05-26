using Bruh.Model.Models;

namespace Bruh.Model.DBs
{
    internal interface ISampleDB
    {
        internal bool Insert(IModel obj, bool changeCorrespondingEntries);
        internal bool Update(IModel obj, bool changeCorrespondingEntries);
        internal bool Remove(IModel obj, bool changeCorrespondingEntries);
        internal List<IModel> GetEntries(string search, List<string> filter);
        internal IModel GetSingleEntry(int id);
    }
}
