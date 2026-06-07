using BizRent.Data;
using BizRent.Views.Admin;
using BizRent.Views.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BizRent.Views.Accountant
{
    public partial class ReportsPage : Page
    {
        public class ReportItem
        {
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public string Contract { get; set; }
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; }
        }

        private bool _isDiagramVisible = false;

        public ReportsPage()
        {
            InitializeComponent();

            DatePickerFrom.SelectedDate = new DateTime(2021, 1, 1);
            DatePickerTo.SelectedDate = DateTime.Today;

            GenerateReport();
        }

        private void GenerateReport()
        {
            try
            {
                var fromDate = DatePickerFrom.SelectedDate ?? new DateTime(2021, 1, 1);
                var toDate = DatePickerTo.SelectedDate ?? DateTime.Today;

                if (fromDate > toDate)
                {
                    MessageBox.Show("Дата 'с' не может быть больше даты 'по'", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new DataBaseContext())
                {
                    var payments = db.Payments
                        .Include("Contracts")
                        .Include("PaymentTypes")
                        .Include("PaymentStatuses")
                        .Where(p => p.Period >= fromDate && p.Period <= toDate)
                        .OrderByDescending(p => p.Period)
                        .ToList();

                    // Заполнение таблицы
                    var reportItems = payments.Select(p => new ReportItem
                    {
                        Date = p.Period,
                        Type = p.PaymentTypes?.Name ?? "Неизвестно",
                        Contract = p.Contracts?.ContractNumber ?? "Без договора",
                        Description = p.Comment ?? "Платёж",
                        Amount = p.Amount,
                        Status = p.PaymentStatuses?.Name ?? "Неизвестно"
                    }).ToList();

                    DataGridReport.ItemsSource = reportItems;

                    // Статистика
                    decimal total = payments.Sum(p => p.Amount);
                    decimal paid = payments.Where(p => p.StatusId == 2).Sum(p => p.Amount);
                    decimal overdue = payments.Where(p => p.StatusId == 1 && p.DueDate < DateTime.Today).Sum(p => p.Amount);

                    TextTotalIncome.Text = $"{total:N0} ₽";
                    TextPaid.Text = $"{paid:N0} ₽";
                    TextOverdue.Text = $"{overdue:N0} ₽";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчёта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void BtnToggleView_Click(object sender, RoutedEventArgs e)
        {
            _isDiagramVisible = !_isDiagramVisible;

            if (_isDiagramVisible)
            {
                DataGridReport.Visibility = Visibility.Collapsed;
                DiagramArea.Visibility = Visibility.Visible;
                BtnToggleView.Content = "Отчетность";
                CreateBarChart();
            }
            else
            {
                DataGridReport.Visibility = Visibility.Visible;
                DiagramArea.Visibility = Visibility.Collapsed;
                BtnToggleView.Content = "Диаграмма";
            }
        }

        private void CreateBarChart()
        {
            ChartGrid.Children.Clear();

            var chartData = new List<(string Label, decimal Value, Brush Color)>
    {
        ("Оплачено", 245000, (Brush)FindResource("SuccessBrush")),
        ("Ожидает оплаты", 98000, (Brush)FindResource("WarningBrush")),
        ("Просрочено", 67000, (Brush)FindResource("DangerBrush"))
    };

            if (chartData.Count == 0) return;

            decimal maxValue = chartData.Max(x => x.Value);
            double maxBarHeight = 250;   // Увеличил высоту

            foreach (var item in chartData)
            {
                double height = (double)(item.Value / maxValue) * maxBarHeight;

                var column = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(50, 0, 25, 0)   // увеличил отступы
                };

                // Значение сверху
                var valueText = new TextBlock
                {
                    Text = item.Value.ToString("N0"),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                // Столбец
                var bar = new Rectangle
                {
                    Width = 90,           // увеличил ширину
                    Height = height,
                    Fill = item.Color,
                    RadiusX = 10,
                    RadiusY = 10
                };

                // Подпись
                var label = new TextBlock
                {
                    Text = item.Label,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 90,
                    Margin = new Thickness(0, 15, 0, 0)
                };

                column.Children.Add(valueText);
                column.Children.Add(bar);
                column.Children.Add(label);

                ChartGrid.Children.Add(column);
            }
        }

        private void DatePicker_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Можно оставить пустым
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Отчёт отправлен на печать", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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