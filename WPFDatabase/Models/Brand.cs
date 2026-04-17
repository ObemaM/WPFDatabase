using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFDatabase.Models; 

public class Brand : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _country = string.Empty;
    private int _foundedYear;
    private ObservableCollection<Series> _series;
    public Brand()
    {
        _series = new ObservableCollection<Series>();
    }

    public int Id
    {
        get => _id;
        // Поле переписывается и после вызывается хэндлер
        set { _id = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Country
    {
        get => _country;
        set { _country = value; OnPropertyChanged(); }
    }

    public int FoundedYear
    {
        get => _foundedYear;
        set { _foundedYear = value; OnPropertyChanged(); }
    }

    // ObservableCollection - коллекция, которая уведомляет UI о добавлении/удалении элементов
    public ObservableCollection<Series> Series
    {
        get => _series;
        set { _series = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

