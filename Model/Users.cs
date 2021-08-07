using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Models
{
    public class Users : IdentityUser
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public List<Tokens> Tokens { get; set; }
    }

}
