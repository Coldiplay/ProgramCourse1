using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    public class CategoriesWindowVM : BaseVM
    {
        public ObservableCollection<Category> Categories { get; set; } = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("","").Select(c => (Category)c));
        public Category? SelectedCategory { get; set; }

        public ICommand AddCategory { get; set; }
        public ICommand DeleteCategory { get; set; }
        public ICommand EditCategory { get; set; }

        public CategoriesWindowVM()
        { 
            AddCategory = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(new Category(), add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => true);

            DeleteCategory = new CommandVM(() =>
            {
                if (MessageBox.Show("Вы точно хотите удалить выбранную запись?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //bool change = MessageBox.Show("", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    DB.GetDb(typeof(CategoriesDB)).Remove(SelectedCategory, false/*change*/);
                    UpdateList();
                }
            }, () => SelectedCategory != null);

            EditCategory = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(SelectedCategory, add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => SelectedCategory != null);
        }

        public void UpdateList()
        {
            Categories = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("","").Select(c => (Category)c));
            Signal(nameof(Categories));
        }
        
    }
}
