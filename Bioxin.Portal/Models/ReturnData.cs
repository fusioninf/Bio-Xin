using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ReturnData
    {
        public string ReturnCode { get; set; }
        public string ReturnDocEntry { get; set; }
        public string ReturnObjType { get; set; }
        public string ReturnSeries { get; set; }
        public string ReturnDocNum { get; set; }
        public string ReturnMsg { get; set; }
        public int UnAutorized { get; set; }
    }
}