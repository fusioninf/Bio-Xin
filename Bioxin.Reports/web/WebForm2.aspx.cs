using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CrystalReportCallingFromWeb.web
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.BindReport();
            }
        }
        private void BindReport()
        {
            ReportDocument crystalReport = new ReportDocument();
            crystalReport.Load(Server.MapPath("~/web/bill.rpt"));
            //Customers dsCustomers = GetData();
            //crystalReport.SetDataSource(dsCustomers);
            CrystalReportViewer1.ReportSource = crystalReport;
            CrystalReportViewer1.DisplayGroupTree = false;
        }
    }
}