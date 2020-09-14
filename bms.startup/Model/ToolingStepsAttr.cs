using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    class ToolingStepsAttr
    {
        private string id;
        private int type;
        //private byte frame;//发送的帧数据
        private string frame;
        private double resolution;//分辨率
        private double offset;//偏移量
        private string resolutionS;//完整的分辨率
        private string offsetS;//完整的分辨率
        private string description;//功能描述

        //同类型的上下限不是固定值，这两个值不是从ToolingAttr.xml文件中读取，而是在上位机发送的时候赋予的值
        private double upperLimit;//上限
        private double lowerLimit;//下限
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

        public int Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

       
        public string Frame
        {
            get
            {
                return frame;
            }

            set
            {
                frame = value;
            }
        }

        public double UpperLimit
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

        public double LowerLimit
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

       

        public string ResolutionS
        {
            get
            {
                return resolutionS;
            }

            set
            {
                resolutionS = value;
            }
        }

        public string OffsetS
        {
            get
            {
                return offsetS;
            }

            set
            {
                offsetS = value;
            }
        }

        public double Resolution
        {
            get
            {
                return resolution;
            }

            set
            {
                resolution = value;
            }
        }

        public double Offset
        {
            get
            {
                return offset;
            }

            set
            {
                offset = value;
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
    }
}
