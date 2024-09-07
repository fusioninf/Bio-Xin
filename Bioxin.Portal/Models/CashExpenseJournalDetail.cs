using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CashExpenseJournalDetail
    {
        public string AccountCode { get; set; }
        public string Remarks { get; set; }
        public string EmployeeCode { get; set; }
        public double Anount { get; set; }
    }
}