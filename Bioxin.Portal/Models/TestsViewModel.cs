using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class TestsViewModel
    {
        public int DocEntry { get; set; }
        public string Test { get; set; }
        public string TestDesc { get; set; }
        public string TestResult { get; set; }
    }
}