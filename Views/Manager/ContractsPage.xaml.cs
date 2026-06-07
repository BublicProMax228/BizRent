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
    public partial class ContractsPage : Page
    {
        private int? _tenantFilterId;

        public ContractsPage(int? tenantId = null)
        {
            InitializeComponent();
            _tenantFilterId = tenantId;

            InitializeFilters();
            LoadContracts();
        }

        private void InitializeFilters()
        {
            ComboFilterStatus.Items.Clear();
            ComboFilterStatus.Items.Add("Все статусы");
            ComboFilterStatus.Items.Add("Черновик");
            ComboFilterStatus.Items.Add("Активен");
            ComboFilterStatus.Items.Add("Завершен");
            ComboFilterStatus.Items.Add("Расторгнут");
            ComboFilterStatus.Items.Add("На согласовании");

            ComboFilterStatus.SelectedIndex = 0;
        }

        private void LoadContracts()
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            try
            {
                DataGridContracts.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var query = db.Contracts
                        .Include("Tenants")
                        .Include("Areas")
                        .Include("ContractStatuses")
                        .AsQueryable();

                    if (_tenantFilterId.HasValue)
                    {
                        query = query.Where(c => c.TenantId == _tenantFilterId.Value);
                    }

                    if (ComboFilterStatus.SelectedIndex > 0)
                    {
                        int statusId = ComboFilterStatus.SelectedIndex;
                        query = query.Where(c => c.StatusId == statusId);
                    }

                    // Фильтр по дате
                    if (DatePickerFrom.SelectedDate.HasValue)
                    {
                        query = query.Where(c => c.StartDate >= DatePickerFrom.SelectedDate.Value);
                    }

                    if (DatePickerTo.SelectedDate.HasValue)
                    {
                        query = query.Where(c => c.EndDate <= DatePickerTo.SelectedDate.Value);
                    }

                    var result = query
                        .OrderByDescending(c => c.StartDate)
                        .ToList();

                    DataGridContracts.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки договоров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== СОБЫТИЯ ==================
        private void ComboFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DatePickerTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ================== КНОПКИ ==================
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContractEditWindow();
            if (dialog.ShowDialog() == true)
                LoadContracts();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var contract = DataGridContracts.SelectedItem as Contracts;
            if (contract != null)
            {
                var dialog = new ContractEditWindow(contract.Id);
                if (dialog.ShowDialog() == true)
                    LoadContracts();
            }
            else
            {
                MessageBox.Show("Выберите договор для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var contract = DataGridContracts.SelectedItem as Contracts;
            if (contract != null)
            {
                if (MessageBox.Show($"Удалить договор №{contract.ContractNumber}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new DataBaseContext())
                        {
                            var toDelete = db.Contracts.Find(contract.Id);
                            if (toDelete != null)
                            {
                                db.Contracts.Remove(toDelete);
                                db.SaveChanges();
                                LoadContracts();
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
        }

        private void DataGridContracts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
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