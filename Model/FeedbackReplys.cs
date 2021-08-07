using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class FeedbackReplys
    {
        [Key]
        public int Id { get; set; }
        public int FeedbackAssignedId { get; set; }
        public FeedbackAssigned FeedbackAssigned { get; set; }

        [Column(TypeName = "text")]
        public string Answer { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; }

    }
}
