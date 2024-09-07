using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DoctorsPrescription
    {
        public string CardCode { get; set; }
        public string Branch { get; set; }
        public string PostingDate { get; set; }
        public string DueDate { get; set; }
        public string RefNo { get; set; }
        public string RefDate { get; set; }
        public string PhoneNo { get; set; }
        public string ExternalDoctorsRef { get; set; }
        public string ShiptoCode { get; set; }
        public string BilltoCode { get; set; }
        public string PatientAge { get; set; }
        public string PatientConcern { get; set; }
        public string DoctorsCode { get; set; }
        public string DoctorsComment { get; set; }
        public string DoctorSuggestion { get; set; }
        public string DoctorObservation { get; set; }
        public string Remarks { get; set; }
        public string FollowupDate { get; set; }
        public string InvoiceNo { get; set; }
        public List<DoctorsPrescriptionDetail> Items { get; set; }
        public List<DoctorsPrescriptionTestDetail> Testing { get; set; }
    }
}