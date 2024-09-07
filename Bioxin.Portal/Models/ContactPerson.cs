using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ContactPerson
    {
		public int CntctCode { get; set; }
		public string CardCode { get; set; }
		public string Name { get; set; }
		public string Position { get; set; }
		public string Address { get; set; }
		public string Tel1 { get; set; }
		public string E_MailL { get; set; }
		public char DataSource { get; set; }
		public Int16? UserSign { get; set; }
		public char Active { get; set; }
		public string Password { get; set; }
		public DateTime? BirthDate { get; set; }
		public char Gender { get; set; }
		public DateTime? CreateDate { get; set; }
		public DateTime? updateDate { get; set; }
	}
}