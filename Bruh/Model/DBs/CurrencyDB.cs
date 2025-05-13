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
    public class CurrencyDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filter)
        {
            List<IModel> currencies = new();
            if (DbConnection.GetDbConnection() == null)
                return currencies;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Id`, `Title`, `Symbol` FROM `Currencies`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            currencies.Add(new Currency
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title"),
                                Symbol = dr.GetChar("Symbol")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return currencies;
        }

        public IModel GetSingleEntry(int id)
        {
            Currency currency = new Currency();
            if (DbConnection.GetDbConnection() == null)
                return currency;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `Symbol` FROM `Currencies` WHERE `ID` = {id}"))
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
                            currency.ID = id;
                            currency.Title = dr.GetString("Title");
                            currency.Symbol = dr.GetChar("Symbol");
                        }
                    }
                }
                DbConnection.GetDbConnection().CloseConnection();
            }
            return currency;
        }


        public bool Insert(IModel curr, bool changeCorrespondingEntries)
        {
            Currency currency = (Currency)curr;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Currencies` VALUES(0, @title, @symbol); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", currency.Title));
                cmd.Parameters.Add(new MySqlParameter("symbol", currency.Symbol));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        currency.ID = id;
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

        public bool Remove(IModel curr, bool changeCorrespondingEntries)
        {
            Currency currency = (Currency)curr;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Currencies` WHERE ID = {currency.ID}"))
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

        public bool Update(IModel curr, bool changeCorrespondingEntries)
        {
            Currency currency = (Currency)curr;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Currencies` set `Title`=@title, `Symbol`=@symbol WHERE `ID` = {currency.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", currency.Title));
                cmd.Parameters.Add(new MySqlParameter("symbol", currency.Symbol));

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
