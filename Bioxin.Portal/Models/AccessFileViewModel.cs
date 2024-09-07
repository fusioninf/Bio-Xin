using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class AccessFileViewModel
    {
        public string IPPort { get; set; }
        public string AccessKey { get; set; }
        public string RPTPort { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
    }
}