using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    internal class CategoriesWindowVM : BaseVM
    {
        private Category? selectedCategory;

        public ObservableCollection<Category> Categories { get; private set; } = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c));
        public Category? SelectedCategory
        {
            get => selectedCategory;
            set
            {
                selectedCategory = value;
                Signal();
            }
        }

        public ICommand AddCategory { get; private set; }
        public ICommand DeleteCategory { get; private set; }
        public ICommand EditCategory { get; private set; }

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

        private void UpdateList()
        {
            Categories = new(DB.GetDb(typeof(CategoriesDB)).GetEntries("", []).Select(c => (Category)c));
            Signal(nameof(Categories));
        }
        
    }
}
