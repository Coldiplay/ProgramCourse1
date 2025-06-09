using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    internal class BanksDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filter)
        {
            List<IModel> banks = new();
            if (DbConnection.GetDbConnection() == null)
                return banks;
            
            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Id`, `Title` FROM `Banks`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
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
                ExceptionHandler.Try(() =>
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
                ExceptionHandler.Try(() =>
                {
                    int? id = (int?)(ulong?)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        bank.ID = (int)id;
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

            // Проверка на наличие привязанных записей к банку
            bool isEverythingBad = false;
            using (var cmd1 = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID` FROM `Accounts` WHERE `BankID` = {bank.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() => isEverythingBad = (int?)cmd1.ExecuteScalar() != null);
                DbConnection.GetDbConnection().CloseConnection();                    
            }
            if (isEverythingBad)
            {
                MessageBox.Show($"Вы не можете удалить этот банк пока к нему привязаны счета. Пожалуйста, отвяжите все счета привязанные к банку '{bank.Title}'");
                return result;
            }

            using (var cmd2 = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID` FROM `Deposits` WHERE `BankID` = {bank.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() => isEverythingBad = (int?)cmd2.ExecuteScalar() != null);
                DbConnection.GetDbConnection().CloseConnection();
            }
            if (!isEverythingBad)
            {
                MessageBox.Show($"Вы не можете удалить этот банк пока к нему привязаны вклады. Пожалуйста, отвяжите все вклады привязанные к банку '{bank.Title}'");
                return result;
            }
            //

            using (var cmd3 = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Banks` WHERE ID = {bank.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    cmd3.ExecuteNonQuery();
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
                ExceptionHandler.Try(() =>
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
