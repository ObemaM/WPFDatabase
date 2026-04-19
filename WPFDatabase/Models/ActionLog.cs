using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFDatabase.Models;
public class ActionLog
{
    private int _id;
    private int? _userId;
    private User? _user;
    private string _userLoginSnapshot = string.Empty;
    private string _actionType = string.Empty;
    private string _entityType = string.Empty;
    private int _entityId;
    private DateTime _createdAt;
    private string _details = string.Empty;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public int? UserId
    {
        get => _userId;
        set { _userId = value; OnPropertyChanged(); }
    }

    public User? User
    {
        get => _user;
        set { _user = value; OnPropertyChanged(); }
    }

    public string UserLoginSnapshot
    {
        get => _userLoginSnapshot;
        set { _userLoginSnapshot = value; OnPropertyChanged(); }
    }

    public string ActionType
    {
        get => _actionType;
        set { _actionType = value; OnPropertyChanged(); }
    }

    public string EntityType
    {
        get => _entityType;
        set { _entityType = value; OnPropertyChanged(); }
    }

    public int EntityId
    {
        get => _entityId;
        set { _entityId = value; OnPropertyChanged(); }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set { _createdAt = value; OnPropertyChanged(); }
    }

    public string Details
    {
        get => _details;
        set { _details = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
