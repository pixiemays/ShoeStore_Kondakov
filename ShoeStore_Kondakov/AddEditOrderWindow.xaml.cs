using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShoeStore_Kondakov
{
    public partial class AddEditOrderWindow : Window
    {
        private Order _currentOrder;
        private bool _isNew = false;
        private ObservableCollection<OrderContent> _orderContents;
        private List<OrderContent> _deletedContents = new List<OrderContent>();

        public AddEditOrderWindow(Order order)
        {
            InitializeComponent();

            if (order != null)
            {
                _currentOrder = order;
                LblId.Visibility = Visibility.Visible;
                TxtId.Visibility = Visibility.Visible;
                _isNew = false;
            }
            else
            {
                _currentOrder = new Order
                {
                    OrderDate = DateTime.Today,
                    DeliverDate = DateTime.Today.AddDays(3),
                    StatusId = 2
                };
                _isNew = true;
            }

            DataContext = _currentOrder;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var db = ShoeStoreEntities.GetContext();
                CmbStatus.ItemsSource = db.Statuses.ToList();
                CmbPickupPoint.ItemsSource = db.PickupPoints.ToList();

                var products = db.Products.Include(p => p.ProductName).OrderBy(p => p.Article).ToList();
                CmbProducts.ItemsSource = products;

                if (!_isNew)
                {
                    var contents = _currentOrder.OrderContents.ToList();
                    foreach (var c in contents)
                    {
                        if (c.Product == null && c.ProductId > 0)
                        {
                            c.Product = products.FirstOrDefault(p => p.Id == c.ProductId);
                        }
                    }

                    _orderContents = new ObservableCollection<OrderContent>(contents);
                    DpOrderDate.SelectedDate = _currentOrder.OrderDate;
                    DpDeliverDate.SelectedDate = _currentOrder.DeliverDate;
                }
                else
                {
                    _orderContents = new ObservableCollection<OrderContent>();
                    DpOrderDate.SelectedDate = DateTime.Today;
                    DpDeliverDate.SelectedDate = DateTime.Today.AddDays(3);
                    if (CmbStatus.Items.Count > 0)
                    {
                        CmbStatus.SelectedIndex = 0;
                    }
                    if (CmbPickupPoint.Items.Count > 0)
                    {
                        CmbPickupPoint.SelectedIndex = 0;
                    }
                }

                DgOrderItems.ItemsSource = _orderContents;
                UpdateTotalInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var db = ShoeStoreEntities.GetContext();
            Product selectedProduct = CmbProducts.SelectedItem as Product;

            if (selectedProduct == null && !string.IsNullOrWhiteSpace(CmbProducts.Text))
            {
                string query = CmbProducts.Text.Trim();
                selectedProduct = db.Products.FirstOrDefault(p =>
                    p.Article.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                    (p.ProductName != null && p.ProductName.Name.Equals(query, StringComparison.OrdinalIgnoreCase)));
            }

            if (selectedProduct == null)
            {
                MessageBox.Show("Пожалуйста, выберите товар из списка или введите существующий артикул.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtQuantity.Text?.Trim(), out int count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество (целое число больше 0).", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (count > selectedProduct.Count)
            {
                var result = MessageBox.Show($"На складе доступно всего {selectedProduct.Count} шт. этого товара.\nВсе равно добавить {count} шт. в заказ?",
                    "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var existingItem = _orderContents.FirstOrDefault(c => c.ProductId == selectedProduct.Id);
            if (existingItem != null)
            {
                existingItem.Count += count;
                DgOrderItems.Items.Refresh();
            }
            else
            {
                var newItem = new OrderContent
                {
                    OrderId = _currentOrder.Id,
                    ProductId = selectedProduct.Id,
                    Product = selectedProduct,
                    Count = count
                };
                _orderContents.Add(newItem);
            }

            UpdateTotalInfo();

            TxtQuantity.Text = "1";
            CmbProducts.SelectedIndex = -1;
            CmbProducts.Text = string.Empty;
        }

        private void BtnIncreaseCount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is OrderContent item)
            {
                if (item.Product != null && item.Count >= item.Product.Count)
                {
                    MessageBox.Show($"На складе всего {item.Product.Count} шт. данного товара.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                item.Count++;
                DgOrderItems.Items.Refresh();
                UpdateTotalInfo();
            }
        }

        private void BtnDecreaseCount_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is OrderContent item)
            {
                if (item.Count > 1)
                {
                    item.Count--;
                    DgOrderItems.Items.Refresh();
                    UpdateTotalInfo();
                }
                else
                {
                    BtnRemoveItem_Click(sender, e);
                }
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is OrderContent item)
            {
                string art = item.Product?.Article ?? "товар";
                if (MessageBox.Show($"Удалить артикул «{art}» из заказа?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (item.Id > 0)
                    {
                        _deletedContents.Add(item);
                    }
                    _orderContents.Remove(item);
                    UpdateTotalInfo();
                }
            }
        }

        private void UpdateTotalInfo()
        {
            if (_orderContents == null || _orderContents.Count == 0)
            {
                TxtTotalSum.Text = "В заказе нет товаров | Итоговая сумма: 0.00 ₽";
                return;
            }

            int totalCount = _orderContents.Sum(c => c.Count);
            double totalCost = _orderContents.Sum(c => c.TotalPrice);
            TxtTotalSum.Text = $"Всего товаров: {totalCount} шт. ({_orderContents.Count} позиций) | Итоговая сумма: {totalCost:N2} ₽";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (_orderContents == null || _orderContents.Count == 0)
            {
                errors.AppendLine("В заказе должен быть хотя бы один товар.");
            }

            if (CmbStatus.SelectedValue == null || (int)CmbStatus.SelectedValue <= 0)
            {
                errors.AppendLine("Выберите статус заказа.");
            }

            if (CmbPickupPoint.SelectedValue == null || (int)CmbPickupPoint.SelectedValue <= 0)
            {
                errors.AppendLine("Выберите пункт выдачи.");
            }

            if (!DpOrderDate.SelectedDate.HasValue)
            {
                errors.AppendLine("Укажите дату заказа.");
            }

            if (!DpDeliverDate.SelectedDate.HasValue)
            {
                errors.AppendLine("Укажите дату выдачи.");
            }

            if (DpOrderDate.SelectedDate.HasValue && DpDeliverDate.SelectedDate.HasValue && DpDeliverDate.SelectedDate.Value < DpOrderDate.SelectedDate.Value)
            {
                errors.AppendLine("Дата выдачи не может быть раньше даты заказа.");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var db = ShoeStoreEntities.GetContext();

                _currentOrder.OrderDate = DpOrderDate.SelectedDate.Value;
                _currentOrder.DeliverDate = DpDeliverDate.SelectedDate.Value;
                _currentOrder.StatusId = (int)CmbStatus.SelectedValue;
                _currentOrder.PickupPointId = (int)CmbPickupPoint.SelectedValue;

                if (_isNew)
                {
                    if (_currentOrder.ClientId <= 0)
                    {
                        _currentOrder.ClientId = db.Users.FirstOrDefault()?.Id ?? 1;
                    }
                    if (_currentOrder.ObtainCode <= 0)
                    {
                        _currentOrder.ObtainCode = new Random().Next(100, 1000);
                    }

                    _currentOrder.OrderContents.Clear();
                    foreach (var item in _orderContents)
                    {
                        item.Order = _currentOrder;
                        _currentOrder.OrderContents.Add(item);
                    }

                    db.Orders.Add(_currentOrder);
                }
                else
                {
                    foreach (var del in _deletedContents)
                    {
                        var inDb = db.OrderContents.Local.FirstOrDefault(x => x.Id == del.Id) ?? db.OrderContents.Find(del.Id);
                        if (inDb != null)
                        {
                            db.OrderContents.Remove(inDb);
                        }
                        _currentOrder.OrderContents.Remove(del);
                    }

                    foreach (var item in _orderContents)
                    {
                        if (item.Id == 0)
                        {
                            item.OrderId = _currentOrder.Id;
                            item.Order = _currentOrder;
                            if (!_currentOrder.OrderContents.Contains(item))
                            {
                                _currentOrder.OrderContents.Add(item);
                            }
                            if (db.Entry(item).State == EntityState.Detached)
                            {
                                db.OrderContents.Add(item);
                            }
                        }
                        else
                        {
                            var inDb = db.OrderContents.Local.FirstOrDefault(x => x.Id == item.Id) ?? db.OrderContents.Find(item.Id);
                            if (inDb != null)
                            {
                                inDb.Count = item.Count;
                            }
                        }
                    }
                }

                db.SaveChanges();

                MessageBox.Show("Заказ успешно сохранен!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sb.AppendLine($"Поле: {validationError.PropertyName} — Ошибка: {validationError.ErrorMessage}");
                    }
                }
                MessageBox.Show(sb.ToString(), "Ошибка валидации БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                MessageBox.Show($"Ошибка базы данных:\n{inner.Message}", "Ошибка сохранения SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var db = ShoeStoreEntities.GetContext();
            foreach (var entry in db.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    entry.Reload();
                }
                else if (entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Detached;
                }
            }

            DialogResult = false;
            Close();
        }
    }
}
