using System.Windows;
using WPFDatabase.Models;
using WPFDatabase.ViewModels;

namespace WPFDatabase.Views;

public partial class UserWindow : Window
{
    public UserWindow(User? user = null)
    {
        InitializeComponent();

        var viewModel = new UserWindowViewModel(user);
        viewModel.CloseRequested += OnCloseRequested;
        DataContext = viewModel;
    }

    private void OnCloseRequested(bool? dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
}
