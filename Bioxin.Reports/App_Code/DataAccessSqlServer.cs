using Sap.Data.Hana;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace CrystalReportCallingFromWeb.App_Code
{
    public class DataAccessSqlServer
    {
        private string str_con_normal;//= "Data Source=DESKTOP-R15BJEA;Initial Catalog=Local_POS;User ID=sa;Password=sql2017;trusted_Connection=false; Connection timeout=15;  Min Pool Size=0; Max Pool Size=1500;";
        private SqlConnection conn;
        public string _dbServer, _dbUid, _dbPwd, _CompanyDB,_Driver;
        public DataAccessSqlServer()
        {
            try
            {
                if (System.IO.File.Exists(HttpContext.Current.Server.MapPath("~\\App_Data\\DB_URL_Info.xml")))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(HttpContext.Current.Server.MapPath("~\\App_Data\\DB_URL_Info.xml"));
                    XmlNode xNode = xDoc.SelectSingleNode("//Servers/Server");
                    _dbServer = xNode.Attributes.GetNamedItem("dbServer").InnerText;
                    _dbUid = xNode.Attributes.GetNamedItem("dbUid").InnerText;
                    _dbPwd = xNode.Attributes.GetNamedItem("dbPwd").InnerText;
                    _CompanyDB = xNode.Attributes.GetNamedItem("CompanyDB").InnerText;
                    _Driver = xNode.Attributes.GetNamedItem("Driver").InnerText;
                    //str_con_normal = "DRIVER=" + _Driver + ";UID=" + _dbUid + ";PWD=" + _dbPwd + ";SERVERNODE=" + _dbServer ;
                    str_con_normal = "Server=" + _dbServer + ";UserID=" + _dbUid + ";Password=" + _dbPwd + ";Current Schema=" + _CompanyDB + ";";
                }
                else
                    throw new Exception("DB & Url File is not available");
            }
            catch
            {
                throw new Exception("DB & Url File is not available");
            }
        }

        public DataTable get_db_connection_details_for_report_print()
        {
            DataTable dt = new DataTable();
            DataRow dr;
            try
            {
                if (System.IO.File.Exists(HttpContext.Current.Server.MapPath("~\\App_Data\\DB_URL_Info.xml")))
                {
                    dt.Columns.Add(new DataColumn("db_server", typeof(string)));
                    dt.Columns.Add(new DataColumn("db_userid", typeof(string)));
                    dt.Columns.Add(new DataColumn("db_pass", typeof(string)));
                    dt.Columns.Add(new DataColumn("db_name", typeof(string)));
                    dr = dt.NewRow();

                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(HttpContext.Current.Server.MapPath("~\\App_Data\\DB_URL_Info.xml"));
                    XmlNode xNode = xDoc.SelectSingleNode("//Servers/Server");
                    dr["db_server"] = xNode.Attributes.GetNamedItem("dbServer").InnerText;
                    dr["db_userid"] = xNode.Attributes.GetNamedItem("dbUid").InnerText;
                    dr["db_pass"] = xNode.Attributes.GetNamedItem("dbPwd").InnerText;
                    dr["db_name"] = xNode.Attributes.GetNamedItem("CompanyDB").InnerText;
                    dt.Rows.Add(dr);
                    return dt;
                    //str_con_normal = "Data Source=" + _dbServer + ";Initial Catalog=" + _dbName + ";User ID=" + _dbUid + ";Password=" + _dbPwd + ";trusted_Connection=false; Connection timeout=15;  Min Pool Size=0; Max Pool Size=1500;";
                }
                else
                    throw new Exception("DB & Url File is not available");
            }
            catch
            {
                throw new Exception("DB & Url File is not available");
            }
        }
        public void convertFetchString(ref string str)
        {
            str = Regex.Replace(str, @"\=(\s{0,})\'", "=N'");
            str = Regex.Replace(str, @"\<>(\s{0,})\'", "<>N'");
            str = Regex.Replace(str, @"\((\s{0,})\'", "(N'");
            str = Regex.Replace(str, @"LIKE(\s{0,})\'", "LIKE N'");
        }
        public DataTable ExecuteDataTable(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                conn = new SqlConnection(str_con_normal);
                SqlDataAdapter adpt = new SqlDataAdapter(new SqlCommand(sql, conn));
                adpt.Fill(dt);
                return (dt);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
     
        public DataTable ExecuteDataTable(HanaCommand sqlCommand)
        {
            HanaConnection con = new HanaConnection(str_con_normal);
            sqlCommand.Connection = con;
            //convertFetchString(sqlCommand);
            HanaDataAdapter sqlAdapter = new HanaDataAdapter(sqlCommand);
            System.Data.DataTable sqlDataTable = new System.Data.DataTable();

            Exception Ex = null;
            try
            {
                sqlAdapter.Fill(sqlDataTable);
            }
            catch 
            {
                throw;
            }
            sqlCommand = null;
            return sqlDataTable;
        }
        /////////////////////////////////
        public DataSet ExecuteDataSet(SqlCommand sqlCommand)
        {
            try
            {
                DataSet ds = new DataSet();
                conn = new SqlConnection(str_con_normal);
                sqlCommand.Connection = conn;
                sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter adpt = new SqlDataAdapter(sqlCommand);
                //adpt.SelectCommand.CommandTimeout = 120;
                adpt.Fill(ds);
                return (ds);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
        public DataSet ExecuteDataSet(string sql)
        {
            try
            {
                DataSet ds = new DataSet();
                conn = new SqlConnection(str_con_normal);
                SqlDataAdapter adpt = new SqlDataAdapter(new SqlCommand(sql, conn));
                adpt.Fill(ds);
                return (ds);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}