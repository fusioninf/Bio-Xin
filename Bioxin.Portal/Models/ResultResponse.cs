using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ResultResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public string msg { get; set; }
        public string data { get; set; }
        public int pk { get; set; }
    }
}