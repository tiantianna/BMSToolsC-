using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using bms.startup.SDK;
using slaveUpperComputer.util;

namespace bms.startup.Model
{
    [Serializable]
    public class SlaveBalanceModel : INotifyPropertyChanged
    {
        //private bool[] isBalance = new bool[60];

        //public bool[] IsBalance
        //{
        //    get { return isBalance; }
        //    set { isBalance = value; }
        //}


        private ObservableCollection<bool> isBalance = new ObservableCollection<bool>();

        public ObservableCollection<bool> IsBalance
        {
            get { return isBalance; }
            set { isBalance = value; OnPropertyChanged("IsBalance"); }
        }

        public SlaveBalanceModel()
        {
            for (int i = 0; i < 60; i++)
            {
                isBalance.Add(false);
            }
        }

        public void setAllTrue() {
            for (int i = 0; i < 60; i++)
            {
                isBalance[i]=true;
            }
        }

        public void setAllFalse() {
            for (int i = 0; i < 60; i++)
            {
                isBalance[i]=false;
            }
        }

        public CANSDK.VCI_CAN_OBJ getBalanceCANSDK1(int bmuNum)
        {
            byte[] data = new byte[8];
            data[0] = 0x4C;
            data[1] = (byte)bmuNum;
            
            for (int i = 0; i < 40; i++) {
                data[i / 8 + 2] = (byte)(data[i / 8 + 2] | (IsBalance[i] == true ? 1 : 0) << (i % 8));
            }
            data[7] = DataConverter.getAllBytesSum(data);

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            obj.DataLen = 8;
            obj.Data = data;

            return obj;
        }

        public CANSDK.VCI_CAN_OBJ getBalanceCANSDK2(int bmuNum)
        {
            byte[] data = new byte[8];
            data[0] = 0x4D;
            data[1] = (byte)bmuNum;

            for (int i = 40; i < 60; i++) {
                data[i/8-3]=(byte)(data[i/8-3]|(IsBalance[i]==true?1:0)<<(i%8));
            }
            data[7] = DataConverter.getAllBytesSum(data);

            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//数据帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;//正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("18F203F0", System.Globalization.NumberStyles.HexNumber);
            obj.DataLen = 8;
            obj.Data = data;

            return obj;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
