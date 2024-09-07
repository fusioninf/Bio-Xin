using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
	[Table("OCRD")]
	public class BusinessPartner
	{
		[Key]
		public string CardCode { get; set; }
		public string CardName { get; set; }
		public char CardType { get; set; }
		public int DocEntry { get; set; }
		public Int16 GroupCode { get; set; }
		//Payment Terms Code
		public Int16 GroupNum { get; set; }
		public Int16 ListNum { get; set; }
        public decimal? CreditLine { get; set; }
        public decimal? DebtLine { get; set; }
        public string DebPayAcct { get; set; }
		public string BillToDef { get; set; }
		public string Country { get; set; }
		public string State1 { get; set; }
		public string ShipToDef { get; set; }
		public string MailCountr { get; set; }
		public string State2 { get; set; }
		public string Currency { get; set; }
		public char CmpPrivate { get; set; }
		public Int16? ShipType { get; set; }
	    public int? SlpCode { get; set; }
		public string CntctPrsn { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Discount { get; set; }
        public char VatStatus { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public char DataSource { get; set; }
		public Int16? UserSign { get; set; }
		public char sEmployed { get; set; }
		public string VatGroup { get; set; }
		public char Deleted { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public DateTime? ExpireDate { get; set; }
		public int? Territory { get; set; }
	}
}