using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Employee
    {
        public int empID { get; set; }
        public string EmployeeCode { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Active { get; set; }
    }
}