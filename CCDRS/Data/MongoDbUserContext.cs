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

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using CCDRS.Model;
using MongoDB.Bson;


namespace CCDRS.Data
{
    /// <summary>
    /// Class for creating a service context for connecting to the MongoDb database.
    /// </summary>
    public class MongoDBUserContext
    {
        /// <summary>
        /// Url to where the mongodb is to be connected to. E.g localhost. This includes the port number as well.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Name of the Mongo database to connect to.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Name of Collection to connect to.
        /// </summary>
        public string? UserCollectionsName { get; set; }
    }

    /// <summary>
    /// Mongo Database user service to connect to the mongo database 
    /// </summary>
    public class MongoDBUserService
    {
        private readonly IMongoCollection<MongoUser> _mongoUsersCollection;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public MongoDBUserService(IOptions<MongoDBUserContext> context)
        {
            var mongoClient = new MongoClient(context.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(context.Value.DatabaseName);

            _mongoUsersCollection = mongoDatabase.GetCollection<MongoUser>(context.Value.UserCollectionsName);
        }
        
        //Method to get the User data.
        public async Task<List<MongoUser>> GetUserAsync()
        {
            return await _mongoUsersCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Method to get the User data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MongoUser?> GetUserByIdAsync(string id)
        {
            return await _mongoUsersCollection.Find(x => x.email == id).FirstOrDefaultAsync();
        }
    }
}
