using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    class ToolingStep : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string id;
        private string upperLimit;//上限
        private string lowerLimit;//下限
        private bool isCheck;//是否选中
        private string result = "                                   ";//结果
        private string sendID = "00000000";//发送ID
        private string receID = "00000000";//接收ID
        private string description;//功能描述

        //private byte[] frame;//发送的帧数据
        //private int type;//步骤类型

       

        public bool IsCheck
        {
            get
            {
                return isCheck;
            }

            set
            {
                isCheck = value;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public string Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Result"));
                }
            }
        }

        public string SendID
        {
            get
            {
                return sendID;
            }

            set
            {
                sendID = value;
            }
        }

        public string ReceID
        {
            get
            {
                return receID;
            }

            set
            {
                receID = value;
            }
        }

        public string UpperLimit
        {
            get
            {
                return upperLimit;
            }

            set
            {
                upperLimit = value;
            }
        }

        public string LowerLimit
        {
            get
            {
                return lowerLimit;
            }

            set
            {
                lowerLimit = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        //public byte[] Frame
        //{
        //    get
        //    {
        //        return frame;
        //    }

        //    set
        //    {
        //        frame = value;
        //    }
        //}

        //public int Type
        //{
        //    get
        //    {
        //        return type;
        //    }

        //    set
        //    {
        //        type = value;
        //    }
        //}
    }
}
