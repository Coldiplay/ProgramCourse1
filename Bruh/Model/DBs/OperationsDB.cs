using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    internal class OperationsDB : ISampleDB
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
                ExceptionHandler.Try(() =>
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


            // Проверка на наличие нужной суммы на счету при добавлении расхода
            if (changeCorrespondingEntries && !operation.Income && GetBalance(operation.AccountID) < operation.Cost)
            {
                MessageBox.Show("На счету недостаточно средств для операции", "", MessageBoxButton.OK);
                return true;
            }

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
                ExceptionHandler.Try(() =>
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
                ChangeSummOnAccount(operation.AccountID, operation.Income , 0, operation.Cost);
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
                ExceptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }

            if (changeCorrespondingEntries)
            {
                if (operation.Income && GetBalance(operation.AccountID) - operation.Cost < 0)
                    MessageBox.Show("На счету недостаточно средств для изменения баланса. Баланс счёта останется не тронутым", "", MessageBoxButton.OK);
                else
                    ChangeSummOnAccount(operation.AccountID, operation.Income, operation.Cost, 0);
            }

            return result;
        }

        public bool Update(IModel oper, bool changeCorrespondingEntries)
        {
            var operation = (Operation)oper;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            if (changeCorrespondingEntries)
            {
                decimal cost = 0;
                bool income = operation.Income;
                using (var cmd2 = DbConnection.GetDbConnection().CreateCommand("SELECT `Cost`, `Income` FROM `Operations` WHERE `ID`=@id"))
                {
                    cmd2.Parameters.Add(new MySqlParameter("id", operation.ID));
                    DbConnection.GetDbConnection().OpenConnection();
                    using (var dr = cmd2.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cost = dr.IsDBNull("Cost") ? 0 : dr.GetDecimal("Cost");
                            income = dr.IsDBNull("Income") ? income : dr.GetBoolean("Income");
                        }
                    }
                    DbConnection.GetDbConnection().CloseConnection();
                }
                if (operation.Account.ID > 0 && operation.AccountID != operation.Account.ID)
                    ChangeSummOnAccount(operation.AccountID, operation.Account.ID, operation.Income, cost , income == operation.Income ? operation.Cost : -operation.Cost);
                else
                    ChangeSummOnAccount(operation.AccountID, operation.Income, cost, operation.Cost);
            }

            operation.DebtID = operation.Debt?.ID == 0 ? null : operation.Debt?.ID;
            operation.AccountID = operation.Account.ID;
            operation.CategoryID = operation.Category.ID;

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
                ExceptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
                return result;
        }

        // Только при изменении счёта у операции
        private static bool ChangeSummOnAccount(int prevAccId, int currAccId, bool isIncome ,decimal prevSumm, decimal curSumm)
        {
            decimal? balance = 0;
            decimal? balance2 = 0;

            balance = GetBalance(prevAccId);
            balance2 = GetBalance(currAccId);
            decimal summForChangeAcc1;
            decimal summForChangeAcc2;

            if (isIncome)
            {
                summForChangeAcc1 = -prevSumm;
                summForChangeAcc2 = curSumm;
                if (balance - summForChangeAcc1 < 0)
                {
                    switch (MessageBox.Show("Внимание! На старом счету недостаточно средств для изменения привязки операции к счёту.\nПри отмене операции изменения не будут внесены.\nПродолжить изменение балансов счетов? (Баланс старого счёта будет равен 0)", "", MessageBoxButton.YesNoCancel))
                    {
                        case MessageBoxResult.Yes:
                            summForChangeAcc1 = -(decimal)balance;
                            break;
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.Cancel:
                            return false;
                    }
                }
            }
            else
            {
                summForChangeAcc1 = prevSumm;
                summForChangeAcc2 = -curSumm;
                if (balance2 - summForChangeAcc2 < 0)
                {
                    switch (MessageBox.Show("Внимание! На новом счету недостаточно средств для изменения привязки операции к счёту.\nПри отмене операции изменения не будут внесены.\nПродолжить изменение балансов счетов? (Баланс нового счёта будет равен 0)", "", MessageBoxButton.YesNoCancel))
                    {
                        case MessageBoxResult.Yes:
                            summForChangeAcc2 = -(decimal)balance2;
                            break;
                        case MessageBoxResult.No:
                            return true;
                        case MessageBoxResult.Cancel:
                            return false;
                    }
                }
            }

            using (var cmd3 = DbConnection.GetDbConnection().CreateCommand("UPDATE `Accounts` set `Balance`=@acc1balance WHERE `ID`=@prevAccId ; UPDATE `Accounts` set `Balance`=@acc2balance WHERE `ID`=@currAccId"))
            {
                cmd3.Parameters.Add(new MySqlParameter("acc1balance", balance + summForChangeAcc1));
                cmd3.Parameters.Add(new MySqlParameter("prevAccId", prevAccId));
                cmd3.Parameters.Add(new("currAccId", currAccId));
                cmd3.Parameters.Add(new("acc2balance", balance2 + summForChangeAcc2));
                DbConnection.GetDbConnection().OpenConnection();
                if (!ExceptionHandler.Try(() => cmd3.ExecuteNonQuery()))
                    MessageBox.Show("Внимание! Не удалось изменить счета");
                DbConnection.GetDbConnection().CloseConnection();
            }

            return true;
        }
        private static void ChangeSummOnAccount(int accountId, bool isIncome, decimal prevSumm, decimal curSumm)
        {
            decimal balance = GetBalance(accountId);
            decimal changedBalance = isIncome ? balance + (curSumm - prevSumm) : balance - (curSumm - prevSumm);
            using (var cmd2 = DbConnection.GetDbConnection().CreateCommand("UPDATE `Accounts` set `Balance`=@balance WHERE `ID`=@id"))
            {
                cmd2.Parameters.Add(new MySqlParameter("balance", changedBalance));
                cmd2.Parameters.Add(new MySqlParameter("id", accountId));
                DbConnection.GetDbConnection().OpenConnection();
                if (!ExceptionHandler.Try(() => cmd2.ExecuteNonQuery()))
                    MessageBox.Show("Внимание! Не удалось изменить соответствующий счёт");
                DbConnection.GetDbConnection().CloseConnection();
            }
        }
        private static decimal GetBalance(int accountId)
        {
            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Balance` FROM `Accounts` WHERE `ID`=@idAccount"))
            {
                cmd.Parameters.Add(new MySqlParameter("idAccount", accountId));
                decimal? balance = 0;
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() => balance = (decimal?)cmd.ExecuteScalar());
                DbConnection.GetDbConnection().CloseConnection();
                return balance == null ? 0 : (decimal)balance;
            }
        }
    }
}
