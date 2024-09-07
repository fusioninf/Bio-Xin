using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Serial
    {
        public int VisOrder { get; set; }
        public string InternalSerialNumber { get; set; }
        public string SystemSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
    }
}