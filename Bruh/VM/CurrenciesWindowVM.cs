using Bruh.Model.DBs;
using Bruh.Model.Models;
using Bruh.View;
using Bruh.VMTools;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Bruh.VM
{
    internal class CurrenciesWindowVM : BaseVM
    {
        private Currency? selectedCurrency;

        public ObservableCollection<Currency> Currencies { get; private set; } = new(DB.GetDb(typeof(CurrencyDB)).GetEntries("", []).Select(c => (Currency)c));
        public Currency? SelectedCurrency
        {
            get => selectedCurrency;
            set
            {
                selectedCurrency = value;
                Signal();
            }
        }

        public ICommand AddCurrency { get; private set; }
        public ICommand DeleteCurrency { get; private set; }
        public ICommand EditCurrency { get; private set; }

        public CurrenciesWindowVM()
        {
            AddCurrency = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(new Currency(), add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => true);

            DeleteCurrency = new CommandVM(() =>
            {
                if (MessageBox.Show("Вы точно хотите удалить выбранную запись?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DB.GetDb(typeof(CurrencyDB)).Remove(SelectedCurrency, false);
                    UpdateList();
                }
            }, () => SelectedCurrency != null);

            EditCurrency = new CommandVM(() =>
            {
                var add = new EditWindow();
                ((EditWindowVM)add.DataContext).Set(SelectedCurrency, add.Close);
                add.ShowDialog();
                UpdateList();
            }, () => SelectedCurrency != null);
        }

        private void UpdateList()
        {
            Currencies = new(DB.GetDb(typeof(CurrencyDB)).GetEntries("", []).Select(c => (Currency)c));
            Signal(nameof(Currencies));
        }
    }
}
