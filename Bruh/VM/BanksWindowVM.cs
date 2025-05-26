using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    internal class BanksWindowVM : BaseVM
    {
        private Bank? selectedBank;

        public ObservableCollection<Bank> Banks { get; private set; } = new(DB.GetDb(typeof(BanksDB)).GetEntries("", []).Select(b => (Bank)b));
        public Bank? SelectedBank
        {
            get => selectedBank;
            set
            {
                selectedBank = value;
                Signal();
            }
        }

        public ICommand AddBank { get; private set; }
        public ICommand DeleteBank { get; private set; }
        public ICommand EditBank { get; private set; }

        public BanksWindowVM()
        {
            AddBank = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(new Bank(), add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => true);

            DeleteBank = new CommandVM(() =>
            {
                if (MessageBox.Show("Вы точно хотите удалить выбранную запись?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DB.GetDb(typeof(BanksDB)).Remove(SelectedBank, false);
                    UpdateList();
                }
            }, () => SelectedBank != null);

            EditBank = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(SelectedBank, add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => SelectedBank != null);
        }

        private void UpdateList()
        {
            Banks = new(DB.GetDb(typeof(BanksDB)).GetEntries("", []).Select(b => (Bank)b));
            Signal(nameof(Banks));
        }
    }
}
