using System.Windows.Input;
using WPFDatabase.Services;

namespace WPFDatabase.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(ExecuteRegister);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(false));
    }

    public event Action<bool?>? CloseRequested;

    public ICommand RegisterCommand { get; }
    public ICommand CancelCommand { get; }

    public string RegisteredLogin { get; private set; } = string.Empty;

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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private void ExecuteRegister()
    {
        ErrorMessage = string.Empty;

        var result = AuthService.Register(Login, Password, ConfirmPassword);
        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        RegisteredLogin = Login.Trim();
        CloseRequested?.Invoke(true);
    }
}
