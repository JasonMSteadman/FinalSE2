using System;
using System.Data.SqlClient;
using System.Text;
using System.Data;
namespace sqlStatements
{

    // This class will hold all the SQL statements
    class SqlStatements
    {
        private const String connectionString = "Server=tcp:se2final.database.windows.net,1433;Initial Catalog=SE2Final;Persist Security Info=False;User ID=se2final;Password=@@abc123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private string sSQL;
        public void UpdateBody(string sGroupname, string sTitle, string sNew)
        {
            //  Create sql statement
            IntoDB(sSQL);
        }

        public void UpdateTitle(string sGroupname, string sTitle, string sNewTitle)
        {
            //  update title
            //  Create sql statement

            IntoDB(sSQL);
        }

        public int Insert(string sGroupName, string sTitle)
        {
            int iID = 0;
            //  Add to db
            //  Create sql statement

            IntoDB(sSQL);
            //  Que for the ID of the new row 


            return iID;
        }

        public void Delete(string sSQLStatment)
        {
            //  Delete
        }

        public DataSet CreateDataSet(string sGroupName)
        {
            //  Return a data set based on the group name
            return null;
        }

        private void IntoDB(string sSQLStatment)
        {

        }

        private DataSet OutOfDB(string SQLStatement)
        {

            return null;
        }
    }
}