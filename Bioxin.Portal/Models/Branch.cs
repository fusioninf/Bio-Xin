using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Branch
    {
        public string PrcCode { get; set; }
        public string PrcName { get; set; }
        public string GrpCode { get; set; }
        public double Balance { get; set; }
        public string Locked { get; set; }
        public string DataSource { get; set; }
        public int UserSign { get; set; }
        public int DimCode { get; set; }
        public string CCTypeCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Active { get; set; }
        public int LogInstanc { get; set; }

        public int? UserSign2 { get; set; }
        public DateTime UpdateDate { get; set; }
        public string CCOwner { get; set; }
    }
}