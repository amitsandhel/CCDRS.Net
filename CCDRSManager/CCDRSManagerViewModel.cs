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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    /// <summary>
    /// Name of stationfile csv file.
    /// </summary>
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

    /// <summary>
    /// Name of StationCountObservation CCDRS csv file.
    /// </summary>
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
    /// Property of the progress bar to check if it is running or not.
    /// </summary>
    private bool _isRunning;
    public bool IsRunning
    {
        get { return _isRunning; }
        set
        {
            _isRunning = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }
    }

    private string _screenlineFileName = string.Empty;

    /// <summary>
    /// Name of screenline csv file.
    /// </summary>
    public string ScreenlineFileName
    {
        get
        {
            return _screenlineFileName;
        }
        set
        {
            _screenlineFileName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenlineFileName)));
        }
    }

    /// <summary>
    /// Enum of all the steps to upload a survey.
    /// </summary>
    public enum ImportSurveyStep
    {
        IntroPage = 0,
        Page1 = 1,
        Page2 = 2,
        Page3 = 3,
        LastPage = 4
    }

    private ImportSurveyStep _currentSurveyStep;
    /// <summary>
    /// Get the current page the wizard is on when the next button is clicked.
    /// </summary>
    public ImportSurveyStep CurrentSurveyStep
    {
        get
        {
            return _currentSurveyStep;
        }
        set
        {
            _currentSurveyStep = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSurveyStep)));
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

    /// <summary>
    /// Add Screenline data to the database.
    /// </summary>
    public void AddScreenlineData()
    {
        _ccdrsRepository.AddScreenlineData(RegionId, ScreenlineFileName);
    }

    /// <summary>
    /// Increment the counter to determine the next step to execute.
    /// </summary>
    internal void GoToNextStep()
    {
        if (CurrentSurveyStep != ImportSurveyStep.LastPage)
        {
            CurrentSurveyStep = (ImportSurveyStep)((int)CurrentSurveyStep + 1);
        }
    }

    /// <summary>
    /// Decrement the counter to return to the previous step.
    /// </summary>
    internal void GoToPreviousStep()
    {
        if (CurrentSurveyStep != ImportSurveyStep.IntroPage)
        {
            CurrentSurveyStep = (ImportSurveyStep)((int)CurrentSurveyStep - 1);
        }
    }
}
