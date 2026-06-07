using BizRent.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BizRent.Views
{
    public partial class AreaEditWindow : Window
    {
        private int _areaId;
        private Areas _area;

        public AreaEditWindow(int areaId = 0)
        {
            InitializeComponent();
            _areaId = areaId;
            WindowTitle.Text = areaId == 0 ? "Новое помещение" : "Редактирование помещения";

            LoadData();
            LoadAreaData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    ComboAreaType.ItemsSource = db.AreaTypes.OrderBy(t => t.Name).ToList();
                    ComboStatus.ItemsSource = db.AreaStatuses.OrderBy(s => s.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAreaData()
        {
            if (_areaId > 0)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        _area = db.Areas.Find(_areaId);
                        if (_area != null)
                        {
                            TextBoxName.Text = _area.Name;
                            TextBoxAddress.Text = _area.Address;
                            ComboAreaType.SelectedValue = _area.AreaTypeId;
                            TextBoxFloor.Text = _area.Floor?.ToString() ?? "";
                            TextBoxAreaSqM.Text = _area.AreaSqM.ToString();
                            TextBoxPricePerMonth.Text = _area.PricePerMonth.ToString();
                            ComboStatus.SelectedValue = _area.StatusId;
                            TextBoxDescription.Text = _area.Description;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки помещения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                ComboStatus.SelectedValue = 1;
            }
        }

        private void TextBoxFloor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void TextBoxAreaSqM_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text + e.Text);
        }

        private void TextBoxPricePerMonth_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxName.Text))
            {
                MessageBox.Show("Введите название помещения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxAddress.Text))
            {
                MessageBox.Show("Введите адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxAddress.Focus();
                return;
            }

            if (ComboAreaType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип помещения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboAreaType.Focus();
                return;
            }

            if (!decimal.TryParse(TextBoxAreaSqM.Text, out decimal area) || area <= 0)
            {
                MessageBox.Show("Введите корректную площадь", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxAreaSqM.Focus();
                return;
            }

            if (!decimal.TryParse(TextBoxPricePerMonth.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену за месяц", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxPricePerMonth.Focus();
                return;
            }

            if (ComboStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ComboStatus.Focus();
                return;
            }

            try
            {
                using (var db = new DataBaseContext())
                {
                    if (_areaId == 0) // Создание
                    {
                        _area = new Areas
                        {
                            Name = TextBoxName.Text.Trim(),
                            Address = TextBoxAddress.Text.Trim(),
                            AreaTypeId = (int)ComboAreaType.SelectedValue,
                            AreaSqM = area,
                            PricePerMonth = price,
                            StatusId = (int)ComboStatus.SelectedValue,
                            Description = TextBoxDescription.Text?.Trim()
                        };

                        if (int.TryParse(TextBoxFloor.Text, out int floor))
                            _area.Floor = floor;

                        db.Areas.Add(_area);
                    }
                    else // Редактирование
                    {
                        _area = db.Areas.Find(_areaId);
                        if (_area != null)
                        {
                            _area.Name = TextBoxName.Text.Trim();
                            _area.Address = TextBoxAddress.Text.Trim();
                            _area.AreaTypeId = (int)ComboAreaType.SelectedValue;
                            _area.AreaSqM = area;
                            _area.PricePerMonth = price;
                            _area.StatusId = (int)ComboStatus.SelectedValue;
                            _area.Description = TextBoxDescription.Text?.Trim();

                            if (int.TryParse(TextBoxFloor.Text, out int floor))
                                _area.Floor = floor;
                            else
                                _area.Floor = null;
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Помещение успешно сохранено", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
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