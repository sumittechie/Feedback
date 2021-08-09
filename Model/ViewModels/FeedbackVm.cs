using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class FeedbackVm
    {
        public int? FeedbackId { get; set; }
        public string Question { get; set; }
        public List<string> Users { get; set; }

    }
}
