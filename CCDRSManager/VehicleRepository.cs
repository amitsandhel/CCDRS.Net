using CCDRSManager.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CCDRSManager;

public class VehicleRepository
{
    private readonly CCDRSContext _context;
    
    private readonly ObservableCollection<VehicleCountTypeModel> _vehiclesModel;

    /// <summary>
    /// Initialize VehicleRepository and get all the vehiclCountType objects in the database on page load.
    /// </summary>
    /// <param name="context">instance of CCDRSContext.</param>
    public VehicleRepository(CCDRSContext context)
    {
        _context = context;
         _vehiclesModel = new ObservableCollection<VehicleCountTypeModel>(_context.VehicleCountTypes.Select(r => new VehicleCountTypeModel(r)));
    }

    /// <summary>
    /// Method to get a list of all vehicle_count_types that exist in the database.
    /// </summary>
    public ObservableCollection<VehicleCountTypeModel> Vehicles
    {
        get => new(_vehiclesModel);
    }

    /// <summary>
    /// Method that updates the VehicleCountType object in the database.
    /// </summary>
    /// <param name="vehicleCountTypeId">Primary serial key of the VehicleCountType object.</param>
    /// <param name="header">Name of the column e.g. Description and Occupancy.</param>
    /// <param name="newValue">Value to update and edit the database.</param>
    public void UpdateVehicleCountTypeData(int vehicleCountTypeId, string header, string newValue)
    {
        var item = _context.VehicleCountTypes.Find(vehicleCountTypeId);
        if (header == "Description")
        {
            item.GetType().GetProperty(header).SetValue(item, newValue);
        }
        else
        {
            item.GetType().GetProperty(header).SetValue(item, int.Parse(newValue));
        }
        
        _context.SaveChanges();

    }
}
