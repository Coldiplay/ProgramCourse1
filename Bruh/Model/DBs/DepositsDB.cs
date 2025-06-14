﻿using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    internal class DepositsDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filterlist)
        {
            List<IModel> deposits = new();
            if (DbConnection.GetDbConnection() == null)
                return deposits;

            string filter = "";
            filterlist.ForEach(f =>
            {
                if (!string.IsNullOrEmpty(f))
                    filter = $"{filter} AND {f}";
            });
            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `InitalSumm`, `DateOfOpening`, `DateOfClosing`, `Capitalization`, `InterestRate`, `PeriodicityOfPaymentID`, `BankID` FROM `Deposits` WHERE (`Title` LIKE @search OR `InitalSumm` LIKE @search OR `DateOfOpening` LIKE @search OR `DateOfClosing` LIKE @search) {filter}"))
            {
                cmd.Parameters.Add(new MySqlParameter("search", $"%{search}%"));

                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            deposits.Add(new Deposit
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title"),
                                InitalSumm = dr.GetDecimal("InitalSumm"),
                                OpenDate = dr.GetDateOnly("DateOfOpening").ToDateTime(new TimeOnly()),
                                CloseDate = dr.GetDateOnly("DateOfClosing").ToDateTime(new TimeOnly()),
                                Capitalization = dr.GetBoolean("Capitalization"),
                                InterestRate = dr.GetInt32("InterestRate"),
                                PeriodicityOfPaymentID = dr.GetInt32("PeriodicityOfPaymentID"),
                                BankID = dr.GetInt32("BankID")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            deposits.ForEach(depos =>
            {
                Deposit deposit = (Deposit)depos;
                deposit.Bank = (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry(deposit.BankID);
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

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title`, `InitalSumm`, `DateOfOpening`, `DateOfClosing`, `Capitalization`, `InterestRate`, `PeriodicityOfPaymentID`, `BankID` FROM `Deposits` WHERE `ID` = {id}"))
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
                        }    
                    }
                }
                DbConnection.GetDbConnection().CloseConnection();
            }

            deposit.Bank = (Bank)DB.GetDb(typeof(BanksDB)).GetSingleEntry(deposit.BankID);
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

            deposit.BankID = deposit.Bank.ID;
            deposit.PeriodicityOfPaymentID = deposit.PeriodicityOfPayment.ID;
            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Deposits` VALUES(0, @title, @initalSumm, @dateOfOpening, @dateOfClosing, @capitalization, @interestRate, @periodicityOfPaymentId, @bankID); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", deposit.Title));
                cmd.Parameters.Add(new MySqlParameter("initalSumm", deposit.InitalSumm));
                cmd.Parameters.Add(new MySqlParameter("dateOfOpening", deposit.OpenDate));
                cmd.Parameters.Add(new MySqlParameter("dateOfClosing", deposit.CloseDate));
                cmd.Parameters.Add(new MySqlParameter("capitalization", deposit.Capitalization));
                cmd.Parameters.Add(new MySqlParameter("interestRate", deposit.InterestRate));
                cmd.Parameters.Add(new MySqlParameter("periodicityOfPaymentId", deposit.PeriodicityOfPaymentID));
                cmd.Parameters.Add(new MySqlParameter("bankID", deposit.BankID));

                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    int? id = (int?)(ulong?)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        deposit.ID = (int)id;
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
                ExceptionHandler.Try(() =>
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

            deposit.BankID = deposit.Bank.ID;
            deposit.PeriodicityOfPaymentID = deposit.PeriodicityOfPayment.ID;
            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Deposits` set `Title`=@title, `InitalSumm`=@initalSumm, `DateOfOpening`=@dateOfOpening, `DateOfClosing`=@dateOfClosing, `Capitalization`=@capitalization, `InterestRate`=@interestRate, `PeriodicityOfPaymentID`=@periodicityOfPaymentId, `BankID`=@bankId, WHERE `ID`={deposit.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", deposit.Title));
                cmd.Parameters.Add(new MySqlParameter("initalSumm", deposit.InitalSumm));
                cmd.Parameters.Add(new MySqlParameter("dateOfOpening", deposit.OpenDate));
                cmd.Parameters.Add(new MySqlParameter("dateOfClosing", deposit.CloseDate));
                cmd.Parameters.Add(new MySqlParameter("capitalization", deposit.Capitalization));
                cmd.Parameters.Add(new MySqlParameter("interestRate", deposit.InterestRate));
                cmd.Parameters.Add(new MySqlParameter("periodicityOfPaymentId", deposit.PeriodicityOfPaymentID));
                cmd.Parameters.Add(new MySqlParameter("bankID", deposit.BankID));

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
