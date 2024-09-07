using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Net;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Sap.Data.Hana;

namespace CrystalReportCallingFromWeb.App_Code
{
    public class Report_Related
    {
        DataAccessSqlServer objDataAccess = new DataAccessSqlServer();
        public ReportDocument prepare_report_document(int report_id, string param_value, string user_id, string is_print_enable)
        {
         
            try
            {
                DataTable dt = new DataTable();
                ReportDocument crystalReport = new ReportDocument();
                string rpt_file_url, report_param_list, auto_print_applicable, source_db;
                int no_of_report_parameter;
                string[] array_param_name;
                string[] array_param_value;

                dt = get_url_for_report(report_id);
                rpt_file_url = dt.Rows[0]["rpt_file_url"].ToString();
                report_param_list = dt.Rows[0]["report_param_list"].ToString();
                no_of_report_parameter = Convert.ToInt16(dt.Rows[0]["no_of_report_parameter"].ToString());

                auto_print_applicable = dt.Rows[0]["auto_print_applicable"].ToString();
                source_db = dt.Rows[0]["source_db"].ToString();
                crystalReport.Load(System.Web.Hosting.HostingEnvironment.MapPath(rpt_file_url));
                string strConnection = String.Format("DRIVER={0};UID={1};PWD={2};SERVERNODE={3};DATABASE={4};", objDataAccess._Driver, objDataAccess._dbUid, objDataAccess._dbPwd, objDataAccess._dbServer, objDataAccess._CompanyDB);
                NameValuePairs2 logonProps2 = crystalReport.DataSourceConnections[0].LogonProperties;
                logonProps2.Set("Provider", objDataAccess._Driver);
                logonProps2.Set("Server Type", objDataAccess._Driver);
                logonProps2.Set("Connection String", strConnection);
                crystalReport.DataSourceConnections[0].SetLogonProperties(logonProps2);
                crystalReport.DataSourceConnections[0].SetConnection(objDataAccess._dbServer, objDataAccess._CompanyDB, false);

                if (no_of_report_parameter > 0)
                {
                    array_param_name = new string[no_of_report_parameter];
                    array_param_value = new string[no_of_report_parameter];

                    array_param_name = report_param_list.Split('|');
                    array_param_value = param_value.Split('|');

                    for (int i = 0; i < no_of_report_parameter; i++)
                    {
                        crystalReport.SetParameterValue(i, array_param_value[i].ToString());
                    }
                }

                if (is_print_enable == "1" && auto_print_applicable == "Y")
                {
                    crystalReport.PrintToPrinter(1, false, 1, 50);
                }
                return (crystalReport);
            }
            catch(Exception ex)
            {
                string message = ex.Message;
                throw;
            }
        }

        public Byte[] prepare_report_document_pdf(int report_id, string param_value, string user_id, string is_print_enable, string file_path)
        {
            DataTable dt = new DataTable();
            ReportDocument crystalReport = new ReportDocument();
            string rpt_file_url, report_param_list, auto_print_applicable, source_db;
            int no_of_report_parameter;
            string[] array_param_name;
            string[] array_param_value;
            try
            {
                dt = get_url_for_report(report_id);
                rpt_file_url = dt.Rows[0]["rpt_file_url"].ToString();
                report_param_list = dt.Rows[0]["report_param_list"].ToString();
                no_of_report_parameter = Convert.ToInt16(dt.Rows[0]["no_of_report_parameter"].ToString());

                auto_print_applicable = dt.Rows[0]["auto_print_applicable"].ToString();
                source_db = dt.Rows[0]["source_db"].ToString();
                crystalReport.Load(System.Web.Hosting.HostingEnvironment.MapPath(rpt_file_url));
                string strConnection = String.Format("DRIVER={0};UID={1};PWD={2};SERVERNODE={3};DATABASE={4};", objDataAccess._Driver, objDataAccess._dbUid, objDataAccess._dbPwd, objDataAccess._dbServer, objDataAccess._CompanyDB);
                NameValuePairs2 logonProps2 = crystalReport.DataSourceConnections[0].LogonProperties;
                logonProps2.Set("Provider", objDataAccess._Driver);
                logonProps2.Set("Server Type", objDataAccess._Driver);
                logonProps2.Set("Connection String", strConnection);
                crystalReport.DataSourceConnections[0].SetLogonProperties(logonProps2);
                crystalReport.DataSourceConnections[0].SetConnection(objDataAccess._dbServer,objDataAccess._CompanyDB,false);
                if (no_of_report_parameter > 0)
                {
                    array_param_name = new string[no_of_report_parameter];
                    array_param_value = new string[no_of_report_parameter];

                    array_param_name = report_param_list.Split('|');
                    array_param_value = param_value.Split('|');

                    for (int i = 0; i < no_of_report_parameter; i++)
                    {
                        crystalReport.SetParameterValue(i, array_param_value[i].ToString());
                    }
                }

                if (is_print_enable == "1" && auto_print_applicable == "Y")
                {
                    System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                    printerSettings.PrinterName = "POS-58-Series";
                    crystalReport.PrintToPrinter(printerSettings, new PageSettings(), false);
                }
                crystalReport.ExportToDisk(ExportFormatType.PortableDocFormat, file_path);
                WebClient client = new WebClient();
                Byte[] buffer = client.DownloadData(file_path);

                return (buffer);
            }
            catch
            {
                throw;
            }
            finally
            {
                crystalReport.Close();
                crystalReport.Dispose();
                GC.Collect();
            }
        }

