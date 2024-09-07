using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebSolution.Models;

namespace Portal.Controllers
{
    public class BaseController : Controller
    {
        protected AccessFileViewModel accessFVM;
        protected string branch;
        protected string userId;
        protected string authToken;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            InitializeAccessFile();
            userId = Session[SessionCollection.UserId]?.ToString() ?? "";
            branch = Session[SessionCollection.Branch]?.ToString() ?? "";
            authToken = Session[SessionCollection.AuthToken]?.ToString() ?? "";
        }

        private void InitializeAccessFile()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/AccessFile.xml"));
                //Loop through the selected Nodes.
                foreach (XmlNode node in doc.SelectNodes("/AccessFiles/AccessFile"))
                {
                    accessFVM = (new AccessFileViewModel
                    {
                        IPPort = node["IPPort"].InnerText
                        ,
                        AccessKey = node["AccessKey"].InnerText,
                        RPTPort = node["RPTPort"].InnerText,
                        Database = node["Database"].InnerText
                    });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected async Task<T> CallApi<T>(string url, string method, object data = null)
        {
            WebRequest requestObject = WebRequest.Create($"{accessFVM.IPPort}{url}");
            requestObject.Method = method;
            requestObject.ContentType = "application/json";
            requestObject.Headers.Add("AccessKey", accessFVM.AccessKey);
            requestObject.Headers.Add("Branch", branch);
            requestObject.Headers.Add("UserId", userId);
            requestObject.Headers.Add("AuthToken", authToken);

            HttpWebResponse responseObject = (HttpWebResponse)requestObject.GetResponse();
            Stream dataStream = responseObject.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string msg = reader.ReadToEnd();
            await Task.Delay(0);
            return (new JavaScriptSerializer()).Deserialize<T>(msg);
        }
      
    }
}