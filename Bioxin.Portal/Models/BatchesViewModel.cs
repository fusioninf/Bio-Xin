using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class BatchesViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string BatchNum { get; set; }
        public string ExpDate { get; set; }
        public string InDate { get; set; }
        public double Stock { get; set; }
        public int RowId { get; set; }
    }
}