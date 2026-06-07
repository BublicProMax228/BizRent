using BizRent.Data;
using BizRent.Views.Accountant;
using BizRent.Views.Admin;
using BizRent.Views.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BizRent.Views
{
    public partial class AreasPage : Page
    {
        private Dictionary<string, int> _areaTypeMap = new Dictionary<string, int>();

        public AreasPage()
        {
            InitializeComponent();
            InitializeFilters();
            LoadAreas();
        }

        private void InitializeFilters()
        {
            ComboFilterType.Items.Clear();
            ComboFilterStatus.Items.Clear();

            ComboFilterType.Items.Add("Все типы");

            try
            {
                using (var db = new DataBaseContext())
                {
                    var types = db.AreaTypes.OrderBy(t => t.Name).ToList();
                    foreach (var t in types)
                    {
                        ComboFilterType.Items.Add(t.Name);
                        _areaTypeMap[t.Name] = t.Id;
                    }
                }
            }
            catch { }

            ComboFilterType.SelectedIndex = 0;

            ComboFilterStatus.Items.Add("Все статусы");
            ComboFilterStatus.Items.Add("Свободно");
            ComboFilterStatus.Items.Add("Арендовано");
            ComboFilterStatus.Items.Add("На ремонте");
            ComboFilterStatus.Items.Add("Забронировано");
            ComboFilterStatus.Items.Add("Недоступно");
            ComboFilterStatus.SelectedIndex = 0;
        }

        private void LoadAreas()
        {
            ApplyFilters(); // используем общий метод
        }

        private void ApplyFilters()
        {
            try
            {
                DataGridAreas.ItemsSource = null;

                using (var db = new DataBaseContext())
                {
                    var query = db.Areas
                        .Include("AreaTypes")
                        .Include("AreaStatuses")
                        .AsQueryable();

                    // Фильтр по типу
                    if (ComboFilterType.SelectedIndex > 0 && ComboFilterType.SelectedItem != null)
                    {
                        string typeName = ComboFilterType.SelectedItem.ToString();
                        if (_areaTypeMap.ContainsKey(typeName))
                        {
                            int typeId = _areaTypeMap[typeName];
                            query = query.Where(a => a.AreaTypeId == typeId);
                        }
                    }

                    // Фильтр по статусу
                    if (ComboFilterStatus.SelectedIndex > 0)
                    {
                        int statusId = ComboFilterStatus.SelectedIndex;
                        query = query.Where(a => a.StatusId == statusId);
                    }

                    // Поиск
                    var search = TextBoxSearch.Text?.Trim().ToLower();
                    if (!string.IsNullOrEmpty(search))
                    {
                        query = query.Where(a =>
                            (a.Name != null && a.Name.ToLower().Contains(search)) ||
                            (a.Address != null && a.Address.ToLower().Contains(search))
                        );
                    }

                    var result = query.OrderBy(a => a.Name).ToList();
                    DataGridAreas.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AreaEditWindow();
            if (dialog.ShowDialog() == true)
                LoadAreas();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var area = DataGridAreas.SelectedItem as Areas;
            if (area != null)
            {
                var dialog = new AreaEditWindow(area.Id);
                if (dialog.ShowDialog() == true)
                    LoadAreas();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var area = DataGridAreas.SelectedItem as Areas;
            if (area == null) return;

            if (area.StatusId == 2)
            {
                MessageBox.Show("Нельзя удалить арендованное помещение!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Удалить помещение '{area.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        var toDelete = db.Areas.Find(area.Id);
                        if (toDelete != null)
                        {
                            db.Areas.Remove(toDelete);
                            db.SaveChanges();
                            LoadAreas();
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

        private void DataGridAreas_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAreas();
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