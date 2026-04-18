using System.Windows;

namespace WPFDatabase.Views;

public partial class SmartphoneModelWindow : Window
{
    public string ModelName { get; private set; } = string.Empty;
    public int ReleaseYear { get; private set; }
    public decimal Price { get; private set; }
    public int RamGb { get; private set; }
    public int StorageGb { get; private set; }

    public SmartphoneModelWindow()
    {
        InitializeComponent();
    }

    public SmartphoneModelWindow(string name, int releaseYear, decimal price, int ramGb, int storageGb)
    {
        InitializeComponent();

        NameTextBox.Text = name;
        ReleaseYearTextBox.Text = releaseYear.ToString();
        PriceTextBox.Text = price.ToString();
        RamTextBox.Text = ramGb.ToString();
        StorageTextBox.Text = storageGb.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ErrorTextBlock.Text = "Введите название модели.";
            return;
        }

        if (!int.TryParse(ReleaseYearTextBox.Text, out int releaseYear) || releaseYear < 2000 || releaseYear > 2100)
        {
            ErrorTextBlock.Text = "Некорректный год выпуска.";
            return;
        }

        if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
        {
            ErrorTextBlock.Text = "Некорректная цена.";
            return;
        }

        if (!int.TryParse(RamTextBox.Text, out int ramGb) || ramGb <= 0)
        {
            ErrorTextBlock.Text = "Некорректное значение RAM.";
            return;
        }

        if (!int.TryParse(StorageTextBox.Text, out int storageGb) || storageGb <= 0)
        {
            ErrorTextBlock.Text = "Некорректное значение памяти.";
            return;
        }

        ModelName = NameTextBox.Text.Trim();
        ReleaseYear = releaseYear;
        Price = price;
        RamGb = ramGb;
        StorageGb = storageGb;

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
