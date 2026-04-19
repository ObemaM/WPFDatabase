using System.Windows;
using WPFDatabase.ViewModels;

namespace WPFDatabase.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        var viewModel = new LoginViewModel();
        viewModel.CloseRequested += OnCloseRequested;
        viewModel.RegisterRequested += OnRegisterRequested;

        DataContext = viewModel;
    }

    private LoginViewModel ViewModel => (LoginViewModel)DataContext;

    private void OnCloseRequested(bool? dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }

    private void OnRegisterRequested()
    {
        var registerWindow = new RegisterWindow
        {
            Owner = this
        };

        if (registerWindow.ShowDialog() == true &&
            registerWindow.DataContext is RegisterViewModel registerViewModel)
        {
            ViewModel.Login = registerViewModel.RegisteredLogin;
            ViewModel.Password = string.Empty;
            ViewModel.ErrorMessage = string.Empty;
        }
    }
}
