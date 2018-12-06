using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SE2Final.Classes;

namespace SE2Final
{

    // This class will hold all the SQL statements
    class SqlStatements
    {
        //UpdateDataBase("select * from FROM group_table;");

        private const string  conString = "Server=tcp:jsteadmana1.database.windows.net,1433;Initial " +
                    "Catalog=person99;Persist Security Info=False;User ID=Website;Password=password1!;" +
                    "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public SqlStatements() { }

        /// <summary>
        /// Updates documents title in db.
        /// </summary>
        /// <param name="iID">Document id.</param>
        /// <param name="sNewTitle">New title for the document.</param>
        public void UpdateTitle(int iID, string sNewTitle)  ////////////////////////
        {
            UpdateDataBase(
                "UPDATE groupDoc " +
                "SET docTitle = '" + sNewTitle + "' " +
                "WHERE docID = " + iID + ";"
                );
        }

        /// <summary>
        /// Gathers all the data for a single group.
        /// </summary>
        /// <param name="sGroupName">Group name.</param>
        /// <returns>Returns a list of DocFiles that are in the DB.</returns>
        public List<DocFile> QDB(string sGroupName)
        {
            return GetFromDataBase(
                "SELECT * " +
                "   FROM groupDoc " +
                "WHERE groupName = '" + sGroupName + "';");
        }

        /// <summary>
        /// Gathers all the names within the database
        /// </summary>
        /// <returns>Returns a list of group names.</returns>
        public List<string> QDBGroupNames()
        {
            return GetGNFromDataBase("SELECT groupName FROM groupName;");
        }

        /// <summary>
        /// Updates the state of the lock.
        /// </summary>
        /// <param name="iID">ID of the lock.</param>
        /// <param name="iNewState">The state you wish to place the lock in. e.g. 0 -> unlocked, 1 -> locked.</param>
        /// <returns>Whether your action was completed</returns>
        public bool UpdateLock(int iID, int iNewState)
        {
            if(iNewState == 1)
            {
               DocFile temp = GetFromDataBase("SELECT * FROM groupDoc WHERE docID = " + iID + ";")[0];
                if (temp.iLock != 0)
                    return false;
            }

            UpdateDataBase("UPDATE groupDoc " +
                            "    SET lock = " + iNewState + " " +
                            "WHERE docID = " + iID + ";");
            return true;
        }

        /// <summary>
        /// Deletes a document form the database
        /// </summary>
        /// <param name="iID">Document ID.</param>
        public void DeleteDoc(int iID)
        {
            UpdateDataBase("DELETE FROM groupDoc WHERE docID = " + iID + ";");
        }

        /// <summary>
        /// Adds a document to the DB.
        /// </summary>
        /// <param name="sGroupName">Name of the group the document will be placed.</param>
        /// <param name="sTitle">Title of the document</param>
        /// <returns>Returns a copy of the document.</returns>
        public DocFile CreateDoc(string sGroupName, string sTitle)
        {
            UpdateDataBase(
               "INSERT INTO groupDoc (groupName, docTitle, body, lock)" +
                    "VALUES('" + sGroupName + "', '" + sTitle + "', '', 0);");            

            return GetFromDataBase("SELECT TOP 1 * " +
                                   "     FROM groupDoc " +
                                   "ORDER BY docID DESC")[0];
        }

        /// <summary>
        /// Deletes a group from the DB.
        /// </summary>
        /// <param name="sGroupName">Name of the group you wish to delete.</param>
        public void DeleteGroup(string sGroupName)
        {
            //  Delete all group members
            UpdateDataBase("DELETE FROM groupDoc WHERE groupName = '" + sGroupName + "';");
            
            //  Delete group
            UpdateDataBase("DELETE FROM groupName WHERE groupName = '" + sGroupName + "';");
        }

        /// <summary>
        /// Create a group in the DB.
        /// </summary>
        /// <param name="sGroupName">The Name you would like to try to name your group.</param>
        /// <returns></returns>
        public bool CreateGroup(string sGroupName)
        {
            List<string> groupList = QDBGroupNames();

            //  Check for name
            if (groupList.Contains(sGroupName))
                return false;

            UpdateDataBase("INSERT INTO groupName VALUES('" + sGroupName + "');");
            return true;
        }

        /// <summary>
        /// Adds strings to the DB as they are typed.
        /// </summary>
        /// <param name="iID">Document ID.</param>
        /// <param name="sBody">String being placed into the DB.</param>
        public void AddToBody(int iID, string sBody)
        {
            string sNewBody = GetFromDataBase("SELECT * FROM groupDoc WHERE docID = " + iID + ";")[0].sBody + sBody;

            UpdateDataBase(
                            "UPDATE groupDoc " +
                            "SET body = '" + sNewBody + "' " +
                            "WHERE docID = " + iID + ";"
                            );
        }

        /// <summary>
        /// Helper method that controls all DB updates.
        /// </summary>
        /// <param name="sSQLStatement">Statement used to update the DB.</param>
        private void UpdateDataBase(string sSQLStatement)
        {
            //Add user input to database
            using (SqlConnection dbConn = new SqlConnection())
            {
                //Open connection
                dbConn.ConnectionString = conString;
                dbConn.Open();

                //Create SQL statement
                SqlCommand dbQ = new SqlCommand(sSQLStatement, dbConn);
                //Execute statement
                try
                {
                    dbQ.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Helper method that controls all DB queries.
        /// </summary>
        /// <param name="sSQLStatement">Statement used to gather data from the DB.</param>
        /// <returns></returns>
        private List<DocFile> GetFromDataBase(string sSQLStatement)
        {
            List<DocFile> docRet = new List<DocFile>();
            //Get user input
            using (SqlConnection dbConn = new SqlConnection())
            {
                //Open connection
                dbConn.ConnectionString = conString;


                dbConn.Open();

                SqlCommand getInput = new SqlCommand(sSQLStatement, dbConn);

                //Open reader
                try
                {
                    SqlDataReader dbReader = getInput.ExecuteReader();
                    dbReader.Read();
                    while (dbReader.HasRows)
                    {
                        DocFile temp = new DocFile(dbReader.GetInt32(0));

                        temp.sGroupName = dbReader.GetString(1);
                        temp.sTitle = dbReader.GetString(2);
                        temp.sBody = dbReader.GetString(3);
                        temp.iLock = dbReader.GetInt32(4);
                        docRet.Add(temp);

                        try
                        {
                            dbReader.Read();
                        }
                        catch (Exception e)
                        {
                            break;
                        }
                    }
                    //Close db connection
                    dbReader.Close();
                }
                catch(InvalidOperationException ioe)
                {
                    return docRet;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return docRet;
        }

        private List<string> GetGNFromDataBase(string sSQLStatement)
        {
            List<string> docRet = new List<string>();
            //Get user input
            using (SqlConnection dbConn = new SqlConnection())
            {
                //Open connection
                dbConn.ConnectionString = conString;

                dbConn.Open();

                SqlCommand getInput = new SqlCommand(sSQLStatement, dbConn);

                //Open reader
                try
                {
                    SqlDataReader dbReader = getInput.ExecuteReader();

                dbReader.Read();
                while (dbReader.HasRows)
                {
                    string sGroupName = dbReader.GetString(0);
                    docRet.Add(sGroupName);

                    try
                    {
                        dbReader.Read();
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }

                //Close db connection
                dbReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return docRet;
        }
    }
}