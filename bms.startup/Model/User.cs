using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace bms.startup.Model
{
    class User : INotifyPropertyChanged
    {
        private List<string> powerList = new List<string> { "超级用户", "管理员", "临时用户", "普通用户" };
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        //  private string username;
        public string Username { get; set; }
        //public string Username
        //{
        //    get { return username; }
        //    set { username = value; OnPropertyChanged("Username"); }
        //}
        private string passwd;

        public string Passwd
        {
            get { return passwd; }
            set { passwd = value; }
        }
        private int userpower;

        public int Userpower
        {
            get { return userpower; }
            set { userpower = value; }
        }
        private string itemlist;

        private string powerName;//Userpower对应的权限名称

        public string PowerName
        {
            get { return powerList[Userpower]; }
            set { powerName = value; OnPropertyChanged("PowerName"); }
        }

        public string Itemlist
        {
            get { return itemlist; }
            set { itemlist = value; }
        }
        private int times;

        public int Times
        {
            get { return times; }
            set { times = value; }
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
