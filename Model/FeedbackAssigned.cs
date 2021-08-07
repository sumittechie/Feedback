using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class FeedbackAssigned
    {
        public int Id { get; set; }
 
        public int FeedbackId { get; set; }
        public Feedback Feedback { get; set; }

        public string UsersId { get; set; }
        public Users Users { get; set; }

        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
