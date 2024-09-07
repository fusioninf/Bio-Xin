using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class MemberRegistration
    {
		public string BPType { get; set; }
		public string BPCode { get; set; }
		public string Branch { get; set; }
		public string BPName { get; set; }
		public string BPGroupCode { get; set; }
		public string MobileNo { get; set; }
		public string Email { get; set; }
		public string WebSite { get; set; }
		public string PaymentTerms { get; set; }
		public string CreditLimit { get; set; }
		public string Remarks { get; set; }
		public string BankCode { get; set; }
		public string AccountHolderName { get; set; }
		public string BankAccountNo { get; set; }
		public string BankSwiftCode { get; set; }
		public string BirthDate { get; set; }
		public string Gender { get; set; }
		public string Emergency { get; set; }
		public string Occupation { get; set; }
		public string RelationShip { get; set; }
		public string Connected { get; set; }
		public string HowDoYouHear { get; set; }
		public string ReasonBranchVisit { get; set; }
		public string SalesEmployee { get; set; }
		public string Contact { get; set; }
		public string LeadtoCustomerDate { get; set; }
		public List<Addresses> Addresses { get; set; }
        public List<Contacts> Contacts { get; set; }
	}
}