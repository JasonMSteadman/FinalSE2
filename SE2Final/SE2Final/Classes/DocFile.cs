using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SE2Final.Classes
{
    public class DocFile
    {
        public int iLock { get; set; }
        public string sTitle { get; set; }
        public string sBody { get; set; }
        public string sGroupName { get; set; }
        public int iID { get; set; }

        public DocFile(int iID)
        {
            iLock = 0;
            sTitle = "";
            sBody = "";
            this.iID = iID;
            sGroupName = "";
        }

    }
}
