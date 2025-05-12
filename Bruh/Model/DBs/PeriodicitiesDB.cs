using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bruh.Model.DBs
{
    public class PeriodicitiesDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, string filter)
        {
            List<IModel> periodicities = new();
            if (DbConnection.GetDbConnection() == null)
                return periodicities;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Id`, `Value` FROM `Periodicities`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            periodicities.Add(new Periodicity
                            {
                                ID = dr.GetInt32("Id"),
                                Name = dr.GetString("Value")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return periodicities;
        }

        public IModel GetSingleEntry(int id)
        {
            Periodicity periodicity = new();
            if (DbConnection.GetDbConnection() == null)
                return periodicity;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `Id`, `Value` FROM `Periodicities` WHERE `ID` = {id};"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr.IsDBNull("ID"))
                                break;
                            else
                            {
                                periodicity.ID = dr.GetInt32("ID");
                                periodicity.Name = dr.GetString("Value");
                            }                            
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return periodicity;
        }


        public bool Insert(IModel period, bool changeCorrespondingEntries)
        {
            Periodicity periodicity = (Periodicity)period;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Periodicities` VALUES(0, @value); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("value", periodicity.Name));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        periodicity.ID = id;
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("Запись не добавлена");
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return result;
        }

        public bool Remove(IModel period, bool changeCorrespondingEntries)
        {
            Periodicity periodicity = (Periodicity)period;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Periodicities` WHERE ID = {periodicity.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return result;
        }

        public bool Update(IModel period, bool changeCorrespondingEntries)
        {
            Periodicity periodicity = (Periodicity)period;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Periodicities` set `Value`=@value WHERE `ID` = {periodicity.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("value", periodicity.Name));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return result;
        }
    }
}
