/*
    Copyright 2022 University of Toronto
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

using CCDRS.Model;

namespace CCDRS
{
    /// <summary>
    /// Contains cached data. Call Initialize before usage.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Private method to generate a list of vehicles and associated occupancy. 
        /// </summary>
        /// <param name="context">The CCDRS context service</param>
        /// <returns>A list of the query with the vehicle id, name and occupancy</returns>
        private static List<(int id, string name)> GenerateNumberOfOccupantsInVehicle(CCDRS.Data.CCDRSContext context)
        {
            return (from vehicle in context.Vehicles
                    join vehicleCount in context.VehicleCountTypes on vehicle.Id equals vehicleCount.VehicleId
                    select new
                    {
                        Id = vehicleCount.Id,
                        Name = vehicle.Name + vehicleCount.Occupancy
                    })
                   // The first ToList finishes the logic for EF,
                   // then we select a value tuple from that since EF can't use ValueTuples
                   .ToList()
                   .Select(x => (x.Id, x.Name))
                   .ToList();
        }

        /// <summary>
        /// List of tuple containing the id and new name of technology with vehicle and occupancy number
        /// </summary>
        public static List<(int id, string name)> TechnologyNames { get; private set; }

        /// <summary>
        /// List of Directions user may select from. Accessible by other pages
        /// </summary>
        public static List<Direction> Directions { get; private set; }

        /// <summary>
        /// Function to run the database query to return a list of all directions available
        /// </summary>
        /// <param name="context">The CCDRSContet service</param>
        /// <returns>A list of the Direction objects</returns>
        private static List<Direction> GenerateDirections(CCDRS.Data.CCDRSContext context)
        {
            return context.Directions.ToList();
        }

        /// <summary>
        /// List of IndividualCategory objects
        /// </summary>
        public static IList<IndividualCategory> IndividualCategories { get; private set; }

        /// <summary>
        /// Method to call the database and run the query and return a list of all rows of individual categories
        /// </summary>
        /// <param name="context">The CCDRSContet service</param>
        /// <returns>A list of IndividualCategory Objects</returns>
        private static List<IndividualCategory> GenerateIndividualCategories(CCDRS.Data.CCDRSContext context)
        {
            return context.IndividualCategories.ToList();
        }

        /// <summary>
        /// Invoke this method to initialize cached data.
        /// </summary>
        /// <param name="context">The CCDRS context service</param>
        public static void Initialize(CCDRS.Data.CCDRSContext context)
        {
            TechnologyNames = GenerateNumberOfOccupantsInVehicle(context);
            Directions = GenerateDirections(context);
            IndividualCategories = GenerateIndividualCategories(context);
        }

        /// <summary>
        /// Constant enumerated value for technologies used to filter the individual
        /// Category table for the radio buttons
        /// </summary>
        private const int TechnologyCounts = 1;

        /// <summary>
        /// Constant enumerated value for total vehicle counts used to filter the individual
        /// Category table for the radio buttons
        /// </summary>
        private const int VehicleCounts = 2;

        /// <summary>
        /// Constant enumerated value for person counts used to filter the individual
        /// Category table for the radio buttons
        /// </summary>
        private const int PersonCounts = 3;

        /// <summary>
        /// Method to return the total vehicle count technologies available for given region and survey. 
        /// </summary>
        /// <param name="regionId">Primary serial key of the selected region.</param>
        /// <param name="selectedSurveyId">Primary serial key of the selected survey.</param>
        /// <returns>A list of the individual categories based on the selected inputs.</returns>
        public static IList<IndividualCategory> GetTotalVehicleCounts(int regionId, int selectedSurveyId)
        {
            return GetSpecificTechnologies(regionId, selectedSurveyId, VehicleCounts);
        }

        /// <summary>
        /// Method to return the total person count technologies available for given survey and region.
        /// </summary>
        /// <param name="regionId">Primary serial key of the selected region.</param>
        /// <param name="selectedSurveyId">Primary serial key of the selected survey.</param>
        /// <returns></returns>
        public static IList<IndividualCategory> GetTotalPersonCounts(int regionId, int selectedSurveyId)
        {
            return GetSpecificTechnologies(regionId, selectedSurveyId, PersonCounts);
        }

        /// <summary>
        /// Method to return the technologies available for given survey and region.
        /// </summary>
        /// <param name="regionId">Primary serial key of the selected region.</param>
        /// <param name="selectedSurveyId">Primary serial key of the selected survey.</param>
        /// <returns></returns>
        public static IList<IndividualCategory> GetTechnologyCounts(int regionId, int selectedSurveyId)
        {
            return GetSpecificTechnologies(regionId, selectedSurveyId, TechnologyCounts);
        }

        /// <summary>
        /// General Method to query the individual_category table and return the filtered technologies
        /// </summary>
        /// <param name="regionId">Primary serial key of the selected region.</param>
        /// <param name="selectedSurveyId">Selected survey which is the survey primary key.</param>
        /// <param name="VehicleCounts">Enumeration to determine the type of vehicle</param>
        /// <returns>a filtered list of Individual Categories </returns>
        private static IList<IndividualCategory> GetSpecificTechnologies(int regionId, int selectedSurveyId, int countType)
        {
            return IndividualCategories.Where(s => s.RegionId == regionId &
                               s.SurveyId == selectedSurveyId &
                               s.CountType == countType
                            ).ToList();
        }
    }
}
