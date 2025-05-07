using Bruh.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.DBs
{
    //public class Test
    //{
    //    public void Main()
    //    {
    //        DB<DbOperation>.GetDb().Insert();
    //    }
    //}

    public interface ISampleDB
    {
        public bool Insert(IModel obj, bool changeCorrespondingEntries);
        public bool Update(IModel obj, bool changeCorrespondingEntries);
        public bool Remove(IModel obj, bool changeCorrespondingEntries);
        public List<IModel> GetEntries(string filter);
        public IModel GetSingleEntry(int id);
    }

    //public class DB<T> where T : ISampleDB, new()
    //{
    //    static List<ISampleDB> sampleDBs = new List<ISampleDB>();

    //    public static ISampleDB GetDb()
    //    {
    //        ISampleDB db = sampleDBs.FirstOrDefault(s => s.GetType() == typeof(T));
    //        if (db == null)
    //        {
    //            db = new T();
    //            sampleDBs.Add(db);
    //        }
    //        return db;
    //    }
    //}

    public class DB
    {
        static List<ISampleDB> sampleDBs = new List<ISampleDB>();
        public static ISampleDB GetDb(Type type)
        {
            ISampleDB db = sampleDBs.FirstOrDefault(s => s.GetType() == type);
            if (db == null)
            {
                db = Activator.CreateInstance(type) as ISampleDB;
                sampleDBs.Add(db);
            }
            return db;
        }
    }
}
