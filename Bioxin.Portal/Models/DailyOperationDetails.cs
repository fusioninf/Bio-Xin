using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DailyOperationDetails
    {
        public string Code { get; set; }
        public int LineId { get; set; }
        public string U_WActivitesCode { get; set; }
        public string U_WorkStatus { get; set; }
        public string U_Comments { get; set; }
        public int? U_SignatureCode { get; set; }
    }
}