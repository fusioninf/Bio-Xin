using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DailyOperation
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int DocEntry { get; set; }
        public int? UserSign { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? U_EmpCode { get; set; }
        public string EmpName { get; set; }
        public DateTime? U_Date { get; set; }
        public string U_BranchCode { get; set; }
        public string BranchName { get; set; }
        public string U_Session { get; set; }
        public List<DailyOperationDetails> DailyOperationDetailses { get; set; }
    }
}