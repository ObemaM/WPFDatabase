using System.Windows;

namespace WPFDatabase.Views;

public partial class SeriesWindow : Window
{
    public string SeriesName { get; private set; } = string.Empty;
    public string Segment { get; private set; } = string.Empty;

    public SeriesWindow()
    {
        InitializeComponent();
    }

    public SeriesWindow(string name, string segment)
    {
        InitializeComponent();

        NameTextBox.Text = name;
        SegmentTextBox.Text = segment;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            ErrorTextBlock.Text = "Введите название серии.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SegmentTextBox.Text))
        {
            ErrorTextBlock.Text = "Введите сегмент серии.";
            return;
        }

        SeriesName = NameTextBox.Text.Trim();
        Segment = SegmentTextBox.Text.Trim();

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
