using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class PrimaryData
    {
        public string PrimaryDataType { get; set; }
        public List<OCRG> OCRGs { get; set; }

        public class OCRG
        {
            [Key]
            public int GroupCode { get; set; }
            public string GroupName { get; set; }
            public char GroupType { get; set; }
        }
        public List<OCTG> OCTGs { get; set; }
        public class OCTG
        {
            [Key]
            public int GroupNum { get; set; }
            public string PymntGroup { get; set; }
        }
        public List<OACT> OACTs { get; set; }
        public class OACT
        {
            [Key]
            public string AcctCode { get; set; }
            public string AcctName { get; set; }
        }
        public List<OPLN> OPLNs { get; set; }
        public class OPLN
        {
            [Key]
            public int ListNum { get; set; }
            public string ListName { get; set; }
        }
        public List<OCRY> OCRYs { get; set; }
        public class OCRY
        {
            [Key]
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public List<OCST> OCSTs { get; set; }
        public class OCST
        {
            [Key]
            public string Code { get; set; }
            public string Country { get; set; }
            public string Name { get; set; }
        }
        public List<OCRN> OCRNs { get; set; }
        public class OCRN
        {
            [Key]
            public string CurrCode { get; set; }
            public string CurrName { get; set; }
        }
        public List<OSHP> OSHPs { get; set; }
        public class OSHP
        {
            [Key]
            public int TrnspCode { get; set; }
            public string TrnspName { get; set; }
        }
        public List<OSLP> OSLPs { get; set; }
        public class OSLP
        {
            [Key]
            public int SlpCode { get; set; }
            public string SlpName { get; set; }
        }

        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public string branch { get; set; }
        public int empId { get; set; }
    }
}