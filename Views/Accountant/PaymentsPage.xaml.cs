using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Admin;
using BizRent.Views.Manager;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BizRent.Views.Accountant
{
    public partial class PaymentsPage : Page
    {
        public PaymentsPage()
        {
            InitializeComponent();
            InitializeFilters(); 
            LoadPayments();
        }

        private void InitializeFilters()
        {
            ComboFilter.Items.Clear();

            ComboFilter.Items.Add("Все платежи");
            ComboFilter.Items.Add("Ожидают оплаты");
            ComboFilter.Items.Add("Оплаченные");
            ComboFilter.Items.Add("Просроченные");
            ComboFilter.Items.Add("Отмененные");

            ComboFilter.SelectedIndex = 0;
        }


        private void LoadPayments()
        {
            try
            {
                DataGridPayments.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var payments = db.Payments
                        .Include("Contracts")
                        .Include("PaymentTypes")
                        .Include("PaymentStatuses")
                        .OrderByDescending(p => p.DueDate)
                        .ToList();

                    DataGridPayments.ItemsSource = payments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                DataGridPayments.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    IQueryable<Payments> query = db.Payments
                        .Include("Contracts")
                        .Include("PaymentTypes")
                        .Include("PaymentStatuses");

                    
                    if (ComboFilter.SelectedIndex > 0)
                    {
                        switch (ComboFilter.SelectedIndex)
                        {
                            case 1: 
                                query = query.Where(p => p.StatusId == 1);
                                break;
                            case 2: 
                                query = query.Where(p => p.StatusId == 2);
                                break;
                            case 3: 
                                query = query.Where(p => p.StatusId == 1 && p.DueDate < DateTime.Today);
                                break;
                            case 4: 
                                query = query.Where(p => p.StatusId == 4);
                                break;
                        }
                    }

                    var searchText = TextBoxSearch.Text.Trim();
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query = query.Where(p =>
                            (p.Contracts != null && p.Contracts.ContractNumber.Contains(searchText)) ||
                            (p.Comment != null && p.Comment.Contains(searchText))
                        );
                    }

                    var filteredPayments = query
                        .OrderByDescending(p => p.DueDate)
                        .ToList();

                    DataGridPayments.ItemsSource = filteredPayments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) 
            {
                ApplyFilters();
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PaymentCreateWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadPayments(); 
            }
        }

        private void BtnMarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            var payment = DataGridPayments.SelectedItem as Payments;
            if (payment != null)
            {
                if (payment.StatusId == 2) 
                {
                    MessageBox.Show("Платёж уже отмечен как оплаченный",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"Отметить платёж #{payment.Id} как оплаченный?\nСумма: {payment.Amount:N0} ₽",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DataBaseContext())
                        {
                            var paymentToUpdate = db.Payments.Find(payment.Id);
                            if (paymentToUpdate != null)
                            {
                                paymentToUpdate.StatusId = 2; 
                                paymentToUpdate.PaidDate = DateTime.Today;
                                db.SaveChanges();

                                ApplyFilters(); 
                                MessageBox.Show("Платёж отмечен как оплаченный",
                                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите платёж",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPayments();
            ComboFilter.SelectedIndex = 0;
            TextBoxSearch.Clear();
        }

        private void DataGridPayments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var payment = DataGridPayments.SelectedItem as Payments;
            if (payment != null)
            {
                MessageBox.Show($"Платёж #{payment.Id}\n" +
                               $"Договор: {payment.Contracts?.ContractNumber}\n" +
                               $"Сумма: {payment.Amount:N0} ₽\n" +
                               $"Статус: {payment.PaymentStatuses?.Name}",
                               "Информация о платеже",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
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