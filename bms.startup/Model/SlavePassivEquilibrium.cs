using bms.startup.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.userControl
{
    [Serializable]
   public class SlavePassivEquilibrium : INotifyPropertyChanged
    {
        private ObservableCollection<bool> slavePECheck = new ObservableCollection<bool>();

        public ObservableCollection<bool> SlavePECheck
        {
            get { return slavePECheck; }
            set { slavePECheck = value; OnPropertyChanged("SlavePECheck"); }
        }

        public SlavePassivEquilibrium() {
            for (int i = 0; i < 60; i++) {
                slavePECheck.Add(false);
            }
        }

        //拼发送包
        public CANSDK.VCI_CAN_OBJ getSlavePEPackage(int heartbeat)
        {
            CANSDK.VCI_CAN_OBJ obj = new CANSDK.VCI_CAN_OBJ();
            obj.Init();
            obj.RemoteFlag = 0;//远程帧
            obj.ExternFlag = 1;//扩展帧
            obj.SendType = 0;  //正常发送
            obj.DataLen = 8;
            obj.ID = uint.Parse("1C110141", System.Globalization.NumberStyles.HexNumber);
            byte[] data = new byte[8];
            data[0]=(byte)heartbeat;
            
            //byte[0]
            for (int i = 0; i < 4; i++) {
                data[0] = (byte)(data[0] | (SlavePECheck[i] == false ? 0 : 1) << (4 + i));
            }
            //byte[1]-byte[7]
            for (int i = 4; i < 60; i++) {
                data[(i + 4) / 8] = (byte)(data[(i + 4) / 8] | (SlavePECheck[i] == false ? 0 : 1) << (4 + i)%8);
            }

            obj.DataLen = 8;
            obj.Data = data;

            return new CANSDK.VCI_CAN_OBJ();
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
