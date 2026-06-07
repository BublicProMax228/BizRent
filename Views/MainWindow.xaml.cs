using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Manager;
using System.Windows;
using BizRent.Views.Admin;
using System;

namespace BizRent.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Title = $"BizRent - {CurrentUser.RoleName}";

            SetupMenuByRole();

            StatusBarUser.Text = $"Пользователь: {CurrentUser.GetInfo()}";

            LoadStartPage();
        }

        private void SetupMenuByRole()
        {
            if (CurrentUser.IsManager)
            {
                MenuAreas.Visibility = Visibility.Visible;
                MenuContracts.Visibility = Visibility.Visible;
                MenuTenants.Visibility = Visibility.Visible;
                MenuApplications.Visibility = Visibility.Visible;
            }

            if (CurrentUser.IsAccountant)
            {
                MenuPayments.Visibility = Visibility.Visible;
                MenuReports.Visibility = Visibility.Visible;

            }

            if (CurrentUser.IsAdmin)
            {
                MenuAreas.Visibility = Visibility.Visible;
                MenuContracts.Visibility = Visibility.Visible;
                MenuTenants.Visibility = Visibility.Visible;
                MenuPayments.Visibility = Visibility.Visible;
                MenuApplications.Visibility = Visibility.Visible;
                MenuReports.Visibility = Visibility.Visible;
                MenuAdmin.Visibility = Visibility.Visible;
            }
        }

        private void LoadStartPage()
        {
            if (CurrentUser.IsAdmin)
            {
                MainFrame.Navigate(new AdminPage());
            }
            else if (CurrentUser.IsManager)
            {
                MainFrame.Navigate(new ManagerPage());
            }
            else if (CurrentUser.IsAccountant)
            {
                MainFrame.Navigate(new AccountantPage());
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Clear();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void MenuAreas_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AreasPage());
        }

        private void MenuAddArea_Click(object sender, RoutedEventArgs e)
        {
            var EditWindow = new AreaEditWindow();
            EditWindow.Show();
        }

        private void MenuContracts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ContractsPage());
        }

        private void MenuAddContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new ContractEditWindow();
                editWindow.Owner = this;
                editWindow.ShowDialog(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuTenants_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TenantsPage());
        }

        private void MenuAddTenant_Click(object sender, RoutedEventArgs e)
        {
            var EditWindow = new TenantEditWindow();
            EditWindow.Show();
        }

        private void MenuPaymentsList_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PaymentsPage());
        }

        private void MenuApplications_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApplicationsPage());
        }

        private void MenuAddApplication_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new Manager.ApplicationEditWindow();
                editWindow.Owner = this;
                if (editWindow.ShowDialog() == true) 
                {
                    if (MainFrame.Content is ApplicationsPage applicationsPage)
                    {
                        var currentPage = MainFrame.Content as ApplicationsPage;
                        System.Reflection.MethodInfo method = currentPage.GetType().GetMethod("LoadApplications");
                        if (method != null)
                            method.Invoke(currentPage, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна заявки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuUsers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }

        private void MenuReportFinance_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportsPage());
        }
        private void MenuReportAreas_Click(object sender, RoutedEventArgs e)
        {
            var page = new ReportsPage();
            MainFrame.Navigate(page);
        }
    }
}