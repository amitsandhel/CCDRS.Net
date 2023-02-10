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

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CCDRS.Model
{
    /// <summary>
    /// Class that maps to the mongo table
    /// </summary>
    [BsonIgnoreExtraElements]
    public class MongoUser
    {
        /// <summary>
        /// Mongo Serial user key, auto generated. 
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Affiliation of user. 
        /// </summary>
        public string aff { get; set; } = string.Empty;

        /// <summary>
        /// Email address of user.
        /// </summary>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// Phone of user.
        /// </summary>
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// Name of user, First and last name
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Salt value used to decrypt the password.
        /// </summary>
        [BsonRepresentation(BsonType.Binary)]
        public byte[] salt { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Password of user as a hashed value
        /// </summary>
        [BsonRepresentation(BsonType.Binary)]
        public byte[] hashPW { get; set; } = Array.Empty<byte>();
    }
}
