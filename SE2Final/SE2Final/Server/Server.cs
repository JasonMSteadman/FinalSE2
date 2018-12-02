using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SE2Final.Classes;
using System.Collections.Generic;
using System;

namespace SE2Final
{
    public class Server : Hub
    {
        private static Dictionary<string, Dictionary<int, DocFile>>     group;
        private static Dictionary<int, DocFile>                         content;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public Server()
        {
            if (content == null) 
            {
                content = new Dictionary<int, DocFile>();
                group = new Dictionary<string, Dictionary<int, DocFile>>();

         
                //  Load content from DB

                //  For local testing
                group.Add("group one", content);
                content.Add(0, new DocFile(0));
                content[0].sBody = "testing";
                content[0].sTitle = "Test";
                content[0].iLock = 0;

                content.Add(1, new DocFile(1));
                content[1].sBody = "testing";
                content[1].sTitle = "Test";
                content[1].iLock = 0;

                content.Add(2, new DocFile(2));
                content[2].sBody = "testing";
                content[2].sTitle = "Test";
                content[2].iLock = 0;

                group.Add("group two", new Dictionary<int, DocFile>());
                group["group two"].Add(0, new DocFile(0));
            }
        }

        /// <summary>
        /// Loads page one doc at a time.
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task FirstContact(string groupName)
        {
            string sTitle;
            string sBody;
            int iLock;
            int iID;

            foreach(KeyValuePair<int, DocFile> c in group[groupName])
            {
                iID = c.Value.iID;
                sTitle = c.Value.sTitle;
                sBody = c.Value.sBody;
                iLock = c.Value.iLock;
                await Clients.Caller.SendAsync("LoadPage", iID, sTitle, sBody, iLock, groupName);
            }
        }

        /// <summary>
        /// Populates group box.
        /// </summary>
        /// <returns></returns>
        public async Task LoadGroupBox()
        {
            foreach (KeyValuePair<string, Dictionary<int, DocFile>> c in group)
            {
                await Clients.Caller.SendAsync("PopGroup", c.Key, false);
            }
        }

        /// <summary>
        /// Edits document title.
        /// </summary>
        /// <param name="id">Documents id, this is provided by the html.</param>
        /// <param name="title">New title.</param>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task NewTitle(int id, string title, string groupName)
        {
            if (group[groupName].ContainsKey(id))
            {
                //  TODO //////////////// Update DB ////////////////////////////
                group[groupName][id].sTitle = title;
            }
            else
                return;

            await Clients.All.SendAsync("updateTitle", id, title, groupName);
        }

        /// <summary>
        /// Deletes doc.
        /// </summary>
        /// <param name="id">Documents id, this is provided by the html.</param>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task DeleteDoc(int id, string groupName)
        {
            if (group[groupName].ContainsKey(id))
            {
                //  TODO //////////////// Update DB ////////////////////////////
                group[groupName].Remove(id);
            }
            else
                return;

            await Clients.All.SendAsync("deletingDoc", id, groupName);
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <param name="title">Document title.</param>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task CreateDoc(string title, string groupName)
        {
            int iCount = -1;
            while(true)
            {
                if (!group[groupName].ContainsKey(++iCount))
                    break;
            }
            DocFile d = new DocFile(iCount);
            
            d.sTitle = title;
            d.sBody = "";
            d.iLock = 0;
            //  Add to local
            group[groupName].Add(d.iID, d);

            //  TODO Update DB  //////////////////////////////////////////
            await Clients.All.SendAsync("LoadPage", d.iID, d.sTitle, d.sBody, d.iLock, groupName);
        }

        /// <summary>
        /// Deletes group from the select tag with the id groupbox.
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task DeleteGroup(string groupName)
        {
            if(group.ContainsKey(groupName))
            {
                //  TODO    Update DB   //////////////////////////////////////
                group.Remove(groupName);

                await Clients.All.SendAsync("removeGroup", groupName);
            }
        }
        /// <summary>
        /// Updates the body of a doc.
        /// </summary>
        /// <param name="id">Documents id, this is provided by the html.</param>
        /// <param name="body">New character(s) to add to the body of the selected document.</param>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task BodyTyping(int id, string body, string groupName)
        {
            //  TODO Updated DB  /////////////////////////////
            group[groupName][id].sBody = body;

            await Clients.Others.SendAsync("UpdateBody", "bdy" + id, body, groupName);
        }

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task CreateGroup(string groupName)
        {
            if(group.ContainsKey(groupName))
            {   ///////////////////////////////////////Needs to be finded/////////////
                //  Return to an error
                string message = "A group with the name " + groupName + " already exists.";
                await Clients.Caller.SendAsync("error", message);
                return;
            }

            //  TODO Updated DB  /////////////////////////////

            //  Create new local group
            group.Add(groupName, new Dictionary<int, DocFile>());
            await Clients.Caller.SendAsync("PopGroup", groupName, true);
            await Clients.Others.SendAsync("PopGroup", groupName, false);
        }

        /// <summary>
        /// Checks and updates locks
        /// </summary>
        /// <param name="lockID">An id used to identify the div being locked.  This is provided by the html.</param>
        /// <param name="lckState">0 -> free, 1 -> locked, 2 -> current user owns the lock.</param>
        /// <param name="groupName">Name of the group the doc belows to</param>
        /// <returns></returns>
        public async Task docLock(string lockID, string lckState, string groupName)
        {
            int iID = 0;
            if (lckState.Equals("Locked"))
                return;

            if (!Int32.TryParse(lockID, out iID))
                return;

            if (lckState.Equals("Done"))
            {
                //  Update DB   ////////////////////////////////
                group[groupName][iID].iLock = 0;
                await Clients.All.SendAsync("LockUpdate", iID, 0, groupName);
                return;
            }

            if (content.ContainsKey(iID))
                if (group[groupName][iID].iLock == 0)
                {
                    //  Update DB   /////////////////////////////////
                    group[groupName][iID].iLock = 1;
                    await Clients.Caller.SendAsync("LockUpdate", iID, 2, groupName);
                } 

            await Clients.Others.SendAsync("LockUpdate", iID, 1, groupName);
        }
    }
}