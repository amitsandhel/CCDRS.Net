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

namespace CCDRSManager
{
    /// <summary>
    /// ViewModel Class to manage and track property changes 
    /// </summary>
    public class CCDRSManagerViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Observable List of all regions that exist in the database.
        /// </summary>
        public ReadOnlyObservableCollection<RegionModel> Regions { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Initialize the class 
        public CCDRSManagerViewModel() 
        {
            CCDRSManagerModelRepository ccdrsRepository = App.Us.CCDRSManagerModelRepository;
            Regions = ccdrsRepository.Regions;
        }
    }
}
