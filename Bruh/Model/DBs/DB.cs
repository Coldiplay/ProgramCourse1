namespace Bruh.Model.DBs
{
    internal class DB
    {
        static List<ISampleDB> sampleDBs = new List<ISampleDB>();
        internal static ISampleDB GetDb(Type type)
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
