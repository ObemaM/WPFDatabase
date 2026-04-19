using System.Windows;
using WPFDatabase.Models;
using WPFDatabase.ViewModels;

namespace WPFDatabase.Views;

public partial class UserManagementWindow : Window
{
    public UserManagementWindow()
    {
        InitializeComponent();

        var viewModel = new UserManagementViewModel();
        viewModel.AddUserRequested += OnAddUserRequested;
        viewModel.EditUserRequested += OnEditUserRequested;
        viewModel.CloseRequested += () => Close();

        DataContext = viewModel;
    }

    private UserManagementViewModel ViewModel => (UserManagementViewModel)DataContext;

    private void OnAddUserRequested()
    {
        var window = new UserWindow
        {
            Owner = this
        };

        if (window.ShowDialog() == true)
        {
            ViewModel.LoadUsers();
        }
    }

    private void OnEditUserRequested(User user)
    {
        var trackedUser = App.DbContext.Users.FirstOrDefault(existingUser => existingUser.Id == user.Id);
        if (trackedUser is null)
        {
            return;
        }

        var window = new UserWindow(trackedUser)
        {
            Owner = this
        };

        if (window.ShowDialog() == true)
        {
            ViewModel.LoadUsers();
        }
    }
}
