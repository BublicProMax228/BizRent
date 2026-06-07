using BizRent.Data;
using BizRent.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BizRent.Views.Manager;

namespace BizRent.Views.Accountant
{
    public partial class AccountantPage : Page
    {
        public AccountantPage()
        {
            InitializeComponent();
            LoadFinancialStats();
        }

        private void LoadFinancialStats()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var today = DateTime.Today;
                    var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    var monthlyIncome = db.Payments
                        .Where(p => p.StatusId == 2 && 
                                    p.PaidDate >= firstDayOfMonth &&
                                    p.PaidDate <= lastDayOfMonth)
                        .Sum(p => (decimal?)p.Amount) ?? 0;
                    TextMonthlyIncome.Text = $"{monthlyIncome:N0} ₽";

                    var expectedPayments = db.Payments
                        .Where(p => p.StatusId == 1 && 
                                    p.DueDate >= firstDayOfMonth &&
                                    p.DueDate <= lastDayOfMonth)
                        .Sum(p => (decimal?)p.Amount) ?? 0;
                    TextExpectedPayments.Text = $"{expectedPayments:N0} ₽";

                    var overduePayments = db.Payments
                        .Where(p => p.StatusId == 1 && 
                                    p.DueDate < today)
                        .Sum(p => (decimal?)p.Amount) ?? 0;
                    TextOverduePayments.Text = $"{overduePayments:N0} ₽";

                    var paidPayments = db.Payments
                        .Where(p => p.StatusId == 2)
                        .ToList();
                    
                    if (paidPayments.Count > 0)
                    {
                        var average = paidPayments.Average(p => (decimal)p.Amount);
                        TextAveragePayment.Text = $"{average:N0} ₽";
                    }
                    else
                    {
                        TextAveragePayment.Text = "0 ₽";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardPayments_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PaymentsPage());
        }

        private void CardReports_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ReportsPage());
        }

        private void CardContracts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ContractsPage());
        }

        private void BtnRefreshStats_Click(object sender, RoutedEventArgs e)
        {
            LoadFinancialStats();
        }
    }
}