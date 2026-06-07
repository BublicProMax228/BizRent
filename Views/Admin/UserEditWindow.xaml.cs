using BizRent.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BizRent.Views.Admin
{
    public partial class UserEditWindow : Window
    {
        private int _userId;
        private Users _user;

        public UserEditWindow(int userId = 0)
        {
            InitializeComponent();
            _userId = userId;
            LoadRoles();
            LoadUserData();
        }

        private void LoadRoles()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    ComboRole.ItemsSource = db.Roles.ToList();
                    ComboRole.DisplayMemberPath = "Name";
                    ComboRole.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserData()
        {
            if (_userId > 0)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        _user = db.Users.Find(_userId);
                        if (_user != null)
                        {
                            TextBoxUsername.Text = _user.Username;
                            TextBoxLastName.Text = _user.LastName;
                            TextBoxFirstName.Text = _user.FirstName;
                            TextBoxMiddleName.Text = _user.MiddleName;
                            TextBoxEmail.Text = _user.Email;
                            TextBoxPhone.Text = _user.Phone;
                            ComboRole.SelectedValue = _user.RoleId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; 

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxUsername.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxUsername.Focus();
                return;
            }

            if (_userId == 0 && string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxLastName.Text) ||
                string.IsNullOrWhiteSpace(TextBoxFirstName.Text))
            {
                MessageBox.Show("Введите фамилию и имя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ComboRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboRole.Focus();
                return;
            }

            if (!IsValidEmail(TextBoxEmail.Text))
            {
                MessageBox.Show("Некорректный email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxEmail.Focus();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    var existingUser = db.Users
                        .FirstOrDefault(u => u.Username == TextBoxUsername.Text.Trim() && u.Id != _userId);

                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        TextBoxUsername.Focus();
                        return;
                    }

                    if (_userId == 0)
                    {
                        _user = new Users
                        {
                            Username = TextBoxUsername.Text.Trim(),
                            Password = PasswordBox.Password,
                            LastName = TextBoxLastName.Text.Trim(),
                            FirstName = TextBoxFirstName.Text.Trim(),
                            MiddleName = TextBoxMiddleName.Text?.Trim(),
                            Email = TextBoxEmail.Text?.Trim(),
                            Phone = TextBoxPhone.Text?.Trim(),
                            RoleId = (int)ComboRole.SelectedValue
                        };
                        db.Users.Add(_user);
                    }
                    else
                    {
                        _user.Username = TextBoxUsername.Text.Trim();
                        _user.LastName = TextBoxLastName.Text.Trim();
                        _user.FirstName = TextBoxFirstName.Text.Trim();
                        _user.MiddleName = TextBoxMiddleName.Text?.Trim();
                        _user.Email = TextBoxEmail.Text?.Trim();
                        _user.Phone = TextBoxPhone.Text?.Trim();
                        _user.RoleId = (int)ComboRole.SelectedValue;

                        if (!string.IsNullOrEmpty(PasswordBox.Password))
                        {
                            _user.Password = PasswordBox.Password;
                        }
                    }

                    db.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}