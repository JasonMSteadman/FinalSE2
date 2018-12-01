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
        private static int iCount; //Temp ///////////////////////////

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

                iCount = 2;
            }
        }

        //  Loads page one doc at a time
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
                await Clients.Caller.SendAsync("LoadPage", iID, sTitle, sBody, iLock);
            }
        }

        //  TODO remove////////////////////////////
        public async Task SendMessage()
        {
            //await Clients.All.SendAsync("LoadPage");
        }

        public async Task CreateDoc(string title)
        {
            DocFile d = new DocFile(iCount++);
            
            d.sTitle = title;
            d.sBody = "";
            d.iLock = 0;
            //  Add to local
            content.Add(d.iID, d);

            //  TODO Update DB  //////////////////////////////////////////
            await Clients.All.SendAsync("LoadPage", d.iID, d.sTitle, d.sBody, d.iLock);
        }

        //  Updates the body of a doc
        public async Task BodyTyping(int id, string body)
        {
            //  TODO Updated DB  /////////////////////////////
            content[id].sBody = body;

            await Clients.Others.SendAsync("UpdateBody", "bdy" + id, body);
        }

        //  Checks and updates locks
        public async Task docLock(string lockID, string lckState)
        {
            int iID = 0;
            if (lckState.Equals("Locked"))
                return;

            if (lckState.Equals("Done"))
            {
                //  Update DB   ////////////////////////////////
                content[iID].iLock = 0;
                await Clients.All.SendAsync("LockUpdate", "lck" + lockID, "bdy" + iID, 0);
                return;
            }

            if (!Int32.TryParse(lockID, out iID))
                return;

            if (content.ContainsKey(iID))
                if (content[iID].iLock == 0)
                {
                    //  Update DB   /////////////////////////////////
                    content[iID].iLock = 1;
                    await Clients.Caller.SendAsync("LockUpdate", "lck" + lockID, "bdy" + iID, 2);
                } 

            await Clients.Others.SendAsync("LockUpdate", "lck" + lockID, "bdy" + iID, 1);
        }
    }
}