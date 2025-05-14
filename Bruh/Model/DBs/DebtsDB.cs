using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    public class DebtsDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filterlist)
        {
            List<IModel> debts = new();
            if (DbConnection.GetDbConnection() == null)
                return debts;

            string filter = "";
            filterlist.ForEach(f =>
            {
                if (!string.IsNullOrEmpty(f))
                    filter = $"{filter} && {f}";
            });
            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Summ`, `AnnualInterest`, `DateOfPick`, `DateOfReturn`, `CurrencyID` FROM `Debts` WHERE `Title` LIKE @search OR `Summ` LIKE @search OR `DateOfPick` LIKE @search OR `DateOfReturn` LIKE @search OR `AnnualInterest` LIKE @search {filter}"))
            {
                cmd.Parameters.Add(new MySqlParameter("search", $"%{search}%"));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            debts.Add(new Debt
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.IsDBNull("Title") ? null : dr.GetString("Title"),
                                Summ = dr.GetDecimal("Summ"),
                                AnnualInterest = dr.GetInt16("AnnualInterest"),
                                DateOfPick = dr.GetDateOnly("DateOfPick").ToDateTime(new TimeOnly()),
                                DateOfReturn = dr.GetDateOnly("DateOfReturn").ToDateTime(new TimeOnly()),
                                CurrencyID = dr.GetInt32("CurrencyID")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            debts.ForEach(deb =>
            {
                Debt debt = (Debt)deb;
                debt.Currency = (Currency)DB.GetDb(typeof(CurrencyDB)).GetSingleEntry(debt.CurrencyID);
                using (var cmd2 = DbConnection.GetDbConnection().CreateCommand($"SELECT `Cost` FROM `Operations` WHERE `DebtID`={debt.ID} AND `Income`=0;"))
                {
                    DbConnection.GetDbConnection().OpenConnection();
                    ExeptionHandler.Try(() =>
                    {
                        using (var dr = cmd2.ExecuteReader())
                        {
                            debt.PaidSumm = 0;
                            while (dr.Read())
                            {
                                debt.PaidSumm += dr.GetDecimal("Cost");
                            }
                        }                    
                    });
                    DbConnection.GetDbConnection().OpenConnection();
                }

                debt.SetCode();
            });

            return debts;
        }

        public IModel GetSingleEntry(int id)
        {
            Debt debt = new Debt();
            if (DbConnection.GetDbConnection() == null)
                return debt;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Summ`, `AnnualInterest`, `DateOfPick`, `DateOfReturn`, `CurrencyID` FROM `Debts` WHERE `ID` = {id}"))
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
                            debt.ID = id;
                            debt.Title = dr.IsDBNull("Title") ? null : dr.GetString("Title");
                            debt.Summ = dr.GetDecimal("Summ");
                            debt.AnnualInterest = dr.GetInt16("AnnualInterest");
                            debt.DateOfPick = dr.GetDateOnly("DateOfPick").ToDateTime(new TimeOnly());
                            debt.DateOfReturn = dr.GetDateOnly("DateOfReturn").ToDateTime(new TimeOnly());
                            debt.CurrencyID = dr.GetInt32("CurrencyID");
                        }
                    }
                }
                DbConnection.GetDbConnection().OpenConnection();
            }
            debt.Currency = (Currency)DB.GetDb(typeof(CurrencyDB)).GetSingleEntry(debt.CurrencyID);

            using (var cmd2 = DbConnection.GetDbConnection().CreateCommand($"SELECT `Cost` FROM `Operations` WHERE `DebtID`={debt.ID} AND `Income`=0;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (var dr = cmd2.ExecuteReader())
                    {
                        debt.PaidSumm = 0;
                        while (dr.Read())
                        {
                            debt.PaidSumm += dr.GetDecimal("Cost");
                        }
                    }
                });
                DbConnection.GetDbConnection().OpenConnection();
            }
            debt.SetCode();

            return debt;
        }

        public bool Insert(IModel deb, bool changeCorrespondingEntries)
        {
            Debt debt = (Debt)deb;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            debt.CurrencyID = debt.Currency.ID;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Debts` VALUES(0, @title, @summ, @annualInterest, @dateOfPick, @dateOfReturn,  @currencyId); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", debt.Title));
                cmd.Parameters.Add(new MySqlParameter("summ", debt.Summ));
                cmd.Parameters.Add(new MySqlParameter("annualInterest", debt.AnnualInterest));
                cmd.Parameters.Add(new MySqlParameter("dateOfPick", debt.DateOfPick));
                cmd.Parameters.Add(new MySqlParameter("dateOfReturn", debt.DateOfReturn));
                cmd.Parameters.Add(new MySqlParameter("currencyId", debt.CurrencyID));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        debt.ID = id;
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

        public bool Remove(IModel deb, bool changeCorrespondingEntries)
        {
            Debt debt = (Debt)deb;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Debts` WHERE ID = {debt.ID}"))
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

        public bool Update(IModel deb, bool changeCorrespondingEntries)
        {
            Debt debt = (Debt)deb;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            debt.CurrencyID = debt.Currency.ID;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Debts` set `Title`=@title, `Summ`=@summ, `AnnualInterest`=@annualInterest, `DateOfPick`=@dateOfPick, `DateOfReturn`=@dateOfReturn, `CurrencyID`=@currencyId WHERE `ID` = {debt.ID} ;"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", debt.Title));
                cmd.Parameters.Add(new MySqlParameter("summ", debt.Summ));
                cmd.Parameters.Add(new MySqlParameter("AnnualInterest", debt.AnnualInterest));
                cmd.Parameters.Add(new MySqlParameter("DateOfPick", debt.DateOfPick));
                cmd.Parameters.Add(new MySqlParameter("DateOfReturn", debt.DateOfReturn));
                cmd.Parameters.Add(new MySqlParameter("currencyId", debt.CurrencyID));

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
