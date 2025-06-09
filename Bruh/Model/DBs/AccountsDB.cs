using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    internal class AccountsDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filterList)
        {
            List<IModel> accounts = new();
            if (DbConnection.GetDbConnection() == null)
                return accounts;

            string filter = "";
            filterList.ForEach(f =>
            {
                if (!string.IsNullOrEmpty(f))
                    filter = $"{filter} AND {f}";
            });

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT a.`ID`, a.`Title`, a.`Balance`, a.`BankID` FROM `Accounts` a LEFT JOIN `Banks` b ON a.`BankID` = b.`ID` WHERE (`a`.`Title` LIKE @search OR `a`.`Balance` LIKE @search OR `b`.`Title` LIKE @search) {filter};"))
            {
                    cmd.Parameters.Add(new MySqlParameter("search", $"%{search}%"));

                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var account = new Account
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title"),
                                Balance = dr.GetDecimal("Balance"),
                                BankID = dr.IsDBNull("BankID") ? null : dr.GetInt32("BankID")
                            };
                            accounts.Add(account);
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }

            accounts.ForEach(acc =>
            {
                Account account = (Account)acc;
                account.Bank = account.BankID == null ? null : (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry((int)account.BankID);
            });

            return accounts;
        }

        public IModel GetSingleEntry(int id)
        {
            Account account = new Account();
            if (DbConnection.GetDbConnection() == null)
                return account;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Balance`, `BankID` FROM `Accounts` WHERE `ID` = {id}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr.IsDBNull("ID"))
                            break;

                        account.ID = dr.GetInt32("ID");
                        account.Title = dr.GetString("Title");
                        account.Balance = dr.GetDecimal("Balance");
                        account.BankID = dr.IsDBNull("BankID") ? null : dr.GetInt32("BankID");
                    }
                }
                DbConnection.GetDbConnection().CloseConnection();
            }

            account.Bank = account.BankID == null ? null : (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry((int)account.BankID);

            return account;
        }

        public bool Insert(IModel acc, bool changeCorrespondingEntries)
        {
            Account account = (Account)acc;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            account.BankID = account.Bank?.ID == 0 ? null : account.Bank?.ID;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Accounts` VALUES(0, @title, @balance, @bankId); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", account.Title));
                cmd.Parameters.Add(new MySqlParameter("balance", account.Balance));
                cmd.Parameters.Add(new MySqlParameter("bankId", account.BankID));

                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    int? id = (int?)(ulong?)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        account.ID = (int)id;
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

        public bool Remove(IModel acc, bool changeCorrespondingEntries)
        {
            Account account = (Account)acc;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            bool isEverythingBad = false;
            using (var cmd1 = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID` FROM `Operations` WHERE `AccountID` = {account.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() => isEverythingBad = (int?)cmd1.ExecuteScalar() != null);
                DbConnection.GetDbConnection().CloseConnection();
            }
            if (isEverythingBad)
            {
                MessageBox.Show($"Вы не можете удалить этот счёт пока к нему привязаны операции.");
                return result;
            }

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Accounts` WHERE ID = {account.ID}"))
            {
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

        public bool Update(IModel acc, bool changeCorrespondingEntries)
        {
            Account account = (Account)acc;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            account.BankID = account.Bank?.ID;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Accounts` set `Title`=@title, `Balance`=@balance, `BankID`=@bankId WHERE `ID` = {account.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", account.Title));
                cmd.Parameters.Add(new MySqlParameter("balance", account.Balance));
                cmd.Parameters.Add(new MySqlParameter("bankID", account.BankID));

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
