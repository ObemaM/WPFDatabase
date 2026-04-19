using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WPFDatabase.Models;
using WPFDatabase.Services;

namespace WPFDatabase.ViewModels;

public class UserManagementViewModel : ViewModelBase
{
    private readonly RelayCommand _editUserCommand;
    private readonly RelayCommand _deleteUserCommand;
    private readonly RelayCommand _toggleUserStateCommand;
    private User? _selectedUser;

    public UserManagementViewModel()
    {
        Users = new ObservableCollection<User>();

        AddUserCommand = new RelayCommand(() => AddUserRequested?.Invoke());
        _editUserCommand = new RelayCommand(() => EditUserRequested?.Invoke(SelectedUser!), () => SelectedUser is not null);
        _deleteUserCommand = new RelayCommand(DeleteSelectedUser, () => SelectedUser is not null);
        _toggleUserStateCommand = new RelayCommand(ToggleSelectedUserState, () => SelectedUser is not null);
        RefreshCommand = new RelayCommand(LoadUsers);
        CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());

        LoadUsers();
    }

    public event Action? AddUserRequested;
    public event Action<User>? EditUserRequested;
    public event Action? CloseRequested;

    public ObservableCollection<User> Users { get; }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand => _editUserCommand;
    public ICommand DeleteUserCommand => _deleteUserCommand;
    public ICommand ToggleUserStateCommand => _toggleUserStateCommand;
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (!SetProperty(ref _selectedUser, value))
            {
                return;
            }

            _editUserCommand.RaiseCanExecuteChanged();
            _deleteUserCommand.RaiseCanExecuteChanged();
            _toggleUserStateCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(ToggleUserStateText));
        }
    }

    public string ToggleUserStateText => SelectedUser?.IsActive == true ? "Заблокировать" : "Активировать";

    public void LoadUsers()
    {
        Users.Clear();

        foreach (var user in AuthService.GetUsers())
        {
            Users.Add(user);
        }

        SelectedUser = null;
    }

    private void DeleteSelectedUser()
    {
        if (SelectedUser is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Удалить пользователя {SelectedUser.Login}?",
            "Удаление пользователя",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var deleteResult = AuthService.DeleteUser(SelectedUser);
        if (!deleteResult.Success)
        {
            MessageBox.Show(deleteResult.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        LoadUsers();
    }

    private void ToggleSelectedUserState()
    {
        if (SelectedUser is null)
        {
            return;
        }

        AuthService.ToggleUserActive(SelectedUser);
        LoadUsers();
    }
}
