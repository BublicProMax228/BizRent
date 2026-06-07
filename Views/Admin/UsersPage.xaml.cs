using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Manager;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BizRent.Views.Admin
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var users = db.Users
                        .Include("Roles")
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToList();

                    DataGridUsers.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = DataGridUsers.SelectedItem as Users;
            if (user != null)
            {
                var dialog = new UserEditWindow(user.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var user = DataGridUsers.SelectedItem as Users;
            if (user != null)
            {
                if (user.Id == CurrentUser.Id)
                {
                    MessageBox.Show("Нельзя удалить самого себя!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show($"Удалить пользователя {user.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DataBaseContext())
                        {
                            var userToDelete = db.Users.Find(user.Id);
                            if (userToDelete != null)
                            {
                                db.Users.Remove(userToDelete);
                                db.SaveChanges();
                                LoadUsers();
                                MessageBox.Show("Пользователь удалён",
                                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TextBoxSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadUsers();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    var users = db.Users
                        .Include("Roles")
                        .Where(u => u.Username.ToLower().Contains(searchText) ||
                                    u.LastName.ToLower().Contains(searchText) ||
                                    u.FirstName.ToLower().Contains(searchText) ||
                                    u.Email.ToLower().Contains(searchText) ||
                                    u.Phone.Contains(searchText))
                        .OrderBy(u => u.LastName)
                        .ToList();

                    DataGridUsers.ItemsSource = users;
                }
            }
            catch { }
        }

        private void DataGridUsers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
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