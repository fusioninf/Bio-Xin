using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ActivityDetails
    {
        public string AssignedEmployeeId { get; set; }
        public string ActivityType { get; set; }
        public string Activity { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string Status { get; set; }
        public string DailyOperationStatus { get; set; }
        public string Comments { get; set; }
        public string TaskDetails { get; set; }
        public string Session { get; set; }
        public string CardCode { get; set; }
        public string SoEntry { get; set; }
        public string LineId { get; set; }
        public string AgentCode { get; set; }
        public string VisitingBranch { get; set; }
        public string Doctor { get; set; }
        public string Therapist { get; set; }
        public string ObjType { get; set; }
        public string HowManySession { get; set; }

        public string ActivityId { get; set; }
        public string ActualVisitDate { get; set; }
        public string DocStatus { get; set; }
        public string CheckedBy { get; set; }
        public string Remarks { get; set; }
    }
}