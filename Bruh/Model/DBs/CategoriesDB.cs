using Bruh.Model.Models;
using Bruh.VMTools;
using MySqlConnector;
using System.Data;
using System.Windows;

namespace Bruh.Model.DBs
{
    internal class CategoriesDB : ISampleDB
    {
        public List<IModel> GetEntries(string search, List<string> filter)
        {
            List<IModel> categories = new();
            if (DbConnection.GetDbConnection() == null)
                return categories;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand("SELECT `ID`, `Title` FROM `Categories`;"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            categories.Add(new Category
                            {
                                ID = dr.GetInt32("ID"),
                                Title = dr.GetString("Title")
                            });
                        }
                    }
                });
                DbConnection.GetDbConnection().CloseConnection();
            }
            return categories;
        }

        public IModel GetSingleEntry(int id)
        {
            Category category = new();
            if (DbConnection.GetDbConnection() == null)
                return category;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID`, `Title` FROM `Categories` WHERE `ID` = {id}"))
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
                            category.ID = id;
                            category.Title = dr.GetString("Title");                        
                        }
                    }
                }
                DbConnection.GetDbConnection().OpenConnection();
            }
            return category;
        }

        public bool Insert(IModel categ, bool changeCorrespondingEntries)
        {
            Category category = (Category)categ;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (MySqlCommand cmd = DbConnection.GetDbConnection().CreateCommand("INSERT INTO `Categories` VALUES(0, @title); SELECT LAST_INSERT_ID();"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", category.Title));

                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() =>
                {
                    int? id = (int?)(ulong?)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        category.ID = (int)id;
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

        public bool Remove(IModel categ, bool changeCorrespondingEntries)
        {
            Category category = (Category)categ;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;


            //Проверка на привязку категории к операциям
            bool IsBad = false;
            using (var cmd1 = DbConnection.GetDbConnection().CreateCommand($"SELECT `ID` FROM `Operations` WHERE `CategoryID` = {category.ID}"))
            {
                DbConnection.GetDbConnection().OpenConnection();
                ExceptionHandler.Try(() => IsBad = (int?)cmd1.ExecuteScalar() != null);
                DbConnection.GetDbConnection().CloseConnection();
            }
            if (IsBad)
            {
                MessageBox.Show("Нельзя удалить категорию операций, если к ней привязаны операции. Пожалуйста, отвяжите категорию от операций.");
                return result;
            }
            //

            using (var cmd3 = DbConnection.GetDbConnection().CreateCommand($"DELETE FROM `Categories` WHERE ID = {category.ID}"))
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

        public bool Update(IModel categ, bool changeCorrespondingEntries)
        {
            Category category = (Category)categ;
            bool result = false;
            if (DbConnection.GetDbConnection() == null)
                return result;

            using (var cmd = DbConnection.GetDbConnection().CreateCommand($"UPDATE `Categories` set `Title`=@title WHERE `ID` = {category.ID};"))
            {
                cmd.Parameters.Add(new MySqlParameter("title", category.Title));

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
