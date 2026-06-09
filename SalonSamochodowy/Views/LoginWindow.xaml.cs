using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace SalonSamochodowy.Views
{
    public partial class LoginWindow : FluentWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = ((App)Application.Current).Services.GetRequiredService<LoginViewModel>();
            vm.LoginSucceeded += user =>
            {
                SessionContext.CurrentUser = user; // zeby miec info o userze, jest static, takze mozna sie odwolywac bez tworzenia klasy
                var main = new MainWindow(user);
                main.Show();
                Close();
            };
            vm.ExitRequested += () => Application.Current.Shutdown();

            DataContext = vm;

            // Ustaw początkowy wygląd flag
            string currentLang = SalonSamochodowy.Services.LocalizationHelper.GetString("LanguageCode");
            if (currentLang == "EN")
            {
                LangPlBtn.Opacity = 0.4;
                LangEnBtn.Opacity = 1.0;
            }
            else
            {
                LangPlBtn.Opacity = 1.0;
                LangEnBtn.Opacity = 0.4;
            }
        }

        private void LangPl_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("pl");
        }

        private void LangEn_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("en");
        }

        private void SwitchLanguage(string lang)
        {
            var dict = new ResourceDictionary();
            dict.Source = new System.Uri($"pack://application:,,,/Resources/Languages/Strings.{lang}.xaml");

            var merged = Application.Current.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                if (merged[i].Source != null && merged[i].Source.OriginalString.Contains("Strings."))
                {
                    merged.RemoveAt(i);
                }
            }

            merged.Add(dict);

            if (lang == "pl")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
                LangPlBtn.Opacity = 1.0;
                LangEnBtn.Opacity = 0.4;
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                LangPlBtn.Opacity = 0.4;
                LangEnBtn.Opacity = 1.0;
            }

            WeakReferenceMessenger.Default.Send(new SalonSamochodowy.Messages.LanguageChangedMessage());
        }
    }
}
