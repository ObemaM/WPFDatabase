using System.Windows.Input;
using WPFDatabase.Services;

namespace WPFDatabase.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(ExecuteLogin);
        RegisterCommand = new RelayCommand(() => RegisterRequested?.Invoke());
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(false));
    }

    public event Action<bool?>? CloseRequested;
    public event Action? RegisterRequested;

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand CancelCommand { get; }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private void ExecuteLogin()
    {
        ErrorMessage = string.Empty;

        var result = AuthService.Authenticate(Login, Password);
        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        CloseRequested?.Invoke(true);
    }
}
