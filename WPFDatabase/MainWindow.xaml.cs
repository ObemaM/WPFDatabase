using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFDatabase.Models;
using WPFDatabase.Views;

namespace WPFDatabase;

public partial class MainWindow : Window
{
    private object? _selectedItem;

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedItem = e.NewValue;
        UpdateInfoPanel();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedItem is Series selectedSeries)
        {
            var window = new SmartphoneModelWindow
            {
                Owner = this,
                Title = "Добавить модель"
            };

            if (window.ShowDialog() == true)
            {
                var newModel = new SmartphoneModel
                {
                    Name = window.ModelName,
                    ReleaseYear = window.ReleaseYear,
                    Price = window.Price,
                    RamGb = window.RamGb,
                    StorageGb = window.StorageGb,
                    SeriesId = selectedSeries.Id
                };

                App.DbContext.SmartphoneModels.Add(newModel);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is Brand selectedBrand)
        {
            var window = new SeriesWindow
            {
                Owner = this,
                Title = "Добавить серию"
            };

            if (window.ShowDialog() == true)
            {
                var newSeries = new Series
                {
                    Name = window.SeriesName,
                    Segment = window.Segment,
                    BrandId = selectedBrand.Id
                };

                App.DbContext.Series.Add(newSeries);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is null)
        {
            var window = new BrandWindow
            {
                Owner = this,
                Title = "Добавить бренд"
            };

            if (window.ShowDialog() == true)
            {
                var newBrand = new Brand
                {
                    Name = window.BrandName,
                    Country = window.Country,
                    FoundedYear = window.FoundedYear
                };

                App.DbContext.Brands.Add(newBrand);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else
        {
            MessageBox.Show(
                "Чтобы добавить модель, выберите серию. Чтобы добавить серию, выберите бренд. Чтобы добавить бренд, снимите выделение.",
                "Подсказка",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedItem is SmartphoneModel selectedModel)
        {
            var window = new SmartphoneModelWindow(
                selectedModel.Name,
                selectedModel.ReleaseYear,
                selectedModel.Price,
                selectedModel.RamGb,
                selectedModel.StorageGb)
            {
                Owner = this,
                Title = "Редактировать модель"
            };

            if (window.ShowDialog() == true)
            {
                selectedModel.Name = window.ModelName;
                selectedModel.ReleaseYear = window.ReleaseYear;
                selectedModel.Price = window.Price;
                selectedModel.RamGb = window.RamGb;
                selectedModel.StorageGb = window.StorageGb;

                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is Series selectedSeries)
        {
            var window = new SeriesWindow(selectedSeries.Name, selectedSeries.Segment)
            {
                Owner = this,
                Title = "Редактировать серию"
            };

            if (window.ShowDialog() == true)
            {
                selectedSeries.Name = window.SeriesName;
                selectedSeries.Segment = window.Segment;

                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is Brand selectedBrand)
        {
            var window = new BrandWindow(selectedBrand.Name, selectedBrand.Country, selectedBrand.FoundedYear)
            {
                Owner = this,
                Title = "Редактировать бренд"
            };

            if (window.ShowDialog() == true)
            {
                selectedBrand.Name = window.BrandName;
                selectedBrand.Country = window.Country;
                selectedBrand.FoundedYear = window.FoundedYear;

                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else
        {
            MessageBox.Show(
                "Для редактирования выберите объект в дереве.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedItem is SmartphoneModel selectedModel)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить модель {selectedModel.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.DbContext.SmartphoneModels.Remove(selectedModel);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is Series selectedSeries)
        {
            if (selectedSeries.SmartphoneModels.Any())
            {
                MessageBox.Show(
                    "Нельзя удалить серию, пока в ней есть модели смартфонов.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить серию {selectedSeries.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.DbContext.Series.Remove(selectedSeries);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else if (_selectedItem is Brand selectedBrand)
        {
            if (selectedBrand.Series.Any())
            {
                MessageBox.Show(
                    "Нельзя удалить бренд, пока у него есть серии.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить бренд {selectedBrand.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.DbContext.Brands.Remove(selectedBrand);
                App.DbContext.SaveChanges();
                LoadData();
                ClearSelection();
            }
        }
        else
        {
            MessageBox.Show(
                "Для удаления выберите объект в дереве.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadData()
    {
        var brands = App.DbContext.Brands
            .Include(b => b.Series)
            .ThenInclude(s => s.SmartphoneModels)
            .ToList();

        MainTreeView.ItemsSource = brands;
    }

    private void MainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        if (FindAncestor<Button>(source) is not null ||
            FindAncestor<TreeViewItem>(source) is not null)
        {
            return;
        }

        ClearSelection();
    }

    private void UpdateInfoPanel()
    {
        if (_selectedItem is Brand brand)
        {
            InfoTextBlock.Text = $"Бренд: {brand.Name}\nСтрана: {brand.Country}\nГод основания: {brand.FoundedYear}";
        }
        else if (_selectedItem is Series series)
        {
            InfoTextBlock.Text = $"Серия: {series.Name}\nСегмент: {series.Segment}";
        }
        else if (_selectedItem is SmartphoneModel model)
        {
            InfoTextBlock.Text =
                $"Модель: {model.Name}\nГод: {model.ReleaseYear}\nЦена: {model.Price}\nRAM: {model.RamGb} GB\nПамять: {model.StorageGb} GB";
        }
        else
        {
            InfoTextBlock.Text = "Ничего не выбрано";
        }
    }

    private void ClearSelection()
    {
        if (_selectedItem is not null)
        {
            var treeViewItem = FindTreeViewItem(MainTreeView, _selectedItem);
            if (treeViewItem is not null)
            {
                treeViewItem.IsSelected = false;
            }
        }

        _selectedItem = null;
        UpdateInfoPanel();
        Focus();
    }

    private static TreeViewItem? FindTreeViewItem(ItemsControl parent, object item)
    {
        if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
        {
            return treeViewItem;
        }

        foreach (var childItem in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(childItem) is not TreeViewItem childContainer)
            {
                continue;
            }

            var result = FindTreeViewItem(childContainer, item);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T target)
            {
                return target;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
