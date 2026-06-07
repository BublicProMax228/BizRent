using BizRent.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace BizRent.Views.Accountant
{
    public partial class PaymentCreateWindow : Window
    {
        public PaymentCreateWindow()
        {
            InitializeComponent();
            LoadData();

            DatePickerPeriod.SelectedDate = DateTime.Today;
            DatePickerDueDate.SelectedDate = DateTime.Today.AddDays(10);
        }

        
        private void LoadData()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    
                    ComboContract.ItemsSource = db.Contracts
                        .Where(c => c.StatusId == 2) 
                        .OrderBy(c => c.ContractNumber)
                        .ToList();

                    ComboPaymentType.ItemsSource = db.PaymentTypes
                        .OrderBy(t => t.Id)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBoxAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (ComboContract.SelectedValue == null)
            {
                MessageBox.Show("Выберите договор", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboContract.Focus();
                return;
            }

            if (ComboPaymentType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип платежа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboPaymentType.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxAmount.Text) ||
                !decimal.TryParse(TextBoxAmount.Text, out decimal amount) ||
                amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxAmount.Focus();
                return;
            }

            if (DatePickerPeriod.SelectedDate == null)
            {
                MessageBox.Show("Выберите период", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePickerPeriod.Focus();
                return;
            }

            if (DatePickerDueDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите срок оплаты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePickerDueDate.Focus();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    var payment = new Payments
                    {
                        ContractId = (int)ComboContract.SelectedValue,
                        PaymentTypeId = (int)ComboPaymentType.SelectedValue,
                        StatusId = 1, 
                        Period = DatePickerPeriod.SelectedDate.Value,
                        DueDate = DatePickerDueDate.SelectedDate.Value,
                        Amount = amount,
                        Comment = TextBoxComment.Text?.Trim(),
                        CreatedDate = DateTime.Now
                    };

                    db.Payments.Add(payment);
                    db.SaveChanges();

                    MessageBox.Show($"Платёж на сумму {amount:N0} ₽ создан",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания: {ex.Message}",
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