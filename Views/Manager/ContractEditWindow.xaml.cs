using BizRent.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BizRent.Views.Manager
{
    public partial class ContractEditWindow : Window
    {
        private int _contractId;
        private Contracts _contract;

        public ContractEditWindow(int contractId = 0)
        {
            InitializeComponent();
            _contractId = contractId;
            WindowTitle.Text = contractId == 0 ? "Новый договор" : "Редактирование договора";

            LoadData();

            if (_contractId == 0)
            {
                DatePickerStart.SelectedDate = DateTime.Today;
                DatePickerEnd.SelectedDate = DateTime.Today.AddYears(1);
                TextBoxNumber.Text = GenerateContractNumber();
            }
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    // Помещения
                    var areasQuery = db.Areas.AsQueryable();
                    if (_contractId > 0 && _contract != null)
                    {
                        areasQuery = areasQuery.Where(a => a.StatusId == 1 || a.Id == _contract.AreaId);
                    }
                    else
                    {
                        areasQuery = areasQuery.Where(a => a.StatusId == 1);
                    }

                    ComboArea.ItemsSource = areasQuery.OrderBy(a => a.Name).ToList();
                    ComboArea.DisplayMemberPath = "Name";
                    ComboArea.SelectedValuePath = "Id";

                    // Арендаторы
                    ComboTenant.ItemsSource = db.Tenants.OrderBy(t => t.CompanyName).ToList();
                    ComboTenant.DisplayMemberPath = "CompanyName";
                    ComboTenant.SelectedValuePath = "Id";

                    // Типы договоров ← Важно!
                    ComboType.ItemsSource = db.ContractTypes.OrderBy(t => t.Name).ToList();
                    ComboType.DisplayMemberPath = "Name";
                    ComboType.SelectedValuePath = "Id";

                    // Статусы
                    ComboStatus.ItemsSource = db.ContractStatuses.OrderBy(s => s.Id).ToList();
                    ComboStatus.DisplayMemberPath = "Name";
                    ComboStatus.SelectedValuePath = "Id";

                    if (_contractId > 0)
                    {
                        _contract = db.Contracts.Find(_contractId);
                        if (_contract != null)
                        {
                            TextBoxNumber.Text = _contract.ContractNumber;
                            ComboArea.SelectedValue = _contract.AreaId;
                            ComboTenant.SelectedValue = _contract.TenantId;
                            ComboType.SelectedValue = _contract.ContractTypeId;
                            ComboStatus.SelectedValue = _contract.StatusId;
                            DatePickerStart.SelectedDate = _contract.StartDate;
                            DatePickerEnd.SelectedDate = _contract.EndDate;
                            TextBoxMonthlyPayment.Text = _contract.MonthlyPayment.ToString();
                            TextBoxDeposit.Text = _contract.Deposit.ToString();
                            TextBoxNotes.Text = _contract.Notes;
                        }
                    }
                    else
                    {
                        ComboStatus.SelectedValue = 1;
                        ComboType.SelectedIndex = 0; // первый тип по умолчанию
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateContractNumber()
        {
            return $"ДГ-{DateTime.Today:yyyyMM}-{new Random().Next(100, 999)}";
        }

        private void ComboArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboArea.SelectedItem is Areas selectedArea && _contractId == 0)
            {
                TextBoxMonthlyPayment.Text = selectedArea.PricePerMonth.ToString();
                TextBoxDeposit.Text = (selectedArea.PricePerMonth * 2).ToString();
            }
        }

        private void TextBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxNumber.Text))
            {
                MessageBox.Show("Введите номер договора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxNumber.Focus();
                return;
            }

            if (ComboArea.SelectedValue == null)
            {
                MessageBox.Show("Выберите помещение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboArea.Focus();
                return;
            }

            if (ComboTenant.SelectedValue == null)
            {
                MessageBox.Show("Выберите арендатора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboTenant.Focus();
                return;
            }

            if (ComboType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип договора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboType.Focus();
                return;
            }

            if (!DatePickerStart.SelectedDate.HasValue || !DatePickerEnd.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите даты начала и окончания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    decimal monthly = decimal.TryParse(TextBoxMonthlyPayment.Text, out decimal m) ? m : 0;
                    decimal deposit = decimal.TryParse(TextBoxDeposit.Text, out decimal d) ? d : 0;

                    if (_contractId == 0)
                    {
                        _contract = new Contracts
                        {
                            ContractNumber = TextBoxNumber.Text.Trim(),
                            AreaId = (int)ComboArea.SelectedValue,
                            TenantId = (int)ComboTenant.SelectedValue,
                            ContractTypeId = (int)ComboType.SelectedValue,   // ← Добавлено
                            StatusId = (int)ComboStatus.SelectedValue,
                            StartDate = DatePickerStart.SelectedDate.Value,
                            EndDate = DatePickerEnd.SelectedDate.Value,
                            MonthlyPayment = monthly,
                            Deposit = deposit,
                            CreatedByUserId = CurrentUser.Id,
                            CreatedDate = DateTime.Now,
                            Notes = TextBoxNotes.Text?.Trim()
                        };
                        db.Contracts.Add(_contract);
                    }
                    else
                    {
                        _contract = db.Contracts.Find(_contractId);
                        if (_contract != null)
                        {
                            _contract.ContractNumber = TextBoxNumber.Text.Trim();
                            _contract.AreaId = (int)ComboArea.SelectedValue;
                            _contract.TenantId = (int)ComboTenant.SelectedValue;
                            _contract.ContractTypeId = (int)ComboType.SelectedValue;  // ← Добавлено
                            _contract.StatusId = (int)ComboStatus.SelectedValue;
                            _contract.StartDate = DatePickerStart.SelectedDate.Value;
                            _contract.EndDate = DatePickerEnd.SelectedDate.Value;
                            _contract.MonthlyPayment = monthly;
                            _contract.Deposit = deposit;
                            _contract.Notes = TextBoxNotes.Text?.Trim();
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Договор успешно сохранён!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                string inner = ex.InnerException?.Message ?? "";
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}\n\n{inner}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}