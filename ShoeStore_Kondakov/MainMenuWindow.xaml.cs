using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShoeStore_Kondakov
{
    public partial class MainMenuWindow : Window
    {
        private User _currentUser;
        private int _sortType = 0;

        public MainMenuWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            ConfigureUserAccess();
            LoadSuppliers();
            UpdateProducts();
        }

        private void ConfigureUserAccess()
        {
            if (_currentUser == null)
            {
                FIO.Text = "Гость";
                FilterPanel.Visibility = Visibility.Collapsed;
                AdminActionsPanel.Visibility = Visibility.Collapsed;
                btnOrders.Visibility = Visibility.Collapsed;
                return;
            }

            FIO.Text = _currentUser.FIO;

            int roleId = _currentUser.RoleId;

            if (roleId == 1) // Администратор
            {
                FilterPanel.Visibility = Visibility.Visible;
                AdminActionsPanel.Visibility = Visibility.Visible;
                btnOrders.Visibility = Visibility.Visible;
            }
            else if (roleId == 2) // Менеджер
            {
                FilterPanel.Visibility = Visibility.Visible;
                AdminActionsPanel.Visibility = Visibility.Collapsed;
                btnOrders.Visibility = Visibility.Visible;
            }
            else if (roleId == 3) // клиент
            {
                FilterPanel.Visibility = Visibility.Collapsed;
                AdminActionsPanel.Visibility = Visibility.Collapsed;
                btnOrders.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = ShoeStoreEntities.GetContext().Supplier.ToList();
                suppliers.Insert(0, new Supplier { Id = 0, Name = "Все поставщики" });
                supplierBox.ItemsSource = suppliers;
                supplierBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки поставщиков");
            }
        }

        private void UpdateProducts()
        {
            try
            {
                var context = ShoeStoreEntities.GetContext();
                var currentProducts = context.Products.ToList();

                if (!string.IsNullOrWhiteSpace(searchBox?.Text))
                {
                    string search = searchBox.Text.ToLower().Trim();
                    currentProducts = currentProducts.Where(p =>
                        (p.ProductName != null && p.ProductName.Name.ToLower().Contains(search)) ||
                        (p.Description != null && p.Description.ToLower().Contains(search)) ||
                        (p.ProductCategory != null && p.ProductCategory.Name.ToLower().Contains(search)) ||
                        (p.Manufacturer != null && p.Manufacturer.Name.ToLower().Contains(search))
                    ).ToList();
                }

                if (supplierBox?.SelectedItem is Supplier selectedSupplier && selectedSupplier.Id != 0)
                {
                    currentProducts = currentProducts.Where(p => p.SupplierId == selectedSupplier.Id).ToList();
                }

                if (_sortType == 1)
                {
                    currentProducts = currentProducts.OrderBy(p => p.Count).ToList();
                }
                else if (_sortType == 2)
                {
                    currentProducts = currentProducts.OrderByDescending(p => p.Count).ToList();
                }

                ProductsList.ItemsSource = null;
                ProductsList.ItemsSource = currentProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления списка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageBox.Show("Добавление товаров доступно только администратору!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddEditProductWindow editWin = new AddEditProductWindow(null);
            if (editWin.ShowDialog() == true)
            {
                UpdateProducts();
            }
        }

        private void ProductsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                return;
            }

            if (ProductsList.SelectedItem is Product selectedProduct)
            {
                AddEditProductWindow editWin = new AddEditProductWindow(selectedProduct);
                if (editWin.ShowDialog() == true)
                {
                    UpdateProducts();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageBox.Show("Удаление товаров доступно только администратору!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedProducts = ProductsList.SelectedItems.Cast<Product>().ToList();

            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Выберите товар(ы) для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var blockedProducts = selectedProducts.Where(p => p.OrderContents != null && p.OrderContents.Count > 0).ToList();
            if (blockedProducts.Any())
            {
                MessageBox.Show("Невозможно удалить выбранные товары, так как некоторые из них присутствуют в оформленных заказах!", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить выбранные элементы ({selectedProducts.Count} шт.)?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var db = ShoeStoreEntities.GetContext();
                    foreach (var item in selectedProducts)
                    {
                        db.Products.Remove(item);
                    }
                    db.SaveChanges();

                    MessageBox.Show("Данные успешно удалены!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void supplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void SortAsc_Click(object sender, RoutedEventArgs e)
        {
            _sortType = 1;
            UpdateProducts();
        }

        private void SortDesc_Click(object sender, RoutedEventArgs e)
        {
            _sortType = 2;
            UpdateProducts();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = string.Empty;
            supplierBox.SelectedIndex = 0;
            _sortType = 0;
            UpdateProducts();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authWin = new AuthorizationWindow();
            authWin.Show();
            Close();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWin = new OrdersWindow(_currentUser);
            ordersWin.Show();
            Close();
        }
    }
}