using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Activitys
    {
        public string ActivityDate { get; set; }
        public List<ActivityDetails> Activity { get; set; }
    }
}