using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Addresses
    {
		public int LineId { get; set; }
		public string AddressType { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string State { get; set; }
		public string City { get; set; }
		public string PinCode { get; set; }
		public string Defalult { get; set; }
	}
}