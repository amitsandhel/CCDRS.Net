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

using CCDRSManager.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace CCDRSManager
{

    /// <summary>
    /// Class to provides access to the persistent storage?
    /// </summary>
    public class CCDRSManagerModelRepository
    {
        private readonly CCDRSContext _context;
        private ObservableCollection<RegionModel> _regionsModel;

        // Initialize the CCDRS class
        public CCDRSManagerModelRepository(CCDRSContext context)
        {
            _context = context;
            _regionsModel = new ObservableCollection<RegionModel>(_context.Regions.Select(r => new RegionModel(r)));
        }

        /// <summary>
        /// Property to get a list of all Regions that exist in the database.
        /// </summary>
        public ReadOnlyObservableCollection<RegionModel> Regions
        {
            get => new ReadOnlyObservableCollection<RegionModel>(_regionsModel);
        }
        
        /// <summary>
        /// Checks if the survey exists or not.
        /// </summary>
        public bool CheckSurveyExists(int regionId, int surveyYear)
        {
            // find one instance of the survey data
            return (from surveys in _context.Surveys
                            join regions in _context.Regions on surveys.RegionId equals regions.Id
                            where
                               regions.Id == regionId
                               && surveys.Year == surveyYear
                            select
                              surveys
                    ).Any();
        }
    }
}
