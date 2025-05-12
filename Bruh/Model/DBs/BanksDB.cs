using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    public class BanksDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, string filter)
        {
            List<IModel> banks = new();
            if (DbConnection.GetDbConnection() == null)
                return banks;
            
            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Id`, `Title` FROM `Banks`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            banks.Add(new Bank
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return banks;
        }

        public IModel GetSingleEntry(int id)
        {
            Bank bank = new();
            if (DbConnection.GetDbConnection() == null)
                return bank;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `Id`, `Title` FROM `Banks` WHERE `ID` = {id};"))
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
                                bank.ID = dr.GetInt32("ID");
                                bank.Title = dr.GetString("Title");
                            }
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return bank;
        }

        public bool Insert(IModel ban, bool changeCorrespondingEntries)
        {
            Bank bank = (Bank)ban;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Banks` VALUES(0, @title); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", bank.Title));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        bank.ID = id;
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

        public bool Remove(IModel ban, bool changeCorrespondingEntries)
        {
            Bank bank = (Bank)ban;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Banks` WHERE ID = {bank.ID}"))
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

        public bool Update(IModel ban, bool changeCorrespondingEntries)
        {
            Bank bank = (Bank)ban;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Banks` set `Title`=@title WHERE `ID` = {bank.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", bank.Title));

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
