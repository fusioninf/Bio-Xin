using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SerialViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string IntrSerial { get; set; }
        public int SysSerial { get; set; }
        public string SuppSerial { get; set; }
        public string ExpDate { get; set; }
        public string InDate { get; set; }
        public double Stock { get; set; }
        public int RowId { get; set; }
    }
}