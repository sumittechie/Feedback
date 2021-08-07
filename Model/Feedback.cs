using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public string Question { get; set; }
        [MaxLength(255)]
        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }


    }
}
