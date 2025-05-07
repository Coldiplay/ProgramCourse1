using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    public class DepositsDB : ISampleDB
    {
        public List<IModel> GetEntries(string filter)
        {
            List<IModel> deposits = new();
            if (DbConnection.GetDbConnection() == null)
                return deposits;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `ID`, `Title`, `InitalSumm`, `DateOfOpening`, `DateOfClosing`, `Capitalization`, `InterestRate`, `PeriodicityOfPaymentID`, `BankID`, `TypeOfDepositID`, `CurrencyID` FROM `Deposits` WHERE `Title` LIKE @filter OR `InitalSumm` LIKE @filter OR `DateOfOpening` LIKE @filter OR `DateOfClosing` LIKE @filter"))
            {
                cmd.Parameters.Add(new MySqlParameter("filter", $"%{filter}%"));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            deposits.Add(new Deposit
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title"),
                                //Title = dr.IsDBNull("Title") ? null : dr.GetString("Title"),
                                InitalSumm = dr.GetDecimal("InitalSumm"),
                                OpenDate = dr.GetDateOnly("DateOfOpening").ToDateTime(new TimeOnly()),
                                CloseDate = dr.GetDateOnly("DateOfClosing").ToDateTime(new TimeOnly()),
                                Capitalization = dr.GetBoolean("Capitalization"),
                                InterestRate = dr.GetInt32("InterestRate"),
                                PeriodicityOfPaymentID = dr.GetInt32("PeriodicityOfPaymentID"),
                                BankID = dr.GetInt32("BankID"),
                                TypeOfDepositID = dr.GetInt32("TypeOfDepositID"),
                                CurrencyID = dr.GetInt32("CurrencyID")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            deposits.ForEach(depos =>
            {
                Deposit deposit = (Deposit)depos;
                deposit.Type = (TypeOfDeposit)DB.GetDb(typeof(TypesOfDepositDB)).GetSingleEntry(deposit.TypeOfDepositID);
                deposit.Bank = (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry(deposit.BankID);
                deposit.Currency = (Currency)DB.GetDb(typeof(CurrencyDB)).GetSingleEntry(deposit.CurrencyID);
                deposit.PeriodicityOfPayment = (PeriodicityOfPayment)DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetSingleEntry(deposit.PeriodicityOfPaymentID);

                deposit.SetCodeForChangeInPeriodicity();
            });
            return deposits;
        }

        public IModel GetSingleEntry(int id)
        {
            Deposit deposit = new Deposit();
            if (DbConnection.GetDbConnection() == null)
                return deposit;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `InitalSumm`, `DateOfOpening`, `DateOfClosing`, `Capitalization`, `InterestRate`, `PeriodicityOfPaymentID`, `BankID`, `TypeOfDepositID`, `CurrencyID` FROM `Deposits` WHERE `ID` = {id}"))
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
                            deposit.ID = id;
                            deposit.Title = dr.GetString("Title");
                            deposit.InitalSumm = dr.GetDecimal("InitalSumm");
                            deposit.OpenDate = dr.GetDateOnly("DateOfOpening").ToDateTime(new TimeOnly());
                            deposit.CloseDate = dr.GetDateOnly("DateOfClosing").ToDateTime(new TimeOnly());
                            deposit.Capitalization = dr.GetBoolean("Capitalization");
                            deposit.InterestRate = dr.GetInt32("InterestRate");
                            deposit.PeriodicityOfPaymentID = dr.GetInt32("PeriodicityOfPaymentID");
                            deposit.BankID = dr.GetInt32("BankID");
                            deposit.TypeOfDepositID = dr.GetInt32("TypeOfDepositID");
                            deposit.CurrencyID = dr.GetInt32("CurrencyID");
                        }    
                    }
                }
                DbConnection.GetDbConnection().CloseConnection();
            }

            deposit.Type = (TypeOfDeposit)DB.GetDb(typeof(TypesOfDepositDB)).GetSingleEntry(deposit.TypeOfDepositID);
            deposit.Bank = (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry(deposit.BankID);
            deposit.Currency = (Currency)DB.GetDb(typeof(CurrencyDB)).GetSingleEntry(deposit.CurrencyID);
            deposit.PeriodicityOfPayment = (PeriodicityOfPayment)DB.GetDb(typeof(PeriodicitiesOfPaymentDB)).GetSingleEntry(deposit.PeriodicityOfPaymentID);
            deposit.SetCodeForChangeInPeriodicity();
            return deposit;
        }

        public bool Insert(IModel depos, bool changeCorrespondingEntries)
        {
            Deposit deposit = (Deposit)depos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Deposits` VALUES(0, @title, @initalSumm, @dateOfOpening, @dateOfClosing, @capitalization, @interestRate, @periodicityOfPaymentId, @bankID, @typeOfDeposit, @currencyId); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", deposit.Title));
                cmd.Parameters.Add(new MySqlParameter("initalSumm", deposit.InitalSumm));
                cmd.Parameters.Add(new MySqlParameter("dateOfOpening", deposit.OpenDate));
                cmd.Parameters.Add(new MySqlParameter("dateOfClosing", deposit.CloseDate));
                cmd.Parameters.Add(new MySqlParameter("capitalization", deposit.Capitalization));
                cmd.Parameters.Add(new MySqlParameter("interestRate", deposit.InterestRate));
                cmd.Parameters.Add(new MySqlParameter("periodicityOfPaymentId", deposit.PeriodicityOfPaymentID));
                cmd.Parameters.Add(new MySqlParameter("bankID", deposit.BankID));
                cmd.Parameters.Add(new MySqlParameter("typeOfDeposit", deposit.TypeOfDepositID));
                cmd.Parameters.Add(new MySqlParameter("currencyId", deposit.CurrencyID));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        deposit.ID = id;
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

        public bool Remove(IModel depos, bool changeCorrespondingEntries)
        {
            Deposit deposit = (Deposit)depos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Deposits` WHERE ID = {deposit.ID}"))
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

        public bool Update(IModel depos, bool changeCorrespondingEntries)
        {
            Deposit deposit = (Deposit)depos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Deposits` set `Title`=@title, `InitalSumm`=@initalSumm, `DateOfOpening`=@dateOfOpening, `DateOfClosing`=@dateOfClosing, `Capitalization`=@capitalization, `InterestRate`=@interestRate, `PeriodicityOfPaymentID`=@periodicityOfPaymentId, `BankID`=@bankId, `TypeOfDepositID`=@typeOfDeposit, `CurrencyID`=@currencyId WHERE `ID`={deposit.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", deposit.Title));
                cmd.Parameters.Add(new MySqlParameter("initalSumm", deposit.InitalSumm));
                cmd.Parameters.Add(new MySqlParameter("dateOfOpening", deposit.OpenDate));
                cmd.Parameters.Add(new MySqlParameter("dateOfClosing", deposit.CloseDate));
                cmd.Parameters.Add(new MySqlParameter("capitalization", deposit.Capitalization));
                cmd.Parameters.Add(new MySqlParameter("interestRate", deposit.InterestRate));
                cmd.Parameters.Add(new MySqlParameter("periodicityOfPaymentId", deposit.PeriodicityOfPaymentID));
                cmd.Parameters.Add(new MySqlParameter("bankID", deposit.BankID));
                cmd.Parameters.Add(new MySqlParameter("typeOfDeposit", deposit.TypeOfDepositID));
                cmd.Parameters.Add(new MySqlParameter("currencyId", deposit.CurrencyID));

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
