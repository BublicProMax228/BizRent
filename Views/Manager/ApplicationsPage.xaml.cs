using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Admin;
using BizRent.Views.Manager;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BizRent.Views.Manager
{
    public partial class ApplicationsPage : Page
    {
        public ApplicationsPage()
        {
            InitializeComponent();
            InitializeFilters();
            LoadApplications();
        }

        private void InitializeFilters()
        {
            ComboFilter.Items.Clear();
            ComboFilter.Items.Add("Все заявки");
            ComboFilter.Items.Add("Новые");
            ComboFilter.Items.Add("В обработке");
            ComboFilter.Items.Add("Просмотр объекта");
            ComboFilter.Items.Add("Ожидает договора");
            ComboFilter.Items.Add("Отклонена");

            ComboFilter.SelectedIndex = 0;
        }

        private void LoadApplications()
        {
            try
            {
                DataGridApplications.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var applications = db.Applications
                        .Include("Areas")
                        .Include("ApplicationStatuses")
                        .Include("Users")
                        .OrderByDescending(a => a.CreatedDate)
                        .ToList();

                    DataGridApplications.ItemsSource = applications;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                DataGridApplications.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var query = db.Applications
                        .Include("Areas")
                        .Include("ApplicationStatuses")
                        .Include("Users")
                        .AsQueryable();

                    if (ComboFilter.SelectedIndex > 0)
                    {
                        int statusId = ComboFilter.SelectedIndex;
                        query = query.Where(a => a.StatusId == statusId);
                    }

                    var searchText = TextBoxSearch.Text?.Trim().ToLower();
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query = query.Where(a =>
                            (a.ClientName != null && a.ClientName.ToLower().Contains(searchText)) ||
                            (a.Phone != null && a.Phone.Contains(searchText)) ||
                            (a.Email != null && a.Email.ToLower().Contains(searchText))
                        );
                    }

                    var result = query.OrderByDescending(a => a.CreatedDate).ToList();
                    DataGridApplications.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== СОБЫТИЯ ==================
        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ================== КНОПКИ ==================
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ApplicationEditWindow();
            if (dialog.ShowDialog() == true)
                LoadApplications();
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            var app = DataGridApplications.SelectedItem as Applications;
            if (app == null) return;

            try
            {
                using (var db = new DataBaseContext())
                {
                    var toUpdate = db.Applications.Find(app.Id);
                    if (toUpdate != null)
                    {
                        toUpdate.StatusId = 2;
                        toUpdate.AssignedToUserId = CurrentUser.Id;
                        db.SaveChanges();
                        ApplyFilters();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            var app = DataGridApplications.SelectedItem as Applications;
            if (app != null)
            {
                var window = new ApplicationViewWindow(app.Id);
                window.ShowDialog();
                ApplyFilters();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var app = DataGridApplications.SelectedItem as Applications;
            if (app == null) return;

            if (MessageBox.Show($"Удалить заявку от {app.ClientName}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        var toDelete = db.Applications.Find(app.Id);
                        if (toDelete != null)
                        {
                            db.Applications.Remove(toDelete);
                            db.SaveChanges();
                            LoadApplications();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DataGridApplications_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnView_Click(sender, e);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentUser.RoleName)
            {
                case "Администратор": NavigationService.Navigate(new AdminPage()); break;
                case "Менеджер": NavigationService.Navigate(new ManagerPage()); break;
                case "Бухгалтер": NavigationService.Navigate(new AccountantPage()); break;
                default: NavigationService.Navigate(new AdminPage()); break;
            }
        }
    }
}