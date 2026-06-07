using BizRent.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BizRent.Views.Manager
{
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var newApplications = db.Applications
                        .Count(a => a.StatusId == 1);
                    TextNewApps.Text = newApplications.ToString();

                    var activeContracts = db.Contracts
                        .Count(c => c.StatusId == 2);
                    TextActiveContracts.Text = activeContracts.ToString();

                    var availableAreas = db.Areas
                        .Count(a => a.StatusId == 1);
                    TextAvailableAreas.Text = availableAreas.ToString();

                    var expiringDate = DateTime.Now.AddDays(30);
                    var expiringContracts = db.Contracts
                        .Count(c => c.StatusId == 2 && c.EndDate <= expiringDate);
                    TextExpiringContracts.Text = expiringContracts.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardApplications_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ApplicationsPage());
        }

        private void CardContracts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ContractsPage());
        }

        private void CardClients_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new TenantsPage());
        }

        private void CardAreas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new AreasPage());
        }

        private void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
    }
}