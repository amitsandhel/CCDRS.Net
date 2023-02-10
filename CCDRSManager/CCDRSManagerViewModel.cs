/*
    Copyright 2023 University of Toronto
    This file is part of CCDRS.
    CCDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    CCDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using CCDRSManager.Data;
using CCDRSManager.Model;

namespace CCDRSManager;

/// <summary>
/// Manage and provides executes the various use cases. 
/// </summary>
public class CCDRSManagerViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// ccdrsRepository variable used and accessed by all methods.
    /// </summary>
    private readonly CCDRSManagerModelRepository _ccdrsRepository = Configuration.CCDRSManagerModelRepository;

    /// <summary>
    /// Observable List of all regions that exist in the database.
    /// </summary>
    public ReadOnlyObservableCollection<RegionModel> Regions { get; }

    private int _regionId;

    private int _surveyYear;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Set the value of the user selected RegionId
    /// </summary>
    public int RegionId
    {
        get
        {
            return _regionId;
        }
        set
        {
            _regionId = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegionId)));
        }
    }

    /// <summary>
    /// The survey year the user inputs.
    /// </summary>
    public int SurveyYear
    {
        get
        {
            return _surveyYear;
        }
        set
        {
            _surveyYear = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SurveyYear)));
        }
    }

    private string _stationFileName = string.Empty;
    public string StationFileName
    {
        get
        {
            return _stationFileName;
        }
        set
        {
            _stationFileName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StationFileName)));
        }

    }

    private string _stationCountObservationFile = string.Empty;
    public string StationCountObservationFile
    {
        get
        {
            return _stationCountObservationFile;
        }
        set
        {
            _stationCountObservationFile = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StationCountObservationFile)));
        }
    }

    /// <summary>
    /// Controls access to the CCDRS model repository.
    /// </summary>
    public CCDRSManagerViewModel()
    {
        Regions = _ccdrsRepository.Regions;
    }

    /// <summary>
    /// Check if the survey exists in the database or not.
    /// </summary>
    public bool CheckSurveyExists()
    {
        return _ccdrsRepository.CheckSurveyExists(RegionId, SurveyYear);
    }

    /// <summary>
    /// Delete the survey data from the database for selected region and year.
    /// </summary>
    public void DeleteSurveyData()
    {
        _ccdrsRepository.DeleteSurveyData(RegionId, SurveyYear);
    }

    /// <summary>
    /// Add survey data to the database.
    /// </summary>
    public void AddSurveyData()
    {
        _ccdrsRepository.AddSurveyData(RegionId, SurveyYear);
    }

    /// <summary>
    /// Add station data to the database.
    /// </summary>
    public void AddStationData()
    {
        _ccdrsRepository.AddStationData(StationFileName, RegionId);
    }

    /// <summary>
    /// Add SurveyStation data to the database.
    /// </summary>
    public void AddSurveyStationData()
    {
        _ccdrsRepository.AddSurveyStationData(RegionId, SurveyYear);
    }

    /// <summary>
    /// Add StationCountObservation data to the database.
    /// </summary>
    public void AddStationCountObserationData()
    {
        _ccdrsRepository.AddStationCountObservationData(StationCountObservationFile, RegionId, SurveyYear);
    }
}
