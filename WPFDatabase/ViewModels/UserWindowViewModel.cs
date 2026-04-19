using System.Windows.Input;
using WPFDatabase.Models;
using WPFDatabase.Services;

namespace WPFDatabase.ViewModels;

public class UserWindowViewModel : ViewModelBase
{
    private readonly User? _editingUser;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isActive = true;
    private string _errorMessage = string.Empty;

    public UserWindowViewModel(User? editingUser = null)
    {
        _editingUser = editingUser;

        if (_editingUser is not null)
        {
            Login = _editingUser.Login;
            IsActive = _editingUser.IsActive;
        }

        SaveCommand = new RelayCommand(ExecuteSave);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(false));
    }

    public event Action<bool?>? CloseRequested;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool IsEditMode => _editingUser is not null;

    public string WindowTitle => IsEditMode ? "Редактирование пользователя" : "Создание пользователя";

    public string SaveButtonText => IsEditMode ? "Сохранить" : "Создать";

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

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private void ExecuteSave()
    {
        ErrorMessage = string.Empty;

        if (IsEditMode && _editingUser is not null)
        {
            var updateResult = AuthService.UpdateUser(_editingUser, Login, Password, ConfirmPassword, IsActive);
            if (!updateResult.Success)
            {
                ErrorMessage = updateResult.ErrorMessage;
                return;
            }

            CloseRequested?.Invoke(true);
            return;
        }

        var createResult = AuthService.CreateUser(Login, Password, ConfirmPassword, IsActive);
        if (!createResult.Success)
        {
            ErrorMessage = createResult.ErrorMessage;
            return;
        }

        CloseRequested?.Invoke(true);
    }
}
