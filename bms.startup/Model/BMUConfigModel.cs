using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace bms.startup.Model
{
    public class BMUConfigModel
    {
        private int bmuindex;
        private int cellcouts;
        private int tempcounts;
        private float vol;

        private string versionA1;
        private string versionA2;
        private string versionB1;
        private string versionB2;
        private string versionC1;
        private string versionC2;
        private string monitor1;
        private string monitor2;
        private string boota1;
        private string boota2;
        public string VersionA1
        {
            get { return versionA1; }
            set { versionA1 = value; }
        }

        public string VersionA2
        {
            get { return versionA2; }
            set { versionA2 = value; }
        }

        public string VersionB1
        {
            get { return versionB1; }
            set { versionB1 = value; }
        }

        public string VersionB2
        {
            get { return versionB2; }
            set { versionB2 = value; }
        }

        public string VersionC1
        {
            get { return versionC1; }
            set { versionC1 = value; }
        }

        public string VersionC2
        {
            get { return versionC2; }
            set { versionC2 = value; }
        }

        public string Monitor1
        {
            get { return monitor1; }
            set { monitor1 = value; }
        }

        public string Monitor2
        {
            get { return monitor2; }
            set { monitor2 = value; }
        }

        public string Boota1
        {
            get { return boota1; }
            set { boota1 = value; }
        }

        public string Boota2
        {
            get { return boota2; }
            set { boota2 = value; }
        }

        private bool isinitdt;

        public bool Isinitdt
        {
            get { return isinitdt; }
            set { isinitdt = value; }
        }

        private Dictionary<int, CellModel> cellDic = new Dictionary<int, CellModel>();

        public Dictionary<int, CellModel> CellDic
        {
            get { return cellDic; }
            set { cellDic = value; }
        }

        private String version;

        public String Version
        {
            get
            {
                int i = 1;
                version = null;
                version += (VersionA1 != null && VersionA2 != null) ? (Application.Current.FindResource("modulea") + (VersionA1.Insert(3, ".").Insert(7, "_") + VersionA2) + (((i++ % 2) == 0) ? "\r\n" : "\t")) : null;
                version += (VersionB1 != null && VersionB2 != null) ? (Application.Current.FindResource("moduleb") + (VersionB1.Insert(3, ".").Insert(7, "_") + VersionB2) + (((i++ % 2) == 0) ? "\r\n" : "\t")) : null;
                version += (VersionC1 != null && VersionC2 != null) ? (Application.Current.FindResource("modulec") + (VersionC1.Insert(3, ".").Insert(7, "_") + VersionC2) + (((i++ % 2) == 0) ? "\r\n" : "\t")) : null;
                version += (Monitor1 != null && Monitor2 != null) ? (Application.Current.FindResource("monitor") + (Monitor1.Insert(3, ".").Insert(7, "_") + Monitor2) + (((i++ % 2) == 0) ? "\r\n" : "\t")) : null;
                version += (Boota1 != null && Boota2 != null) ? (Application.Current.FindResource("boota") + (Boota1 + Boota2.Substring(0, 4)) + (((i++ % 2) == 0) ? "\r\n" : "\t")) : null;
                return version;
            }
            set
            {
                version = value;
            }
        }

        /// <summary>
        /// 温度值
        /// </summary>
        private int temp;
        private int cellmodelAcounts;
        private int cellmodelBcounts;
        private int cellmodelCcounts;
        private int cellmodelDcounts;
        private int cellmodelEcounts;
        private bool isReceive;

        public bool IsReceive
        {
            get { return isReceive; }
            set { isReceive = value; }
        }
        public int Bmuindex
        {
            get { return bmuindex; }
            set { bmuindex = value; }
        }

        public int Cellcouts
        {
            get { return cellcouts; }
            set { cellcouts = value; }
        }

        public int Tempcounts
        {
            get { return tempcounts; }
            set { tempcounts = value; }
        }

        public float Vol
        {
            get { return vol; }
            set { vol = value; }
        }

        public int Temp
        {
            get { return temp; }
            set { temp = value; }
        }

        public int CellmodelAcounts
        {
            get { return cellmodelAcounts; }
            set { cellmodelAcounts = value; }
        }

        public int CellmodelBcounts
        {
            get { return cellmodelBcounts; }
            set { cellmodelBcounts = value; }
        }

        public int CellmodelCcounts
        {
            get { return cellmodelCcounts; }
            set { cellmodelCcounts = value; }
        }

        public int CellmodelDcounts
        {
            get
            {
                return cellmodelDcounts;
            }

            set
            {
                cellmodelDcounts = value;
            }
        }

        public int CellmodelEcounts
        {
            get
            {
                return cellmodelEcounts;
            }

            set
            {
                cellmodelEcounts = value;
            }
        }
    }
}
