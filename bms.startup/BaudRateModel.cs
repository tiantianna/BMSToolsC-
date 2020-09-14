using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup
{
    public class BaudRateModel
    {
        private string baudratename;

        public string Baudratename
        {
            get { return baudratename; }
            set { baudratename = value; }
        }


        private byte time0;

        public byte Time0
        {
            get { return time0; }
            set { time0 = value; }
        }

        private byte time1;

        public byte Time1
        {
            get { return time1; }
            set { time1 = value; }
        }
    }
}
