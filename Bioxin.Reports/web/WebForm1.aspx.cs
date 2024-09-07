using System;
using CrystalReportCallingFromWeb.App_Code;

namespace CrystalReportCallingFromWeb.web
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        Report_Related objReportRelated = new Report_Related();
        protected void Page_Load(object sender, EventArgs e)
        {            
            int report_id;
            string param_value = "",user_id="", file_path="";

            report_id = Convert.ToInt16(Request.QueryString["id1"].ToString());
            if (Request.QueryString["id2"] != "")
                param_value = Request.QueryString["id2"];
            
            //user_id = Request.QueryString["uid"];
            ViewState["param_value"] = param_value;
            ViewState["report_id"] = report_id;
            ViewState["user_id"] = user_id;
            if(Request.QueryString["id3"]!="1")
            {
                string folderPath = "~/web/PDF/";
                string filename, extension = ".pdf";
                filename = "Report";
                file_path = Server.MapPath(folderPath + filename + extension);
            }           

            if (!IsPostBack)
            {
                BindReport(report_id, param_value, user_id, "1", file_path); //for the first time, report also print
            }
            else
            {
                BindReport(report_id, param_value, user_id, "2", file_path); // when next page is selected, report will not print
            }            
        }
        private void BindReport(int report_id, string param_value, string user_id, string is_print_enable,string file_path)
        {
            if (file_path == "")
            {
                CrystalReportViewer1.ReportSource = objReportRelated.prepare_report_document(report_id, param_value, user_id, is_print_enable);
            }
            else
            {
                Byte[] buffer;
                buffer = objReportRelated.prepare_report_document_pdf(report_id, param_value, user_id, is_print_enable, file_path);

                if (buffer != null)
                {
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-length", buffer.Length.ToString());
                    Response.BinaryWrite(buffer);
                }
            }            
        }

        protected void btnprint_Click(object sender, EventArgs e)
        {
            CrystalReportViewer1.ReportSource = objReportRelated.prepare_report_print(Convert.ToInt32(ViewState["report_id"]),ViewState["param_value"].ToString(), ViewState["user_id"].ToString());
        }

        //protected void barcode_print()
        //{
        //    DataSet ds = new DataSet();
        //    ds = objReportRelated.get_url();
        //    ds.Tables[0].Columns.Add(new DataColumn("Barcode", typeof(byte[])));

        //    //Create an instance of Linear Barcode
        //    //Use DataMatrixCrystal for Data Matrix
        //    //Use PDF417Crystal for PDF417
        //    //Use QRCodeCrystal for QR Code
        //    BarCode barcode = new BarCode();
        //    //Barcode settings
        //    barcode.Symbology = KeepAutomation.Barcode.Symbology.Code11;
        //    barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;

        //    foreach (DataRow dr in ds.Tables[0].Rows)
        //    {
        //        barcode.CodeToEncode = (int)dr["ProductId"] + "";
        //        byte[] imageData = barcode.generateBarcodeToByteArray();
        //        dr["Barcode"] = imageData;
        //    }
        //    CrystalReportSource1.ReportDocument.Load(Server.MapPath("CrystalReport1.rpt"));
        //    CrystalReportSource1.ReportDocument.SetDataSource(ds.Tables[0]);
        //    CrystalReportSource1.DataBind();
        //}
    }
}