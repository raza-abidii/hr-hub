using Microsoft.Data.SqlClient;
using System.Data;
using System.Xml;

namespace EMSSolution.DatabaseAccessLayer
{
    public class DataLayer
    {
        SqlConnection conn = new SqlConnection();
        public DataSet GetData(string strQry, ref string errMessage, string database= "master")
        {
            DataSet ds = new DataSet();
            try
            {
                WriteLog("DataAccess", "Query: " + strQry);

                SqlDataAdapter adapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                if (getConnection(database) == false)
                {
                    return ds;
                }

                #region Code to get generic database from Microsoft.practice.enterprice.library
                //GenericDatabase genericDatabase = 
                //    new GenericDatabase(conn.ConnectionString, DbProviderFactories.GetFactory("SqlServer"));

                #endregion

                command.CommandType = CommandType.Text;
                command.Connection = conn;
                command.CommandText = strQry;
                conn.Open();
                adapter.SelectCommand = command;

                adapter.Fill(ds);
                conn.Close();
                return ds;
            }
            catch (Exception e)
            {
                WriteLog("DataAccess", "Exception in GetData: " + e.Message);
                return null;
            }
        }

        public bool getConnection(string dBase)
        {
            try
            {
                if(conn.State==ConnectionState.Open)
                    conn.Close();
                string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory)
                    + "\\DBFile\\DatabaseConfig.xml"; // Path to your XML file

                WriteLog("DataAccess", "DatabaseConfig File Path: " + xmlFilePath);

                if (!File.Exists(xmlFilePath))
                {
                    WriteLog("DataAccess", "DatabaseConfig File does not exist in given Path: " + xmlFilePath);
                    return false;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                // Reading values from the XML
                var serverNode = xmlDoc.SelectSingleNode("/DatabaseConfig/Database/Data_Source");
                var databaseNode = xmlDoc.SelectSingleNode("/DatabaseConfig/Database/Initial_Catalog");
                var userNode = xmlDoc.SelectSingleNode("/DatabaseConfig/Database/User_Id");
                var passwordNode = xmlDoc.SelectSingleNode("/DatabaseConfig/Database/Password");

                if (serverNode == null || databaseNode == null || userNode == null || passwordNode == null)
                    return (false);
                string server = serverNode.InnerText;

                //string database = databaseNode.InnerText;
                string database = dBase;
                string user = userNode.InnerText;
                string password = passwordNode.InnerText;

                string strConnection = string.Empty;
                if (user == "" || password == "")
                {

                    strConnection = $@"Server={server};Database={database};Integrated Security=True;TrustServerCertificate=True";
                }
                else
                {
                    strConnection = $@"Server={server};Database={database};User Id={user};Password={password};
                        Integrated Security=False;TrustServerCertificate=True";
                }

                //WriteLog("DataAccess", "Connection String: " + strConnection );
                if (strConnection != null)
                {
                    conn.ConnectionString = strConnection;
                    conn.Open();
                    if (conn.State != ConnectionState.Open)
                        return false;
                    else
                    {
                        conn.Close();
                        return true;
                    }
                }

                return true;
            }
            catch (Exception e1)
            {
                WriteLog("DataAccess", "Exception in getConnection: " + e1.Message);
                return (false);
            }

        }
        public bool GetExecute(string strQry, ref string errMessage,string database = "master")
        {
            try
            {
                errMessage = string.Empty;
                if (getConnection(database) == false)
                    return false;

                SqlCommand command = new SqlCommand();
                command.CommandText = strQry;
                command.CommandType = CommandType.Text;
                conn.Open();
                command.Connection = conn;
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                WriteLog("DataAccess", "Exception in GetExecute: " + e.Message);
                errMessage = e.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public void WriteLog(string LogFileName, string Content)
        {
            string filePath = $@"C:\Windows\Temp\{LogFileName}{DateTime.Now.Day.ToString("00")}{DateTime.Now.Month.ToString("00")}.txt";

            // Use StreamWriter to write to the file
            try
            {
                // Create or overwrite the file
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine(Content);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
