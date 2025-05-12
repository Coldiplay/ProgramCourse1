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
    public class TypesOfDepositDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, string filter)
        {
            List<IModel> typesDeposit = new();
            if (DbConnection.GetDbConnection() == null)
                return typesDeposit;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `Id`, `Title` FROM `TypesOfDeposit`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            typesDeposit.Add(new TypeOfDeposit
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return typesDeposit;
        }

        public IModel GetSingleEntry(int id)
        {
            TypeOfDeposit typeDeposit = new();
            if (DbConnection.GetDbConnection() == null)
                return typeDeposit;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `Id`, `Title` FROM `TypesOfDeposit` WHERE `ID` = {id};"))
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
                                typeDeposit.ID = id;
                                typeDeposit.Title = dr.GetString("Title");
                            }
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return typeDeposit;
        }


        public bool Insert(IModel typeDepos, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
            // Я хз зачем я сделал это
            /*
            TypeOfDeposit typeDeposit = (TypeOfDeposit)typeDepos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `TypesOfDeposit` VALUES(0, @title); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", typeDeposit.Title));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        typeDeposit.ID = id;
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
            */
        }

        public bool Remove(IModel typeDepos, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
            // И это
            /*
            TypeOfDeposit typeDeposit = (TypeOfDeposit)typeDepos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `TypesOfDeposit` WHERE ID = {typeDeposit.ID}"))
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
            */
        }

        public bool Update(IModel typeDepos, bool changeCorrespondingEntries)
        {
            throw new NotImplementedException();
            // И это тоже
            /*
            TypeOfDeposit typeDeposit = (TypeOfDeposit)typeDepos;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `TypesOfDeposit` set `Title`=@title WHERE `ID` = {typeDeposit.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", typeDeposit.Title));

                DbConnection.GetDbConnection().OpenConnection();
                ExeptionHandler.Try(() =>
                {
                    cmd.ExecuteNonQuery();
                    result = true;
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return result;
            */
        }
    }
}
