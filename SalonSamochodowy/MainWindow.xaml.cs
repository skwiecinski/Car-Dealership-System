using Microsoft.EntityFrameworkCore.Storage;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using System;
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

namespace SalonSamochodowy
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DatabaseTestAsync();
        }

        public async Task DatabaseTestAsync()
        {
            using (var dbContext = new AppDbContext())
            using (var unitOfWork = new UnitOfWork(dbContext))
            {
                var dealerships = await unitOfWork.Dealerships.GetAllAsync();

                if (!dealerships.Any())
                {
                    var newDealerhip = new Dealership
                    {
                        Name = "Salon BMW",
                        Address = "ul. Gliwicka 67",
                        City = "Gliwice",
                        Owner = "Marek Gokarter Znamirowski"
                    };

                    await unitOfWork.Dealerships.AddAsync(newDealerhip);
                    await unitOfWork.CompleteAsync();

                    MessageBox.Show("Salon dodany.", "Test bazy danych");
                }
                else
                {
                    MessageBox.Show($"W bazie znaleziono salon. {dealerships.Count()} salonów w systemie.", "Test bazy danych");
                }
            }
        }
    }
}