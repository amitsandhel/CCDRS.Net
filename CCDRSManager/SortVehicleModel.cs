using CCDRSManager.Model;
using System.ComponentModel;

namespace CCDRSManager;

public class SortVehicleModel : INotifyPropertyChanged
{
    private readonly Vehicle _vehicle;

    /// <summary>
    /// Primary serial key of type int that is auto generated
    /// </summary>
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

    /// <summary>
    /// Name of the vehicle
    /// </summary>
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

    /// <summary>
    /// Display order number used to determine the hierarchy of the technologies to display in the list.
    /// </summary>
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

    /// <summary>
    /// Initialize the SortVehicleModel.
    /// </summary>
    /// <param name="vehicle"></param>
    public SortVehicleModel(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
