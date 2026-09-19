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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShoeStore_Kondakov
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var login = loginText.Text;
            var password = passwordText.Password;

            using (var db = new ShoeStoreEntities())
            {
                var user = db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);

                if (user != null)
                {
                    ShowMainWindow();
                } else
                {
                    MessageBox.Show("qweqw", "MEOW", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            MainMenuWindow mainMenuWindow = new MainMenuWindow();
            mainMenuWindow.Show();
            this.Close();

        }
    }
}
