using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFDatabase.Models;

public class SmartphoneModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private int _releaseYear;
    private decimal _price;
    private int _ramGb;
    private int _storageGb;
    private int _seriesId;
    private Series? _series;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public int ReleaseYear
    {
        get => _releaseYear;
        set { _releaseYear = value; OnPropertyChanged(); }
    }

    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); }
    }

    public int RamGb
    {
        get => _ramGb;
        set { _ramGb = value; OnPropertyChanged(); }
    }

    public int StorageGb
    {
        get => _storageGb;
        set { _storageGb = value; OnPropertyChanged(); }
    }

    public int SeriesId
    {
        get => _seriesId;
        set { _seriesId = value; OnPropertyChanged(); }
    }

    public Series? Series
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
