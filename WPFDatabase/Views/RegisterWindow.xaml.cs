using System.Windows;
using WPFDatabase.ViewModels;

namespace WPFDatabase.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();

        var viewModel = new RegisterViewModel();
        viewModel.CloseRequested += OnCloseRequested;
        DataContext = viewModel;
    }

    private void OnCloseRequested(bool? dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
}
