using BizRent.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BizRent.Views
{
    public partial class TenantEditWindow : Window
    {
        private int _tenantId;
        private Tenants _tenant;

        public TenantEditWindow(int tenantId = 0)
        {
            InitializeComponent();
            _tenantId = tenantId;
            Title = tenantId == 0 ? "Новый арендатор" : "Редактирование арендатора";

            LoadTenantData();
        }

        private void LoadTenantData()
        {
            if (_tenantId > 0)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        _tenant = db.Tenants.Find(_tenantId);
                        if (_tenant != null)
                        {
                            TextBoxCompany.Text = _tenant.CompanyName;
                            TextBoxContact.Text = _tenant.ContactPerson;
                            TextBoxPhone.Text = _tenant.Phone;
                            TextBoxEmail.Text = _tenant.Email;
                            TextBoxINN.Text = _tenant.INN;
                            TextBoxAddress.Text = _tenant.LegalAddress;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных арендатора: {ex.Message}",
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

        private bool IsValidINN(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn))
                return true;

            return Regex.IsMatch(inn, @"^\d{10}$|^\d{12}$");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxCompany.Text.Trim()))
            {
                MessageBox.Show("Введите название компании", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxCompany.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxPhone.Text.Trim()))
            {
                MessageBox.Show("Введите телефон", "Ошибка",
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

            if (!IsValidINN(TextBoxINN.Text))
            {
                MessageBox.Show("ИНН должен содержать 10 или 12 цифр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxINN.Focus();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    if (_tenantId == 0)
                    {
                        _tenant = new Tenants
                        {
                            CompanyName = TextBoxCompany.Text.Trim(),
                            ContactPerson = TextBoxContact.Text?.Trim(),
                            Phone = TextBoxPhone.Text.Trim(),
                            Email = TextBoxEmail.Text?.Trim(),
                            INN = TextBoxINN.Text?.Trim(),
                            LegalAddress = TextBoxAddress.Text?.Trim(),
                            CreatedDate = DateTime.Now
                        };
                        db.Tenants.Add(_tenant);
                    }
                    else
                    {
                        _tenant.CompanyName = TextBoxCompany.Text.Trim();
                        _tenant.ContactPerson = TextBoxContact.Text?.Trim();
                        _tenant.Phone = TextBoxPhone.Text.Trim();
                        _tenant.Email = TextBoxEmail.Text?.Trim();
                        _tenant.INN = TextBoxINN.Text?.Trim();
                        _tenant.LegalAddress = TextBoxAddress.Text?.Trim();
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