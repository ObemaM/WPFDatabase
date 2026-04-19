using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFDatabase.Models;
using WPFDatabase.Services;
using WPFDatabase.Views;
using System.IO;

namespace WPFDatabase;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Brand> _brands = new();
    private Point _dragStartPoint;
    private object? _selectedItem;

    public MainWindow()
    {
        InitializeComponent();
        MainTreeView.ItemsSource = _brands;
        LoadInitialData();
    }

    // Запоминаем текущий выбранный объект дерева, чтобы понимать над чем выполнять CRUD операции
    private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedItem = e.NewValue;
        UpdateInfoPanel();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddSelectedItem();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        EditSelectedItem();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedItem();
    }

    private void OpenUsersButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new UserManagementWindow
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private void AddBrandMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _selectedItem = null;
        UpdateInfoPanel();
        AddBrand();
    }

    private void AddMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            _selectedItem = menuItem.Tag;
            UpdateInfoPanel();
            AddSelectedItem();
        }
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            _selectedItem = menuItem.Tag;
            UpdateInfoPanel();
            EditSelectedItem();
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            _selectedItem = menuItem.Tag;
            UpdateInfoPanel();
            DeleteSelectedItem();
        }
    }

    private void TreeItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            _selectedItem = element.DataContext;
            UpdateInfoPanel();

            var treeViewItem = FindAncestor<TreeViewItem>(element);
            if (treeViewItem is not null)
            {
                treeViewItem.IsSelected = true;
                treeViewItem.Focus();
            }
        }
    }

    private void Model_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void Model_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(null);
        var diff = _dragStartPoint - currentPosition;

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (sender is FrameworkElement element &&
            element.DataContext is SmartphoneModel model)
        {
            DragDrop.DoDragDrop(element, model, DragDropEffects.Move);
        }
    }

    private void Series_DragOver(object sender, DragEventArgs e)
    {
        // Во время переноса разрешаем бросать в серию только объекты типа модели смартфона
        if (e.Data.GetDataPresent(typeof(SmartphoneModel)) &&
            sender is FrameworkElement element &&
            element.DataContext is Series)
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    // Здесь Drag&Drop меняет родительскую серию у модели без полной перезагрузки всего дерева
    private void Series_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(SmartphoneModel)) ||
            sender is not FrameworkElement element ||
            element.DataContext is not Series targetSeries)
        {
            return;
        }

        var model = (SmartphoneModel)e.Data.GetData(typeof(SmartphoneModel))!;
        var sourceSeries = FindParentSeries(model);

        if (sourceSeries is null || sourceSeries.Id == targetSeries.Id)
        {
            return;
        }

        // Меняем и внешний ключ, и навигационное свойство, чтобы модель сразу уехала в новую серию
        model.SeriesId = targetSeries.Id;
        model.Series = targetSeries;

        App.DbContext.SaveChanges();
        LogAction("Move", "SmartphoneModel", model.Id, $"Moved model {model.Name} to series {targetSeries.Name}.");

        // Дерево обновляем локально через коллекции, поэтому заново все из БД не загружаем
        sourceSeries.SmartphoneModels.Remove(model);
        ClearSelection();
    }

    // Бренд можно добавить из правой панели или через контекстное меню на пустой области дерева
    private void AddBrand()
    {
        var window = new BrandWindow
        {
            Owner = this,
            Title = "Добавить бренд"
        };

        if (window.ShowDialog() == true)
        {
            if (IsBrandNameDuplicate(window.BrandName))
            {
                MessageBox.Show(
                    $"Бренд с названием \"{window.BrandName}\" уже существует.",
                    "Ошибка валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var newBrand = new Brand
            {
                Name = window.BrandName,
                Country = window.Country,
                FoundedYear = window.FoundedYear
            };

            App.DbContext.Brands.Add(newBrand);
            if (!TrySaveChanges("Не удалось сохранить бренд. Проверьте уникальность названия и корректность данных."))
            {
                App.DbContext.Entry(newBrand).State = EntityState.Detached;
                return;
            }

            LogAction("Create", "Brand", newBrand.Id, $"Created brand {newBrand.Name}.");

            if (!_brands.Contains(newBrand))
            {
                _brands.Add(newBrand);
            }
            ClearSelection();
        }
    }

    // В зависимости от выбранного узла добавляем либо серию в бренд, либо модель в серию
    private void AddSelectedItem()
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
                if (IsSmartphoneModelNameDuplicate(selectedSeries.Id, window.ModelName))
                {
                    MessageBox.Show(
                        $"Модель с названием \"{window.ModelName}\" уже существует в серии \"{selectedSeries.Name}\".",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

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
                if (!TrySaveChanges("Не удалось сохранить модель. Проверьте уникальность названия в серии и корректность данных."))
                {
                    App.DbContext.Entry(newModel).State = EntityState.Detached;
                    return;
                }

                LogAction("Create", "SmartphoneModel", newModel.Id, $"Created model {newModel.Name}.");

                if (!selectedSeries.SmartphoneModels.Contains(newModel))
                {
                    selectedSeries.SmartphoneModels.Add(newModel);
                }
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
                if (IsSeriesNameDuplicate(selectedBrand.Id, window.SeriesName))
                {
                    MessageBox.Show(
                        $"Серия с названием \"{window.SeriesName}\" уже существует у бренда \"{selectedBrand.Name}\".",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var newSeries = new Series
                {
                    Name = window.SeriesName,
                    Segment = window.Segment,
                    BrandId = selectedBrand.Id
                };

                App.DbContext.Series.Add(newSeries);
                if (!TrySaveChanges("Не удалось сохранить серию. Проверьте уникальность названия у выбранного бренда и корректность данных."))
                {
                    App.DbContext.Entry(newSeries).State = EntityState.Detached;
                    return;
                }

                LogAction("Create", "Series", newSeries.Id, $"Created series {newSeries.Name}.");

                if (!selectedBrand.Series.Contains(newSeries))
                {
                    selectedBrand.Series.Add(newSeries);
                }
                ClearSelection();
            }
        }
        else if (_selectedItem is null)
        {
            AddBrand();
        }
        else
        {
            MessageBox.Show(
                "Невозможно определить, что нужно добавить.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Для редактирования открываем нужное окно по типу выбранной сущности в дереве
    private void EditSelectedItem()
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
                if (IsSmartphoneModelNameDuplicate(selectedModel.SeriesId, window.ModelName, selectedModel.Id))
                {
                    MessageBox.Show(
                        $"Модель с названием \"{window.ModelName}\" уже существует в этой серии.",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                selectedModel.Name = window.ModelName;
                selectedModel.ReleaseYear = window.ReleaseYear;
                selectedModel.Price = window.Price;
                selectedModel.RamGb = window.RamGb;
                selectedModel.StorageGb = window.StorageGb;

                if (!TrySaveChanges("Не удалось сохранить изменения модели. Проверьте уникальность названия в серии и корректность данных."))
                {
                    App.DbContext.Entry(selectedModel).Reload();
                    return;
                }

                LogAction("Update", "SmartphoneModel", selectedModel.Id, $"Updated model {selectedModel.Name}.");
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
                if (IsSeriesNameDuplicate(selectedSeries.BrandId, window.SeriesName, selectedSeries.Id))
                {
                    MessageBox.Show(
                        $"Серия с названием \"{window.SeriesName}\" уже существует у этого бренда.",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                selectedSeries.Name = window.SeriesName;
                selectedSeries.Segment = window.Segment;

                if (!TrySaveChanges("Не удалось сохранить изменения серии. Проверьте уникальность названия у бренда и корректность данных."))
                {
                    App.DbContext.Entry(selectedSeries).Reload();
                    return;
                }

                LogAction("Update", "Series", selectedSeries.Id, $"Updated series {selectedSeries.Name}.");
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
                if (IsBrandNameDuplicate(window.BrandName, selectedBrand.Id))
                {
                    MessageBox.Show(
                        $"Бренд с названием \"{window.BrandName}\" уже существует.",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                selectedBrand.Name = window.BrandName;
                selectedBrand.Country = window.Country;
                selectedBrand.FoundedYear = window.FoundedYear;

                if (!TrySaveChanges("Не удалось сохранить изменения бренда. Проверьте уникальность названия и корректность данных."))
                {
                    App.DbContext.Entry(selectedBrand).Reload();
                    return;
                }

                LogAction("Update", "Brand", selectedBrand.Id, $"Updated brand {selectedBrand.Name}.");
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

    // Перед удалением проверяем, нет ли у объекта дочерних записей
    private void DeleteSelectedItem()
    {
        if (_selectedItem is SmartphoneModel selectedModel)
        {
            var removedModelId = selectedModel.Id;
            var removedModelName = selectedModel.Name;
            var result = MessageBox.Show(
                $"Удалить модель {selectedModel.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var parentSeries = FindParentSeries(selectedModel);

                App.DbContext.SmartphoneModels.Remove(selectedModel);
                App.DbContext.SaveChanges();
                LogAction("Delete", "SmartphoneModel", removedModelId, $"Deleted model {removedModelName}.");

                parentSeries?.SmartphoneModels.Remove(selectedModel);
                ClearSelection();
            }
        }
        else if (_selectedItem is Series selectedSeries)
        {
            var removedSeriesId = selectedSeries.Id;
            var removedSeriesName = selectedSeries.Name;
            if (selectedSeries.SmartphoneModels.Any())
            {
                MessageBox.Show(
                    "Нельзя удалить серию, пока в ней есть модели.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить серию {selectedSeries.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var parentBrand = FindParentBrand(selectedSeries);

                App.DbContext.Series.Remove(selectedSeries);
                App.DbContext.SaveChanges();
                LogAction("Delete", "Series", removedSeriesId, $"Deleted series {removedSeriesName}.");

                parentBrand?.Series.Remove(selectedSeries);
                ClearSelection();
            }
        }
        else if (_selectedItem is Brand selectedBrand)
        {
            var removedBrandId = selectedBrand.Id;
            var removedBrandName = selectedBrand.Name;
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
                $"Удалить бренд {selectedBrand.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                App.DbContext.Brands.Remove(selectedBrand);
                App.DbContext.SaveChanges();
                LogAction("Delete", "Brand", removedBrandId, $"Deleted brand {removedBrandName}.");

                _brands.Remove(selectedBrand);
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

    // Загружаем бренды, серии и модели одним запросом вместе с дочерними коллекциями
    private void LoadInitialData()
    {
        var brands = App.DbContext.Brands
            .Include(b => b.Series)
            .ThenInclude(s => s.SmartphoneModels)
            .ToList();

        _brands.Clear();

        foreach (var brand in brands)
        {
            _brands.Add(brand);
        }
    }

    // Клик по пустой области окна снимает выделение с дерева
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

    // Правая панель показывает краткую информацию о том объекте, который сейчас выбран в дереве
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

    // После операций снимаем выбор, чтобы интерфейс не держался за уже измененный или удаленный объект
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

    // Ищем реальный TreeViewItem для объекта, чтобы уметь снять с него выделение
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

    // Поиск родителя в дереве
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

    private Brand? FindParentBrand(Series series)
    {
        return _brands.FirstOrDefault(brand => brand.Series.Contains(series));
    }

    private Series? FindParentSeries(SmartphoneModel model)
    {
        return _brands
            .SelectMany(brand => brand.Series)
            .FirstOrDefault(series => series.SmartphoneModels.Contains(model));
    }

    // Дубли имен проверяем заранее, чтобы показать ошибку еще до SaveChanges
    private bool IsBrandNameDuplicate(string brandName, int? excludedBrandId = null)
    {
        var normalizedName = brandName.Trim();
        return App.DbContext.Brands.Any(brand =>
            brand.Name == normalizedName &&
            (!excludedBrandId.HasValue || brand.Id != excludedBrandId.Value));
    }

    private bool IsSeriesNameDuplicate(int brandId, string seriesName, int? excludedSeriesId = null)
    {
        var normalizedName = seriesName.Trim();
        return App.DbContext.Series.Any(series =>
            series.BrandId == brandId &&
            series.Name == normalizedName &&
            (!excludedSeriesId.HasValue || series.Id != excludedSeriesId.Value));
    }

    private bool IsSmartphoneModelNameDuplicate(int seriesId, string modelName, int? excludedModelId = null)
    {
        var normalizedName = modelName.Trim();
        return App.DbContext.SmartphoneModels.Any(model =>
            model.SeriesId == seriesId &&
            model.Name == normalizedName &&
            (!excludedModelId.HasValue || model.Id != excludedModelId.Value));
    }

    // Общая обертка над SaveChanges нужна, чтобы одинаково обрабатывать ошибки сохранения в разных CRUD операциях
    private bool TrySaveChanges(string errorMessage)
    {
        try
        {
            App.DbContext.SaveChanges();
            return true;
        }
        catch (DbUpdateException)
        {
            MessageBox.Show(
                errorMessage,
                "Ошибка сохранения",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }
    }

    // Экспорт дерева в JSON
    private void ExportToJson(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json", FileName = "smartphones.json"
        };

        if (dialog.ShowDialog() != true) {
            return;
        }

        var exportData = BuildExportData();
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(dialog.FileName, json);
        LogAction("Export", "TreeView", 0, $"Exported tree data to JSON file {Path.GetFileName(dialog.FileName)}.");
    }

    // Экспорт дерева в XML
    private void ExportToXml(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "XML files (*.xml)|*.xml",
            FileName = "smartphones.xml"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var exportData = BuildExportData();
        var serializer = new XmlSerializer(typeof(List<BrandExportDTO>));

        using var stream = File.Create(dialog.FileName);
        serializer.Serialize(stream, exportData);
        LogAction("Export", "TreeView", 0, $"Exported tree data to XML file {Path.GetFileName(dialog.FileName)}.");
    }

    // DTO для экспорта данных в JSON

    private class BrandExportDTO
    {
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public int FoundedYear { get; set; }
        public List<SeriesExportDTO> Series { get; set; } = [];
    }

    private class SeriesExportDTO
    {
        public string Name { get; set; } = null!;
        public string Segment { get; set; } = null!;
        public List<SmartphoneModelExportDTO> SmartphoneModels { get; set; } = [];
    }

    private class SmartphoneModelExportDTO
    {
        public string Name { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public decimal Price { get; set; }
        public int RamGb { get; set; }
        public int StorageGb { get; set; }
    }


    // Метод для построения структуры данных для экспорта в JSON
    private List<BrandExportDTO> BuildExportData()
    {
        return _brands.Select(brand => new BrandExportDTO
        {
            Name = brand.Name,
            Country = brand.Country,
            FoundedYear = brand.FoundedYear,
            Series = brand.Series.Select(series => new SeriesExportDTO
            {
                Name = series.Name,
                Segment = series.Segment,
                SmartphoneModels = series.SmartphoneModels.Select(model => new SmartphoneModelExportDTO
                {
                    Name = model.Name,
                    ReleaseYear = model.ReleaseYear,
                    Price = model.Price,
                    RamGb = model.RamGb,
                    StorageGb = model.StorageGb
                }).ToList()
            }).ToList()
        }).ToList();
    }

    private static void LogAction(string actionType, string entityType, int entityId, string details)
    {
        LogService.Log(App.CurrentUser, actionType, entityType, entityId, details);
    }
}
