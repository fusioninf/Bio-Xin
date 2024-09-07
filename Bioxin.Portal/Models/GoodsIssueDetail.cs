using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class GoodsIssueDetail
    {
        public int VisOrder { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string EmployeeCostCenter { get; set; }
        public string DepartmentCostCenter { get; set; }
        public string MachineCostCenter { get; set; }
        public List<Batches> Batches { get; set; }
        public List<Serial> Serial { get; set; }
    }
}