using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    public class CellModel
    {
        private float cellvol;
        private int celltatus;

        public int Cellstatus
        {
            get { return celltatus; }
            set { celltatus = value; }
        }

        public float Cellvol
        {
            get { return cellvol; }
            set { cellvol = value; }
        }

    }
}
