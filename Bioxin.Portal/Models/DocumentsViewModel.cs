using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentsViewModel
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public double DocTotal { get; set; }
        public decimal VatSum { get; set; }
        public decimal DiscSum { get; set; }
        public string SeriesName { get; set; }
        public string SAPDocNum { get; set; }
        public string DocStatus { get; set; }
        public string DocStatusDesc { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string TaxDate { get; set; }
        public string ShipToCode { get; set; }
        public string FromWhsCode { get; set; }
        public string FromWhsName { get; set; }
        public string FromBranchCode { get; set; }
        public string FromBranchName { get; set; }
        public string ToWhsCode { get; set; }
        public string ToWhsName { get; set; }
        public string ToBranchCode { get; set; }
        public string ToBranchName { get; set; }
        public string ContactPersonCode { get; set; }
        public string SalesEmployeeCode { get; set; }
        public string Mobile { get; set; }
        public string SalesChannel { get; set; }
        public string SalesChannelDesc { get; set; }
        public decimal Freight { get; set; }
        public string DeliveryAddress { get; set; }


        public string CreatedBy { get; set; }
        public string Remarks { get; set; }


        //Goods Receipt 
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        //Goods Receipt 

        public string PatientAge { get; set; }
        public string ShipToAddress { get; set; }
        public string BillToCode { get; set; }
        public string BillToAddress { get; set; }
        public string PatientConcern { get; set; }
        public string DoctorsComment { get; set; }
        public string DoctorsSuggestion { get; set; }
        public string DoctorsObservation { get; set; }
        public string DoctorsCode { get; set; }
        public string DoctorsName { get; set; }
        public string RefNo { get; set; }
        public string InvoiceNo { get; set; }
        public string SODocEntry { get; set; }
        public string SOSAPDocNum { get; set; }
        public string SAPSODocNum { get; set; }
        public string ObjType { get; set; }
        public string LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Session { get; set; }

        //Start Doctors Prescription
        public string PhoneNo { get; set; }
        public string ExternalDoctorsRef { get; set; }
        public string FollowupDate { get; set; }
        //End Doctors Prescription
        public string ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovedDate { get; set; }
        public int SequenceNo { get; set; }
        public string SlpCode { get; set; }
        public string SlpName { get; set; }
        public string ItemTypeCode { get; set; }
        public List<ItemsViewModel> itemsViewModels { get; set; }
        public List<TestsViewModel> testsViewModels { get; set; }
        public List<PaymentView> paymentViews { get; set; }

        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }
    }
}