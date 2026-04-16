using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFDatabase.Models;

public class User
{
    private int _id;
    private string _login = string.Empty;
    private string _passwordHash = string.Empty;
    private string _salt = string.Empty;
    private DateTime _registeredAt;
    private bool _isActive;
    private ObservableCollection<ActionLog> _actionLogs;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string Login
    {
        get => _login;
        set { _login = value; OnPropertyChanged(); }
    }

    public string PasswordHash
    {
        get => _passwordHash;
        set { _passwordHash = value; OnPropertyChanged(); }
    }

    public string Salt
    {
        get => _salt;
        set { _salt = value; OnPropertyChanged(); }
    }

    public DateTime RegisteredAt
    {
        get => _registeredAt;
        set { _registeredAt = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ActionLog> ActionLogs
    {
        get => _actionLogs;
        set { _actionLogs = value; OnPropertyChanged(); }
    }

    public User()
    {
        _actionLogs = new ObservableCollection<ActionLog>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
