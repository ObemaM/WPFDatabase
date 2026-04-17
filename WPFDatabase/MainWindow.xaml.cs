using Microsoft.EntityFrameworkCore;
using WPFDatabase.Models;
using System.Windows;


namespace WPFDatabase;

public partial class MainWindow : Window
{
    private object? _selectedItem;

    private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedItem = e.NewValue;

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

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        var brands = App.DbContext.Brands
            .Include(b => b.Series)
            .ThenInclude(s => s.SmartphoneModels)
            .ToList();

        MainTreeView.ItemsSource = brands;
    }
}
