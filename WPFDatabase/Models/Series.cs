using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPFDatabase.Models;

public class Series : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _segment = string.Empty;
    private int _brandId;
    private Brand? _brand;
    private ObservableCollection<SmartphoneModel> _smartphoneModels;

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

    public string Segment
    {
        get => _segment;
        set { _segment = value; OnPropertyChanged(); }
    }

    public int BrandId
    {
        get => _brandId;
        set { _brandId = value; OnPropertyChanged(); }
    }

    public Brand? Brand
    {
        get => _brand;
        set { _brand = value; OnPropertyChanged(); }
    }

    public ObservableCollection<SmartphoneModel> SmartphoneModels
    {
        get => _smartphoneModels;
        set { _smartphoneModels = value; OnPropertyChanged(); }
    }

    public Series()
    {
        _smartphoneModels = new ObservableCollection<SmartphoneModel>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
