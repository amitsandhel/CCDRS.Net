using CCDRSManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCDRSManager;

public class SortVehicleModel : INotifyPropertyChanged
{
    private readonly Vehicle _vehicle;

    public int Id
    {
        get
        {
            return _vehicle.Id;
        }
        set
        {
            _vehicle.Id = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
        }
    }

    public string Name
    {
        get
        {
            return _vehicle.Name;
        }
        set
        {
            _vehicle.Name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public int DisplayOrder
    {
        get
        {
            return _vehicle.DisplayOrder;
        }
        set
        {
            _vehicle.DisplayOrder = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayOrder)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
