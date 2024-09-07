using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class LoginUser
    {
        public string userName { get; set; }
        public string password { get; set; }
        public bool isRemember { get; set; }
    }
}