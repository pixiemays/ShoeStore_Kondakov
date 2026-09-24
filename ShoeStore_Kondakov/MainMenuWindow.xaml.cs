using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShoeStore_Kondakov
{
    /// <summary>
    /// Interaction logic for MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private List<Product> searchList = ShoeStoreEntities.GetContext().Products.ToList();
        public MainMenuWindow(User user)
        {
            InitializeComponent();

            ProductsList.ItemsSource = searchList;
            if (user.FIO == null)
                FIO.Text = "Гость";
            else FIO.Text = user.FIO;

            supplierBox.ItemsSource = ShoeStoreEntities.GetContext().Supplier.ToList();
        }

        private void Filter()
        {
            if (searchBox.Text != "")
                searchList = searchList.Where(x => x.Description.ToLower().Contains(searchBox.Text.ToLower())).ToList();

            if (supplierBox.SelectedIndex != -1)
                searchList = searchList.Where(x => x.SupplierId == supplierBox.SelectedIndex).ToList();

            ProductsList.ItemsSource = searchList;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
            Close();
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter();
        }

        private void supplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }
    }
}
