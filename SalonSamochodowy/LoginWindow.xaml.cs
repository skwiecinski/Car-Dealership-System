using System.Windows;

namespace SalonSamochodowy // <--- Sprawdź czy ta nazwa jest identyczna jak w MainWindow.xaml.cs
{
    public partial class LoginWindow : Wpf.Ui.Controls.FluentWindow // <--- Dodaj pełną ścieżkę do FluentWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Na potrzeby testu: wpisz cokolwiek w login, żeby wejść
            if (!string.IsNullOrEmpty(TxtUsername.Text))
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Wprowadź dane logowania!");
            }
        }

        private void ExitBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}