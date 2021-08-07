using Microsoft.AspNetCore.Identity;
using Models;
using System.Collections.Generic;

namespace Data.Identity
{
    public class Users : IdentityUser
    {
        public List<Tokens> Tokens { get; set; }
    }

}
