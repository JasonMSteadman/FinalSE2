using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SE2Final.Classes
{
    public class DocFile
    {
        private int iLock { get; set; }
        private string sTitle { get; set; }
        private string sBody { get; set; }
        private string sGroupName { get; set; }
        private int iID { get; set; }

        public DocFile()
        {
            iLock = 0;
            sTitle = "";
            sBody = "";
        }

    }
}
