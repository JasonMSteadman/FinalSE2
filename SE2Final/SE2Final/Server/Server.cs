using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SE2Final.Classes;
using System.Collections.Generic;
using System;

namespace SE2Final
{
    public class Server : Hub
    {
        //private static Dictionary<string, Dictionary<int, DocFile>> group;
        //private static Dictionary<int, DocFile> content;
        private SqlStatements db = new SqlStatements();


        /// <summary>
        /// Standard constructor.
        /// </summary>
        public Server() { }
 
        /// <summary>
        /// Loads page one doc at a time.
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task FirstContact(string groupName)
        {
            //  Get groups from DB
            List<DocFile> group = db.QDB(groupName);

            //  Load page one doc at a time
            foreach(DocFile d in group)
            {
                await Clients.Caller.SendAsync("LoadPage", d.iID, d.sTitle, d.sBody, d.iLock, groupName);
            }   
        }

        /// <summary>
        /// Populates group box.
        /// </summary>
        /// <returns></returns>
        public async Task LoadGroupBox()
        {
            List<string> group = db.QDBGroupNames();

            foreach(string s in group)
            {
                await Clients.Caller.SendAsync("PopGroup", s, false);
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
            //  Update DB
            db.UpdateTitle(id, title);
            //  Propagate changes
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
            db.DeleteDoc(id);

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
            DocFile d = db.CreateDoc(groupName, title);

            await Clients.All.SendAsync("LoadPage", d.iID, d.sTitle, d.sBody, d.iLock, groupName);
        }

        /// <summary>
        /// Deletes group from the select tag with the id groupbox.
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task DeleteGroup(string groupName) 
        {
            db.DeleteGroup(groupName);
            await Clients.All.SendAsync("removeGroup", groupName);
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
            db.AddToBody(id, body);
            //group[groupName][id].sBody = body;

            await Clients.All.SendAsync("UpdateBody", "bdy" + id, body, groupName);
        }

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="groupName">Name of the group the doc belows to.</param>
        /// <returns></returns>
        public async Task CreateGroup(string groupName)
        {
            if(!db.CreateGroup(groupName))
            {   
                //  Return to an error
                string message = "A group with the name " + groupName + " already exists.";
                await Clients.Caller.SendAsync("error", message);
                return;
            }

            //  Create new local group
            //group.Add(groupName, new Dictionary<int, DocFile>());
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
                db.UpdateLock(iID, 0);
                //group[groupName][iID].iLock = 0;
                await Clients.All.SendAsync("LockUpdate", iID, 0, groupName);
                return;
            }

            if(!db.UpdateLock(iID, 1))
            {
                return;
            }
            await Clients.Caller.SendAsync("LockUpdate", iID, 2, groupName);
            await Clients.Others.SendAsync("LockUpdate", iID, 1, groupName);
        }
    }
}