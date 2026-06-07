using BizRent.Data;
using BizRent.Views.Admin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BizRent.Views.Admin
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            Loaded += AdminPage_Loaded;
        }

        private async void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    // Всего пользователей
                    int totalUsers = await db.Users.CountAsync();
                    TextTotalUsers.Text = totalUsers.ToString();

                    // Всего арендаторов
                    int totalTenants = await db.Tenants.CountAsync();
                    TextTotalTenants.Text = totalTenants.ToString();

                    // Активные договоры (статус 2 = Активен)
                    int activeContracts = await db.Contracts
                        .CountAsync(c => c.StatusId == 2);
                    TextActiveContracts.Text = activeContracts.ToString();

                    // Новые заявки (статус 1 = Новая)
                    int newApplications = await db.Applications
                        .CountAsync(a => a.StatusId == 1);
                    TextNewApplications.Text = newApplications.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardUsers_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new UsersPage());
        }

        private void CardAddUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var window = new UserEditWindow(); // окно создания нового пользователя
            window.Owner = Application.Current.MainWindow;

            if (window.ShowDialog() == true)
            {
                // Обновляем статистику после добавления пользователя
                LoadStatisticsAsync();
            }
        }
    }
}