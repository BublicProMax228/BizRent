using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Admin;
using BizRent.Views.Manager;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BizRent.Views
{
    public partial class TenantsPage : Page
    {
        public TenantsPage()
        {
            InitializeComponent();
            LoadTenants();
        }

        private void LoadTenants()
        {
            try
            {
                DataGridTenants.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var tenants = db.Tenants
                        .OrderBy(t => t.CompanyName)
                        .ToList();

                    DataGridTenants.ItemsSource = tenants;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки арендаторов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearchFilter()
        {
            try
            {
                DataGridTenants.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var searchText = TextBoxSearch.Text?.Trim().ToLower();

                    var query = db.Tenants.AsQueryable();

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query = query.Where(t =>
                            (t.CompanyName != null && t.CompanyName.ToLower().Contains(searchText)) ||
                            (t.ContactPerson != null && t.ContactPerson.ToLower().Contains(searchText)) ||
                            (t.Phone != null && t.Phone.Contains(searchText)) ||
                            (t.Email != null && t.Email.ToLower().Contains(searchText)) ||
                            (t.INN != null && t.INN.Contains(searchText))
                        );
                    }

                    var result = query
                        .OrderBy(t => t.CompanyName)
                        .ToList();

                    DataGridTenants.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TenantEditWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadTenants();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var tenant = DataGridTenants.SelectedItem as Tenants;
            if (tenant != null)
            {
                var dialog = new TenantEditWindow(tenant.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadTenants();
                }
            }
            else
            {
                MessageBox.Show("Выберите арендатора для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var tenant = DataGridTenants.SelectedItem as Tenants;
            if (tenant != null)
            {
                if (MessageBox.Show($"Удалить арендатора '{tenant.CompanyName}'?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DataBaseContext())
                        {
                            var toDelete = db.Tenants.Find(tenant.Id);
                            if (toDelete != null)
                            {
                                db.Tenants.Remove(toDelete);
                                db.SaveChanges();
                                LoadTenants();
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
            else
            {
                MessageBox.Show("Выберите арендатора для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DataGridTenants_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Clear();
            LoadTenants();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentUser.RoleName)
            {
                case "Администратор":
                    NavigationService.Navigate(new AdminPage());
                    break;
                case "Менеджер":
                    NavigationService.Navigate(new ManagerPage());
                    break;
                case "Бухгалтер":
                    NavigationService.Navigate(new AccountantPage());
                    break;
                default:
                    NavigationService.Navigate(new AdminPage());
                    break;
            }
        }
    }
}