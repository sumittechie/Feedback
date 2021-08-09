using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Users : IdentityUser
    {
        public string Name { get; set; }

        public bool IsAdmin { get; set; }
        public string Photo { get; set; }
        [MaxLength(10)]
        public string Gender { get; set; }
        public List<Tokens> Tokens { get; set; }
    }

}
