using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentsCashEntryDetail
    {
        public string AcctCode { get; set; }
        public string AcctName { get; set; }
        public double Amount { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string LineMemo { get; set; }

    }
}