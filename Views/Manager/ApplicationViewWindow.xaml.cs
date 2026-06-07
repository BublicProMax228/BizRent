using BizRent.Data;
using System;
using System.Linq;
using System.Windows;

namespace BizRent.Views.Manager
{
    public partial class ApplicationViewWindow : Window
    {
        private int _applicationId;
        private Applications _application;

        public ApplicationViewWindow(int applicationId)
        {
            InitializeComponent();
            _applicationId = applicationId;
            LoadApplication();
        }

        private void LoadApplication()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    _application = db.Applications
                        .Include("Areas")
                        .Include("ApplicationStatuses")
                        .Include("Users")
                        .FirstOrDefault(a => a.Id == _applicationId);

                    if (_application != null)
                    {
                        TextTitle.Text = $"Заявка #{_application.Id}";
                        TextStatus.Text = $"Статус: {_application.ApplicationStatuses?.Name ?? "Неизвестно"}";

                        TextClient.Text = _application.ClientName;
                        TextPhone.Text = _application.Phone;
                        TextEmail.Text = _application.Email ?? "(не указан)";
                        TextArea.Text = _application.Areas?.Name ?? "(не выбрано)";
                        TextManager.Text = _application.Users != null
                            ? $"{_application.Users.LastName} {_application.Users.FirstName}"
                            : "(не назначен)";
                        TextMessage.Text = _application.Message ?? "(нет сообщения)";
                    }
                    else
                    {
                        MessageBox.Show("Заявка не найдена", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ApplicationEditWindow(_applicationId);
            if (editWindow.ShowDialog() == true)
            {
                LoadApplication();
            }
        }

        private void BtnAssignToMe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var appToUpdate = db.Applications.Find(_applicationId);
                    if (appToUpdate != null)
                    {
                        appToUpdate.AssignedToUserId = CurrentUser.Id;
                        appToUpdate.StatusId = 2; // В обработке

                        db.SaveChanges();
                        LoadApplication();

                        MessageBox.Show("Заявка успешно назначена вам", "Успешно",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка назначения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}