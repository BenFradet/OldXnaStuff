using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Platformer.Libz
{
    class ExcelImport
    {
        public static DataSet GetExcelData(string filePath)
        {
            DataSet ds = new DataSet();
            string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";
            string sheet = "";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();
                    DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    foreach (DataRow schemaRow in schemaTable.Rows)
                    {
                        sheet = schemaRow["TABLE_NAME"].ToString();
                    
                        OleDbCommand cmd = new OleDbCommand("select * from [" + sheet + "]", conn);
                        cmd.CommandType = CommandType.Text;

                        DataTable dt = new DataTable(sheet);
                        ds.Tables.Add(dt);
                        new OleDbDataAdapter(cmd).Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + string.Format("Sheet: {0}.File: {1}", sheet, filePath), ex);
            }
            return ds;
        }

        public static Dictionary<string, List<int>> GetSpreadSheetStructure(DataTable table)
        {
            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();

            //group: B1 appendix (4, 1), Data protection + Secure printing (5, 2), Information classification + Swap (5, 3), ID + Incident reporting + Clean desk + Branding (5, 4)
            foreach (DataRow row in table.Rows)
            {
                if (row["Level"].ToString() == "1")
                {
                    if (!dict.Keys.Contains("Level1"))
                        dict.Add("Level1", new List<int>());
                    dict["Level1"].Add(table.Rows.IndexOf(row));
                }
                else if (row["Level"].ToString() == "2")
                {
                    if (!dict.Keys.Contains("Level2"))
                        dict.Add("Level2", new List<int>());
                    dict["Level2"].Add(table.Rows.IndexOf(row));
                }
                else if (row["Level"].ToString() == "3")
                {
                    if (!dict.Keys.Contains("Level3"))
                        dict.Add("Level3", new List<int>());
                    dict["Level3"].Add(table.Rows.IndexOf(row));
                }
                else if (row["Level"].ToString() == "4")
                {
                    if (!dict.Keys.Contains("Level4"))
                        dict.Add("Level4", new List<int>());
                    dict["Level4"].Add(table.Rows.IndexOf(row));
                }
                else if (row["Level"].ToString() == "5")
                {
                    if (!dict.Keys.Contains("Level5"))
                        dict.Add("Level5", new List<int>());
                    dict["Level5"].Add(table.Rows.IndexOf(row));
                }
            }

            return dict;
        }
    }
}
