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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using CCDRS.Data;
using CCDRS.Model;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace CCDRS.Pages.Account
{
    /// <summary>
    /// Login page to log users in. 
    /// </summary>
    public class LoginModel : PageModel
    {
        // Access Mongodb static settings
        private readonly IConfiguration _config;
        // Access the Mongodb database
        private readonly CCDRS.Data.MongoDBUserService _MongoDBUserService;

        public LoginModel(CCDRS.Data.MongoDBUserService mongoDBUserService, IConfiguration config)
        {
            _MongoDBUserService = mongoDBUserService;
            _config = config;
        }

        /// <summary>
        /// User inputted email address.
        /// </summary>
        [BindProperty]
        [Required]
        [Display(Name = "EmailAddress")]
        public string UserName { get; set; }

        /// <summary>
        /// User inputted password.
        /// </summary>
        [BindProperty]
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        public async Task<IActionResult> OnPostAsync()
        {            
            // Check the password using the pbkdf2 encryption. We take the user password and
            // read the salt value from the mongodb database. We convert the 
            //string salt into a byte array and use the pbkdf2 algorithm to create a new hashed value
            // which is then compared against the values saved in the database. If the value match 
            // then authenticate the user otherwise output an error message
            if (Password is not null)
            {
                var userInfo = await _MongoDBUserService.GetUserByIdAsync(UserName);
                if (userInfo is not null)
                {
                    var dbpassword = Convert.ToBase64String(userInfo.hashPW);
                    string hashedpwd = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: Password,
                        salt: userInfo.salt,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 4096,
                        numBytesRequested: 512));
                    Console.WriteLine($"Hashed PWD: {hashedpwd}");

                    if (hashedpwd != dbpassword)
                    {
                        ModelState.AddModelError("", "Incorrect login and password Please try again");
                        return Page();
                    }
                    else
                    {
                        //sign the user in
                        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        var user = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new[] { new Claim(ClaimTypes.Name, UserName) },
                                scheme
                                )
                            );
                        return SignIn(user, scheme);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect login and password Please try again");
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError("", "Please add a password");
                return Page();
            }

        }
    }
}
