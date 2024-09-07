using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class TreatmentFolloupDetail
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Mobile { get; set; }
        public string SAPDocNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public int LineNum { get; set; }
        public string SODate { get; set; }
        public string TreatmentDate { get; set; }
        public string ObjType { get; set; }
        public int DocEntry { get; set; }
        public int TotalSession { get; set; }
        public string SessionTake { get; set; }
        public int SessionNum { get; set; }
        public string FollowupId { get; set; }
        public string FollowupName { get; set; }
        public int ActivityId { get; set; }
        public string Remarks { get; set; }
        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }
    }
}