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
        private User _user;
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
                _user = db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);

                if (_user != null)
                {
                    ShowMainWindow(_user);
                } else
                {
                    MessageBox.Show("qweqw", "MEOW", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow(new User());
        }

        private void ShowMainWindow(User user)
        {
            MainMenuWindow mainMenuWindow = new MainMenuWindow(user);
            mainMenuWindow.Show();
            this.Close();

        }
    }
}
