using Bruh.Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bruh.Model.DBs
{
    public class PeriodicitiesOfPaymentDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filter)
        {
            List<IModel> periodicities = new();
            if (DbConnection.GetDbConnection() == null)
                return periodicities;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("Select `ID`, `Name` FROM `PeriodicitiesOfPayment`"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        periodicities.Add(new PeriodicityOfPayment 
                        {
                            ID = dr.GetInt32("ID"),
                            Name = dr.GetString("Name")
                        });                    
                    }
                }
                
            }
            return periodicities;
        }

        public IModel GetSingleEntry(int id)
        {
            PeriodicityOfPayment periodicity = new();
            if (DbConnection.GetDbConnection() == null)
                return periodicity;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"Select `ID`, `Name` FROM `PeriodicitiesOfPayment` WHERE `ID`={id}; "))
            {
                DbConnection.GetDbConnection().OpenConnection();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr.IsDBNull("ID"))
                            break;
                        periodicity.ID = dr.GetInt32("ID");
                        periodicity.Name = dr.GetString("Name");
                    }
                }
            }
            return periodicity;
        }

        public bool Insert(IModel obj, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IModel obj, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
        }

        public bool Update(IModel obj, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
        }
    }
}