        public ReportDocument prepare_report_print(int report_id, string param_value, string user_id)
        {
            DataTable dt = new DataTable();
            ReportDocument crystalReport = new ReportDocument();
            string rpt_file_url, report_param_list, auto_print_applicable, source_db;
            int no_of_report_parameter;
            string[] array_param_name;
            string[] array_param_value;
            try
            {
                dt = get_url_for_report(report_id);
                rpt_file_url = dt.Rows[0]["rpt_file_url"].ToString();
                report_param_list = dt.Rows[0]["report_param_list"].ToString();
                no_of_report_parameter = Convert.ToInt16(dt.Rows[0]["no_of_report_parameter"].ToString());

                auto_print_applicable = dt.Rows[0]["auto_print_applicable"].ToString();
                source_db = dt.Rows[0]["source_db"].ToString();

                crystalReport.Load(System.Web.Hosting.HostingEnvironment.MapPath(rpt_file_url));

                crystalReport.SetDatabaseLogon(objDataAccess._dbUid, objDataAccess._dbPwd, objDataAccess._dbServer, objDataAccess._CompanyDB);


                if (no_of_report_parameter > 0)
                {
                    array_param_name = new string[no_of_report_parameter];
                    array_param_value = new string[no_of_report_parameter];

                    array_param_name = report_param_list.Split('|');
                    array_param_value = param_value.Split('|');

                    for (int i = 0; i < no_of_report_parameter; i++)
                    {
                        crystalReport.SetParameterValue(i, array_param_value[i].ToString());
                    }
                }
                System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                printerSettings.PrinterName = "POS-58-Series";
                crystalReport.PrintToPrinter(printerSettings, new PageSettings(), false);
                return (crystalReport);
            }
            catch
            {
                throw;
            }
        }
        public DataTable get_all_item()
        {
            DataTable dt = new DataTable();
            string sql = "";

            sql = "EXEC dbo.SIL_get_all_item_sp ";
            try
            {
                objDataAccess = new DataAccessSqlServer();
                dt = objDataAccess.ExecuteDataTable(sql);
                return dt;
            }
            catch
            {
                throw;
            }
        }
        public ReportDocument prepare_report_document1(int report_id, string param_value)
        {
            DataTable dt = new DataTable();
            DataTable dt_new = new DataTable();
            ReportDocument crystalReport = new ReportDocument();
            string rpt_file_url, report_param_list;
            int no_of_report_parameter;
            string[] array_param_name;
            string[] array_param_value;
            try
            {
                dt = get_url_for_report(report_id);
                rpt_file_url = dt.Rows[0]["rpt_file_url"].ToString();
                report_param_list = dt.Rows[0]["report_param_list"].ToString();
                no_of_report_parameter = Convert.ToInt16(dt.Rows[0]["no_of_report_parameter"].ToString());

                crystalReport.Load(System.Web.Hosting.HostingEnvironment.MapPath(rpt_file_url));

                dt_new = get_all_item();
                crystalReport.SetDataSource(dt_new);
                return (crystalReport);
            }
            catch
            {
                throw;
            }
        }
        private DataTable get_url_for_report(int report_id)
        {
            DataTable dt = new DataTable();
            HanaCommand cmd = new HanaCommand();
            try
            {
                cmd.CommandText = "DTS_get_url_for_report_sp";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("P_report_id", report_id);
                dt = objDataAccess.ExecuteDataTable(cmd);
                return dt;
            }
            catch
            {
                throw;
            }
        }
        public DataSet get_url()
        {
            DataSet dt = new DataSet();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.CommandText = "SIL_get_url_for_report_sp";
                dt = objDataAccess.ExecuteDataSet(cmd);
                return dt;
            }
            catch
            {
                throw;
            }
        }

    }
}