using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace Seo.Crawler.Selenium
{
    public class ExcelHandler
    {
        public static DataTable LoadDataTable(string filePath, string sql, string tableName)
        {
            OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0 Xml;HDR=YES'");
            OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dt.TableName = tableName;
            conn.Close();
            return dt;
        }

        public static void DataTableToExcel(string filePath, DataTable dtDataTable)
        {
            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(dtDataTable, "Result" + DateTime.Now.ToString("YYYYmmDDHHMMss"));
            wb.SaveAs(filePath);
        }


        public static DataTable InitTable(DataTable dtDataTable)
        {
            if (dtDataTable != null)
            {
                dtDataTable.Columns.Add("SourceURL", typeof(System.String));
                dtDataTable.Columns.Add("URL", typeof(System.String));
                dtDataTable.Columns.Add("NotFound", typeof(System.String));
                dtDataTable.Columns.Add("Error", typeof(System.String));
                dtDataTable.Columns.Add("LogCount", typeof(System.String));
            }
            return dtDataTable;
        }

    }
}
