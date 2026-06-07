using BizRent.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BizRent.Views.Manager
{
    public partial class ApplicationEditWindow : Window
    {
        private int _applicationId;
        private Applications _application;

        public ApplicationEditWindow(int applicationId = 0)
        {
            InitializeComponent();
            _applicationId = applicationId;
            Title = applicationId == 0 ? "Новая заявка" : "Редактирование заявки";

            LoadData();
            LoadApplicationData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var areas = db.Areas
                        .Where(a => a.StatusId == 1) 
                        .OrderBy(a => a.Name)
                        .ToList();

                    var areasList = areas.ToList();
                    areasList.Insert(0, new Areas { Id = 0, Name = "(Не выбрано)" });
                    ComboArea.ItemsSource = areasList;
                    ComboArea.DisplayMemberPath = "Name";
                    ComboArea.SelectedValuePath = "Id";

                    var statuses = db.ApplicationStatuses
                        .OrderBy(s => s.Id)
                        .ToList();
                    ComboStatus.ItemsSource = statuses;
                    ComboStatus.DisplayMemberPath = "Name";
                    ComboStatus.SelectedValuePath = "Id";

                    var managers = db.Users
                        .Where(u => u.RoleId == 2) 
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToList();

                    var managersList = managers.ToList();
                    managersList.Insert(0, new Users { Id = 0, LastName = "(Не назначено)", FirstName = "" });
                    ComboManager.ItemsSource = managersList;
                    ComboManager.DisplayMemberPath = "LastName";
                    ComboManager.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadApplicationData()
        {
            if (_applicationId > 0)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        _application = db.Applications
                            .FirstOrDefault(a => a.Id == _applicationId);

                        if (_application != null)
                        {
                            TextBoxClient.Text = _application.ClientName;
                            TextBoxPhone.Text = _application.Phone;
                            TextBoxEmail.Text = _application.Email;
                            TextBoxMessage.Text = _application.Message;

                            if (_application.AreaId.HasValue && _application.AreaId.Value > 0)
                                ComboArea.SelectedValue = _application.AreaId.Value;
                            else
                                ComboArea.SelectedIndex = 0;

                            ComboStatus.SelectedValue = _application.StatusId;

                            if (_application.AssignedToUserId.HasValue && _application.AssignedToUserId.Value > 0)
                                ComboManager.SelectedValue = _application.AssignedToUserId.Value;
                            else
                                ComboManager.SelectedIndex = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки заявки: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                ComboStatus.SelectedValue = 1;
                ComboArea.SelectedIndex = 0;
                ComboManager.SelectedIndex = 0;

                if (CurrentUser.IsManager)
                {
                    ComboManager.SelectedValue = CurrentUser.Id;
                }
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var digits = phone.Where(char.IsDigit).Count();
            return digits >= 5;
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
            if (string.IsNullOrWhiteSpace(TextBoxClient.Text.Trim()))
            {
                MessageBox.Show("Введите имя клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxClient.Focus();
                return;
            }

            if (!IsValidPhone(TextBoxPhone.Text))
            {
                MessageBox.Show("Введите корректный телефон", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxPhone.Focus();
                return;
            }

            if (!IsValidEmail(TextBoxEmail.Text))
            {
                MessageBox.Show("Введите корректный email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxEmail.Focus();
                return;
            }

            if (ComboStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус заявки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboStatus.Focus();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    if (_applicationId == 0)
                    {
                        _application = new Applications
                        {
                            ClientName = TextBoxClient.Text.Trim(),
                            Phone = TextBoxPhone.Text.Trim(),
                            Email = TextBoxEmail.Text?.Trim(),
                            Message = TextBoxMessage.Text?.Trim(),
                            StatusId = (int)ComboStatus.SelectedValue,
                            CreatedDate = DateTime.Now
                        };

                        if (ComboArea.SelectedValue != null && (int)ComboArea.SelectedValue > 0)
                            _application.AreaId = (int)ComboArea.SelectedValue;

                        if (ComboManager.SelectedValue != null && (int)ComboManager.SelectedValue > 0)
                            _application.AssignedToUserId = (int)ComboManager.SelectedValue;

                        db.Applications.Add(_application);
                    }
                    else
                    {
                        var appToUpdate = db.Applications.Find(_applicationId);
                        if (appToUpdate != null)
                        {
                            appToUpdate.ClientName = TextBoxClient.Text.Trim();
                            appToUpdate.Phone = TextBoxPhone.Text.Trim();
                            appToUpdate.Email = TextBoxEmail.Text?.Trim();
                            appToUpdate.Message = TextBoxMessage.Text?.Trim();
                            appToUpdate.StatusId = (int)ComboStatus.SelectedValue;

                            if (ComboArea.SelectedValue != null && (int)ComboArea.SelectedValue > 0)
                                appToUpdate.AreaId = (int)ComboArea.SelectedValue;
                            else
                                appToUpdate.AreaId = null;

                            if (ComboManager.SelectedValue != null && (int)ComboManager.SelectedValue > 0)
                                appToUpdate.AssignedToUserId = (int)ComboManager.SelectedValue;
                            else
                                appToUpdate.AssignedToUserId = null;
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