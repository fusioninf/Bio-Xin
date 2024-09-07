using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ActivityMaster
    {
        public int ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public int ActivityTypeCode { get; set; }
        public string ActivityTypeName { get; set; }
        public int Code { get; set; }
        public string Description { get; set; }
    }
}