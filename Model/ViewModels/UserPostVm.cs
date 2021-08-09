using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class UserPostVm    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
    }
}
