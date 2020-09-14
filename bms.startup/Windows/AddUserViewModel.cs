using bms.startup.Model;
using bms.startup.userControl;
using bms.startup.util;
using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace bms.startup.Windows
{
    class AddUserViewModel : INotifyPropertyChanged
    {
        private int power;//当前登陆用户的权限
        private SqLiteHelper sql;
        private string addUsername;//新增的用户名        
        private string addPasswd;//新增的密码
        private int selectValue;
        private ObservableCollection<string> addPowerList;//权限列表
        private int addPowerSelect = 0;//权限列表选择索引   
        private ObservableCollection<User> userList;

        public DelegateCommand ClosedCommand { get; set; }//关闭addUser窗口
        public DelegateCommand AddUserCommand { get; set; }//添加用户按钮事件
        public DelegateCommand<DataGrid> DataGridSelectedCommand { get; set; }//DataGrid行选中事件
        public DelegateCommand DataGridMouseLeftUp { get; set; }//DataGrid点击事件

        public DelegateCommand<DataGrid> Del { get; set; }

        public int Power
        {
            get { return power; }
            set { power = value; }
        }

        public ObservableCollection<string> AddPowerList
        {
            get
            {
                return addPowerList;
            }
            set
            {
                addPowerList = value;
            }
        }

        public int AddPowerSelect
        {
            get { return addPowerSelect; }
            set { addPowerSelect = value; OnPropertyChanged("AddPowerSelect"); }
        }

        public string AddUsername
        {
            get { return addUsername; }
            set { addUsername = value; OnPropertyChanged("AddUsername"); }
        }
     
        public string AddPasswd
        {
            get { return addPasswd; }
            set { addPasswd = value; OnPropertyChanged("AddPasswd"); }
        }
    
        public int SelectValue
        {
            get { return selectValue; }
            set { selectValue = value; OnPropertyChanged("SelectValue"); }
        }

        public ObservableCollection<User> UserList
        {
            get { return userList; }
            set { userList = value; OnPropertyChanged("UserList"); }
        }

        public AddUserViewModel(int power)
        {
            Power = power;
            Init();
        }

        private void Init()
        {
            userList = new ObservableCollection<User>();
            AddUserCommand = new DelegateCommand(runAddUser);
            ClosedCommand = new DelegateCommand(runClosedCommand);
            DataGridSelectedCommand = new DelegateCommand<DataGrid>(runDataGridSelectedCommand);
            Del = new DelegateCommand<DataGrid>(runDel);

            sql = SqLiteHelper.getInstance;
            sql.ConnectionString = "data source=mydb.db";
            AddPowerList = new ObservableCollection<string>();
            showUsers();
        }

        private void runClosedCommand()
        {
            int a = 0;
            DataTable dt3=sql.ExecuteQuery("select count(1) from userTB where userpower=3");
            int i = Convert.ToInt32(dt3.Rows[0][0].ToString());
            if (i > 0) { 
            //已经创建了普通用户，临时用户可以销毁
               int rownum= sql.ExecuteNonQuery("delete from userTB where username='admin' and passwd='123456'");
               Console.WriteLine(rownum);
            }
        }

        public void runDel(DataGrid datagrid) {
            User user= datagrid.SelectedItem as User;
            if (user == null) { return; }
            sql.ExecuteNonQuery("delete from userTB where username=@username", new Dictionary<string, string> { { "@username", user.Username } });
            showUsers();
        }

        private void runDataGridSelectedCommand(DataGrid datagrid)
        {
            //User user = datagrid.SelectedItem as User;
            //if (user != null) {
            //    Popup p = new Popup();
            //    DelOrUpdate d = new DelOrUpdate();
            //    WrapPanel wp = datagrid.Parent as WrapPanel;

            //    p.Child = d;
            //    wp.Children.Insert(0, p);
            //    //wp.Children.Add(p); 
            //    p.HorizontalOffset = 200;
            //    Point point = Mouse.GetPosition(wp);
            //    Console.WriteLine(point.X + "," + point.Y);
            //    //this.LayoutRoot.Children.Add(p);

            //    //打开显示
            //    p.IsOpen = true;
            //}
        }

       

        //根据权限,展示用户信息
        private void showUsers()
        {
            
                switch (power)
                {
                    case 0:
                        //超级用户
                        AddPowerList.Clear();
                        AddPowerList.Add("普通用户");
                        AddPowerList.Add("管理员");                        
                        //显示除临时用户以外的所有用户
                        DataTable dt = sql.ExecuteQuery("select username,userpower from userTB where userpower<>2");
                        UserList.Clear();
                        for (int i = 0; i < dt.Rows.Count; i++) {
                            User u = new User();
                            u.Username = dt.Rows[i]["username"].ToString();
                            u.Passwd = "XXX";
                            u.Userpower = Convert.ToInt32(dt.Rows[i]["userpower"].ToString());
                            UserList.Add(u);
                        }
                        break;
                    case 2:
                        //临时用户
                        AddPowerList.Clear();
                        AddPowerList.Add("普通用户");                        
                        DataTable dt2 = sql.ExecuteQuery("select (username) from userTB where userpower=3");
                        UserList.Clear();
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            User u = new User();
                            u.Username = dt2.Rows[i]["username"].ToString();
                            u.Passwd = "XXX";
                            u.Userpower =3;
                            UserList.Add(u);
                        }
                        break;
                }
                AddPowerSelect = 0;
        }

        //添加用户按钮事件
        public void runAddUser()
        {
            if (AddUsername == null | AddPasswd == null) {
                return;
            }
            DataTable dt3 = sql.ExecuteQuery("select count(1) from userTB where username='" + AddUsername+"'");
            //result.Read();
            Console.WriteLine(dt3.Rows.Count);
            int i = Convert.ToInt32(dt3.Rows[0][0].ToString());
            //sql.CloseConnection();
            if (i != 0)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ModernDialog.ShowMessage("用户名已经存在", "提示", MessageBoxButton.OK);

                });
                return;
            }

            if (AddUsername.Equals("admin") | AddUsername.Equals("master"))
            {
                //与临时用户或超级用户重名
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ModernDialog.ShowMessage("用户名非法,请更换", "提示", MessageBoxButton.OK);
                });
                return;
            }

            int power = -1;
            string itemlist = null;
            switch (addPowerList[AddPowerSelect])
            {
                case "超级用户":
                    power = 0;
                    itemlist = "1,1,1,1";
                    break;
                case "管理员":
                    power = 1;
                    itemlist = "1,1,1,1";
                    break;
                case "临时用户":
                    power = 2;
                    itemlist = "0,0,0,0";
                    break;
                case "普通用户":
                    power = 3;
                    itemlist = "0,0,1,0";
                    break;
            }
            int insertCount=sql.ExecuteNonQuery("insert into userTB values(null,@username,@passwd,@userpower,@itemlist,0)",
                    new Dictionary<string, string>() { { "@username", AddUsername }, { "@passwd", AddPasswd }, { "@userpower", power.ToString() }, { "@itemlist", itemlist } });
            List<string> aa = AddPowerList.ToList();
            if (insertCount != 1)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    ModernDialog.ShowMessage("添加失败", "提示", MessageBoxButton.OK);
                });
            }
            showUsers();       
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
