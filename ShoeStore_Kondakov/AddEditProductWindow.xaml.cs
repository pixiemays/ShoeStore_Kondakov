using Microsoft.Win32;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ShoeStore_Kondakov
{
    public partial class AddEditProductWindow : Window
    {
        private Product _currentProduct;
        private string _sourceFilePath = null;
        private string _photoPath = null;
        private string _oldPhotoPath = null;
        private bool _isNew = false;

        public AddEditProductWindow(Product product)
        {
            InitializeComponent();

            if (product != null)
            {
                _currentProduct = product;
                LblId.Visibility = Visibility.Visible;
                TxtId.Visibility = Visibility.Visible;
                _isNew = false;
            }
            else
            {
                _currentProduct = new Product
                {
                    Unit = "шт.",
                    Count = 0,
                    Discount = 0,
                    Price = 0
                };
                _isNew = true;
            }

            DataContext = _currentProduct;
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            var db = ShoeStoreEntities.GetContext();
            CmbProductName.ItemsSource = db.ProductNames.ToList();
            CmbCategory.ItemsSource = db.ProductCategories.ToList();
            CmbManufacturer.ItemsSource = db.Manufacturers.ToList();
            CmbSupplier.ItemsSource = db.Supplier.ToList();
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = ".png";
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (dialog.ShowDialog() == true)
                {
                    BitmapImage checkBmp = new BitmapImage();
                    using (var stream = File.OpenRead(dialog.FileName))
                    {
                        checkBmp.BeginInit();
                        checkBmp.CacheOption = BitmapCacheOption.OnLoad;
                        checkBmp.StreamSource = stream;
                        checkBmp.EndInit();
                    }

                    if (checkBmp.PixelWidth > 300 || checkBmp.PixelHeight > 200)
                    {
                        MessageBox.Show("Размер изображения не должен превышать 300x200 пикселей!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _sourceFilePath = dialog.FileName;
                    _photoPath = dialog.SafeFileName;
                    TxtPhotoPath.Text = _photoPath;

                    if (!string.IsNullOrEmpty(_currentProduct.Photo))
                    {
                        _oldPhotoPath = System.IO.Path.Combine(targetDirectory, _currentProduct.Photo);
                    }

                    _currentProduct.Photo = _photoPath;
                    PhotoTovarIm.Source = new BitmapImage(new Uri(_sourceFilePath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка работы программы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();

            StringBuilder errors = new StringBuilder();

            if (_currentProduct.NameId <= 0)
                errors.AppendLine("Выберите наименование товара.");
            if (_currentProduct.ProductCategoryId <= 0)
                errors.AppendLine("Выберите категорию товара.");
            if (_currentProduct.ManufacturerId <= 0)
                errors.AppendLine("Выберите производителя.");
            if (_currentProduct.SupplierId <= 0)
                errors.AppendLine("Выберите поставщика.");
            if (_currentProduct.Price < 0)
                errors.AppendLine("Стоимость не может быть отрицательной.");
            if (_currentProduct.Count < 0)
                errors.AppendLine("Количество на складе не может быть отрицательным.");
            if (_currentProduct.Discount < 0 || _currentProduct.Discount > 100)
                errors.AppendLine("Скидка должна быть в диапазоне от 0 до 100%.");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var db = ShoeStoreEntities.GetContext();

                if (!string.IsNullOrEmpty(_sourceFilePath) && !string.IsNullOrEmpty(_photoPath))
                {
                    string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    string newPath = System.IO.Path.Combine(targetDirectory, _photoPath);

                    if (!string.IsNullOrEmpty(_oldPhotoPath) && File.Exists(_oldPhotoPath) && !_oldPhotoPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(_oldPhotoPath); } catch { }
                    }

                    File.Copy(_sourceFilePath, newPath, overwrite: true);
                }

                if (_isNew)
                {
                    int maxId = db.Products.Any() ? db.Products.Max(p => p.Id) : 0;
                    _currentProduct.Id = maxId + 1;

                    if (string.IsNullOrWhiteSpace(_currentProduct.Article))
                    {
                        _currentProduct.Article = "ART" + _currentProduct.Id.ToString("D4");
                    }

                    db.Products.Add(_currentProduct);
                }

                db.SaveChanges();

                MessageBox.Show("Данные успешно сохранены!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}