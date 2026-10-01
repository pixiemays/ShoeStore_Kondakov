using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoeStore_Kondakov
{
    public partial class MainMenuWindow : Window
    {
        private List<Product> _allProducts = new List<Product>();
        private int _sortType = 0; // 0 - без сортировки, 1 - по возрастанию, 2 - по убыванию
        private User _currentUser;

        public MainMenuWindow()
        {
            InitializeComponent();
            LoadData();
        }

        public MainMenuWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null && !string.IsNullOrWhiteSpace(_currentUser.FIO))
            {
                FIO.Text = _currentUser.FIO;
            }
            else
            {
                FIO.Text = "Гость";
            }

            if (_currentUser == null || (_currentUser.RoleId != 1 && _currentUser.RoleId != 2))
            {
                FilterPanel.Visibility = Visibility.Collapsed;
            }

            LoadData();
        }

        private void LoadData()
        {
            _allProducts = ShoeStoreEntities.GetContext().Products
                .Include(p => p.ProductName)
                .Include(p => p.ProductCategory)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .ToList();

            List<Supplier> suppliers = ShoeStoreEntities.GetContext().Supplier.ToList();
            suppliers.Insert(0, new Supplier { Id = 0, Name = "Все поставщики" });

            supplierBox.ItemsSource = suppliers;
            supplierBox.SelectedIndex = 0;

            Filtr();
        }

        private void Filtr()
        {
            List<Product> searchList = _allProducts;

            string search = searchBox.Text.ToLower().Trim();
            if (search != "")
            {
                searchList = searchList.Where(x =>
                    (x.ProductName != null && x.ProductName.Name.ToLower().Contains(search)) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)) ||
                    (x.Manufacturer != null && x.Manufacturer.Name.ToLower().Contains(search)) ||
                    (x.Supplier != null && x.Supplier.Name.ToLower().Contains(search)) ||
                    (x.ProductCategory != null && x.ProductCategory.Name.ToLower().Contains(search)) ||
                    (x.Unit != null && x.Unit.ToLower().Contains(search)) ||
                    (x.Article != null && x.Article.ToLower().Contains(search))
                ).ToList();
            }

            if (supplierBox.SelectedIndex > 0)
            {
                Supplier selectedSupplier = supplierBox.SelectedItem as Supplier;
                if (selectedSupplier != null)
                {
                    searchList = searchList.Where(x => x.SupplierId == selectedSupplier.Id).ToList();
                }
            }

            if (_sortType == 1)
            {
                searchList = searchList.OrderBy(x => x.Count).ToList();
            }
            else if (_sortType == 2)
            {
                searchList = searchList.OrderByDescending(x => x.Count).ToList();
            }

            ProductsList.ItemsSource = searchList;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtr();
        }

        private void supplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filtr();
        }

        private void SortAsc_Click(object sender, RoutedEventArgs e)
        {
            _sortType = 1;
            Filtr();
        }

        private void SortDesc_Click(object sender, RoutedEventArgs e)
        {
            _sortType = 2;
            Filtr();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
            supplierBox.SelectedIndex = 0;
            _sortType = 0;
            Filtr();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
            this.Close();
        }
    }
}