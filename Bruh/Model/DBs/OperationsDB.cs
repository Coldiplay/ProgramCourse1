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
    public class OperationsDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filterlist)
        {
            List<IModel> operations = new();
            if (DbConnection.GetDbConnection() == null)
                return operations;

            string filter = "";
            filterlist.ForEach(f =>
            {
                if (!string.IsNullOrEmpty(f))
                    filter = $"{filter} AND {f}";
            });

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Cost`, `TransactDate`, `DateOfCreate`, `Income`, `CategoryID`, `DebtID`, `AccountID` FROM `Operations` WHERE (`Title` LIKE @search OR `Cost` LIKE @search OR `TransactDate` LIKE @search) {filter}"))
            {
                cmd.Parameters.Add(new MySqlParameter("search", $"%{search}%"));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Operation operation = new Operation
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title"),
                                Cost = dr.GetDecimal("Cost"),
                                TransactDate = dr.GetDateTime("TransactDate"),
                                DateOfCreate = dr.GetDateTime("DateOfCreate"),
                                Income = dr.GetBoolean("Income"),
                                CategoryID = dr.GetInt32("CategoryID"),
                                DebtID = dr.IsDBNull("DebtID") ? null : dr.GetInt32("DebtID"),
                                AccountID = dr.GetInt32("AccountID")
                            };
                            operations.Add(operation);
                        }                        
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }

            operations.ForEach(oper =>
            {
                Operation operation = (Operation)oper;
                operation.Account = (Account)DB.GetDb(typeof(AccountsDB)).GetSingleEntry(operation.AccountID);
                operation.Debt = operation.DebtID == null ? null : (Debt)DB.GetDb(typeof(DebtsDB)).GetSingleEntry((int)operation.DebtID);
                operation.Category = (Category)DB.GetDb(typeof(CategoriesDB)).GetSingleEntry(operation.CategoryID);
            });
                return operations;
        }

        public IModel GetSingleEntry(int id)
        {
            Operation operation = new Operation();
            if (DbConnection.GetDbConnection() == null)
                return operation;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Cost`, `TransactDate`, `DateOfCreate`, `Income`, `CategoryID`, `DebtID`, `AccountID` FROM `Operations` WHERE `ID` = {id}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr.IsDBNull("ID"))
                            break;
                        else
                        {
                            operation.ID = id;
                            operation.Title = dr.GetString("Title");
                            operation.Cost = dr.GetDecimal("Cost");
                            operation.TransactDate = dr.GetDateTime("TransactDate");
                            operation.DateOfCreate = dr.GetDateTime("DateOfCreate");
                            operation.Income = dr.GetBoolean("Income");
                            operation.CategoryID = dr.GetInt32("CategoryID");
                            operation.DebtID = dr.IsDBNull("DebtID") ? null : dr.GetInt32("DebtID");
                            operation.AccountID = dr.GetInt32("AccountID");

                            operation.Account = (Account)DB.GetDb(typeof(AccountsDB)).GetSingleEntry(operation.AccountID);
                            operation.Debt = operation.DebtID == null ? null : (Debt)DB.GetDb(typeof(DebtsDB)).GetSingleEntry((int)operation.DebtID);
                            operation.Category = (Category)DB.GetDb(typeof(CategoriesDB)).GetSingleEntry(operation.CategoryID);
                        }    
                    }                
                }
                DbConnection.GetDbConnection().CloseConnection();
            }                
            return operation;
        }

        public bool Insert(IModel oper, bool changeCorrespondingEntries)
        {
            var operation = (Operation)oper;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            operation.DebtID = operation.Debt?.ID;
            operation.AccountID = operation.Account.ID;
            operation.CategoryID = operation.Category.ID;
            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Operations` VALUES(0, @title, @cost, @transactDate, @DateOfCreate, @income, @categotyId, @debtId, @accountId); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", operation.Title));
                cmd.Parameters.Add(new MySqlParameter("cost", operation.Cost));
                cmd.Parameters.Add(new MySqlParameter("transactDate", operation.TransactDate));
                cmd.Parameters.Add(new MySqlParameter("DateOfCreate", operation.DateOfCreate));
                cmd.Parameters.Add(new MySqlParameter("income", operation.Income));
                cmd.Parameters.Add(new MySqlParameter("categotyId", operation.CategoryID));
                cmd.Parameters.Add(new MySqlParameter("debtId", operation.DebtID));
                cmd.Parameters.Add(new MySqlParameter("accountId", operation.AccountID));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int? id = (int?)(ulong?)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        operation.ID = (int)id;
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("Запись не добавлена");
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }

            if (changeCorrespondingEntries)
            {
                ChangeSummOnAccount(operation, operation.Income ? operation.Cost : operation.Cost * -1);
            }
            return result;
        }

        public bool Remove(IModel oper, bool changeCorrespondingEntries)
        {
            var operation = (Operation)oper;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Operations` WHERE ID = {operation.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }

            if (changeCorrespondingEntries)
                ChangeSummOnAccount(operation, operation.Income ? operation.Cost * -1 : operation.Cost);

            return result;
        }

        public bool Update(IModel oper, bool changeCorrespondingEntries)
        {
            var operation = (Operation)oper;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            operation.DebtID = operation.Debt?.ID == 0 ? null : operation.Debt?.ID;
            operation.AccountID = operation.Account.ID;
            operation.CategoryID = operation.Category.ID;
            if (changeCorrespondingEntries)
            {
                decimal? cost = 0;
                using (var cmd2 = DbConnection.GetDbConnection().CreateCommand("SELECT `Cost` FROM `Operations` WHERE `ID`=@id"))
                {
                    cmd2.Parameters.Add(new MySqlParameter("id", operation.ID));
                    DbConnection.GetDbConnection().OpenConnection();
                    ExeptionHandler.Try(() => cost = (decimal?)cmd2.ExecuteScalar());
                    DbConnection.GetDbConnection().CloseConnection();
                }
                ChangeSummOnAccount(operation, operation.Cost != (decimal)cost ? operation.Cost - (decimal)cost : operation.Cost);
            }

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Operations` set `Title`=@title, `Cost`=@cost, `TransactDate`=@transactDate, `DateOfCreate`=@dateOfCreate, `Income`=@income, `CategoryID`=@categoryId, `DebtID`=@debtId, `AccountID`=@AccountId WHERE `ID`={operation.ID} ;"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", operation.Title));
                cmd.Parameters.Add(new MySqlParameter("cost", operation.Cost));
                cmd.Parameters.Add(new MySqlParameter("transactDate", operation.TransactDate));
                cmd.Parameters.Add(new MySqlParameter("DateOfCreate", operation.DateOfCreate));
                cmd.Parameters.Add(new MySqlParameter("income", operation.Income));
                cmd.Parameters.Add(new MySqlParameter("categoryId", operation.CategoryID));
                cmd.Parameters.Add(new MySqlParameter("debtId", operation.DebtID));
                cmd.Parameters.Add(new MySqlParameter("AccountId", operation.AccountID));

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

        private static void ChangeSummOnAccount(Operation operation ,decimal summ)
        {
            if (summ == 0)
                return;

            decimal? balance = 0;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Balance` FROM `Accounts` WHERE `ID`=@idAccount"))
            {
                cmd.Parameters.Add(new MySqlParameter("idAccount", operation.AccountID));
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() => balance = (decimal?)cmd.ExecuteScalar());
                DbConnection.GetDbConnection().CloseConnection();
            }

            using (var cmd2 = DbConnection.GetDbConnection().CreateCommand("UPDATE `Accounts` set `Balance`=@balance WHERE `ID`=@id"))
            {
                cmd2.Parameters.Add(new MySqlParameter("balance", operation.Income ? (decimal)balance + summ : (decimal)balance - summ));
                cmd2.Parameters.Add(new MySqlParameter("id", operation.AccountID));
                DbConnection.GetDbConnection().OpenConnection();
                if (!ExeptionHandler.Try(() => cmd2.ExecuteNonQuery()))
                    MessageBox.Show("Внимание! Не удалось изменить соответствующий счёт");
                DbConnection.GetDbConnection().CloseConnection();
            }
        }
    }
}
