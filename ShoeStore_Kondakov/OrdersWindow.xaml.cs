using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShoeStore_Kondakov
{
    public partial class OrdersWindow : Window
    {
        private User _currentUser;

        public OrdersWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            ConfigureUserAccess();
            UpdateOrders();
        }

        private void ConfigureUserAccess()
        {
            if (_currentUser == null)
            {
                FIO.Text = "Гость";
                AdminActionsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            FIO.Text = _currentUser.FIO;

            if (_currentUser.RoleId == 1)
            {
                AdminActionsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AdminActionsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateOrders()
        {
            try
            {
                var db = ShoeStoreEntities.GetContext();
                var orders = db.Orders.Include(o => o.Status).Include(o => o.PickupPoint).Include(o => o.OrderContents.Select(oc => oc.Product)).OrderByDescending(o => o.OrderDate).ToList();
                OrdersList.ItemsSource = null;
                OrdersList.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainMenuWindow mainMenuWindow = new MainMenuWindow(_currentUser);
            mainMenuWindow.Show();
            Close();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageBox.Show("Добавление заказов доступно только администратору!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddEditOrderWindow addWin = new AddEditOrderWindow(null);
            if (addWin.ShowDialog() == true)
            {
                UpdateOrders();
            }
        }

        private void OrdersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                return;
            }

            if (OrdersList.SelectedItem is Order selectedOrder)
            {
                AddEditOrderWindow editWin = new AddEditOrderWindow(selectedOrder);
                if (editWin.ShowDialog() == true)
                {
                    UpdateOrders();
                }
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageBox.Show("Удаление заказов доступно только администратору!", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrdersList.SelectedItem is Order selectedOrder)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить выбранный заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var db = ShoeStoreEntities.GetContext();
                        var contents = db.OrderContents.Where(c => c.OrderId == selectedOrder.Id).ToList();
                        foreach (var c in contents)
                        {
                            db.OrderContents.Remove(c);
                        }
                        db.Orders.Remove(selectedOrder);
                        db.SaveChanges();

                        MessageBox.Show("Заказ успешно удален!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
