using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class BusinessPartnerViewModel
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
        public string CardTypeDesc { get; set; }
        public int GroupCode { get; set; }
        public string GroupName { get; set; }

        public string BirthDate { get; set; }
        public string Age { get; set; }
        public string Mobile { get; set; }
        public string E_Mail { get; set; }
        public string ContactPerson { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactMiddleName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactPhone { get; set; }
        public int PaymentTermsCode { get; set; }
        public string PaymentTermsName { get; set; }
        public string WebSite { get; set; }
        public double CreditLimit { get; set; }
        public string ConnectedVendor { get; set; }
        public string ShiptoAddressId { get; set; }
        public string ShiptoStreet { get; set; }
        public string ShiptoCity { get; set; }

        public string ShiptoStateCode { get; set; }
        public string ShiptoStateName { get; set; }
        public string ShiptoCountryCode { get; set; }
        public string ShiptoCountryName { get; set; }
        public string BillToAddressId { get; set; }
        public string BillToStreet { get; set; }
        public string BillToCity { get; set; }
        public string BillToStateCode { get; set; }
        public string BillToStateName { get; set; }
        public string BillToCountryCode { get; set; }
        public string BillToCountryName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Remarks { get; set; }
        public string BankCountry { get; set; }
        public string BankCountryName { get; set; }
        public string BankSwiftCode { get; set; }
        public string BankAccount { get; set; }
        public string AccountName { get; set; }
        public string SAPDocNum { get; set; }
        public string DocEntry { get; set; }


        public string GroupType { get; set; }
        public string GroupTypeDesc { get; set; }

        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public double DownPaymentBalance { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string PaymentTermsGrpCode { get; set; }
        public string PaymentTermsGrpName { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string SwiftCode { get; set; }
        public string Gender { get; set; }
        public string GenderDesc { get; set; }

        public string Type { get; set; }
        public string TypeDesc { get; set; }
        public string HWABUS { get; set; }
        public string RSBRVS { get; set; }
        public string Occupation { get; set; }
        public string OccupationDesc { get; set; }
        public string Relation { get; set; }
        public string RelationDesc { get; set; }
        public string SalesEmployeeCode { get; set; }
        public string SalesEmployeeName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Emergency { get; set; }
        public string ThaneCode { get; set; }
        public string ThanaName { get; set; }
        public string ShipThana { get; set; }
        public string BillThana { get; set; }
        public string BillToZipCode { get; set; }
        public string ShiptoZipCode { get; set; }
        public string Contact { get; set; }
        public string Connected { get; set; }
        public string CreatedBy { get; set; }
        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
    }
}