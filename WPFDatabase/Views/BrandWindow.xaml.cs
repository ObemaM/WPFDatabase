using System.Windows;

namespace WPFDatabase.Views;

public partial class BrandWindow : Window
{
    public string BrandName { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public int FoundedYear { get; private set; }

    public BrandWindow()
    {
        InitializeComponent();
    }

    public BrandWindow(string name, string country, int foundedYear)
    {
        InitializeComponent();

        NameTextBox.Text = name;
        CountryTextBox.Text = country;
        FoundedYearTextBox.Text = foundedYear.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ErrorTextBlock.Text = "Введите название бренда.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CountryTextBox.Text))
        {
            ErrorTextBlock.Text = "Введите страну бренда.";
            return;
        }

        if (!int.TryParse(FoundedYearTextBox.Text, out var foundedYear) ||
            foundedYear < 1900 ||
            foundedYear > DateTime.Now.Year)
        {
            ErrorTextBlock.Text = "Некорректный год основания.";
            return;
        }

        BrandName = NameTextBox.Text.Trim();
        Country = CountryTextBox.Text.Trim();
        FoundedYear = foundedYear;

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
