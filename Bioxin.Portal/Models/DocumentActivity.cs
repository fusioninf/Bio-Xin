using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentActivity
    {
        public int ActivityId { get; set; }
        public string ActivityDate { get; set; }
        public string StartTime { get; set; }
        public string StartTime1 { get; set; }
        public string TypeMasterCode { get; set; }
        public string TypeMasterName { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string StatusID { get; set; }
        public string StatusName { get; set; }
        public string CreatedBy { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Comments { get; set; }
        public string TaskDetails { get; set; }
        public string OperationStatus { get; set; }
        public string OperationStatusDesc { get; set; }
        public string SESSION { get; set; }
        public string SessionDesc { get; set; }
        public string CardCode { get; set; }
        public string InvoiceNo { get; set; }
        public string SalesOrder { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string SalesItem { get; set; }
        public int UnAutorized { get; set; }
        public string FilterDate { get; set; }
        public string CardName { get; set; }
        public string Mobile { get; set; }
        public string AgentCode { get; set; }
        public string VisitingBranch { get; set; }
        public string AgentName { get; set; }
        public string VisitingBranchName { get; set; }
        public string SessionNo { get; set; }
        public string DoctorCode { get; set; }
        public string DoctorName { get; set; }
        public string TherapistCode { get; set; }
        public string TherapistName { get; set; }
        public string ActualVisitingDate { get; set; }
        public string CheckedByCode { get; set; }
        public string CheckedByName { get; set; }
        public string Remarks { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
    }
}