using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class userlogin_message_from_API
    {
       public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string Branch { get; set; }
        public string AuthToken { get; set; }
        public string EmpId { get; set; }
        public string EmpType { get; set; }

    }
}