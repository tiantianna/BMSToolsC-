using bms.startup.Model;
using bms.startup.util;
using bms.startup.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace bms.startup
{
    public class LoginViewModel : INotifyPropertyChanged
    {

        private string username= "master";//用户名
        private string passwd= "master";//密码

        private SqLiteHelper sql;
        private bool isCreateNewUser = true;//是否需要创建新用户，true需要
        public DelegateCommand<Login> LoginCommand { get; set; }//登陆按钮
        public DelegateCommand<Login> LoginKeyDownCommand { get; set; }


        public string Username
        {
            get { return username; }
            set { username = value; OnPropertyChanged("Username"); }
        }

        public string Passwd
        {
            get { return passwd; }
            set { passwd = value; OnPropertyChanged("Passwd"); }
        }

        private bool isGen2 = true;

        public bool IsGen2
        {
            get { return isGen2; }
            set { isGen2 = value; OnPropertyChanged("IsGen2"); }
        }

        private bool isGen3 = false;

        public bool IsGen3
        {
            get { return isGen3; }
            set { isGen3 = value; OnPropertyChanged("IsGen3"); }
        }

        public bool IsUDS
        {
            get
            {
                return isUDS;
            }

            set
            {
                isUDS = value;
                OnPropertyChanged("IsUDS");
            }
        }

        public bool IsTooling
        {
            get
            {
                return isTooling;
            }

            set
            {
                isTooling = value;
                OnPropertyChanged("IsTooling");
            }
        }

        private bool isUDS = false;

        public LoginViewModel()
        {
            Init();
        }
        private bool isTooling;
        private void Init()
        {
            //ConsoleManager.Show();//打开信息打印窗口
            LoginCommand = new DelegateCommand<Login>(runLoginCommand);
            LoginKeyDownCommand = new DelegateCommand<Login>(runLoginKeyDownCommand);
            sql = SqLiteHelper.getInstance;
            sql.ConnectionString = "data source=mydb.db";
            sql.CreateTable("userTB", new string[] { "id", "username", "passwd", "userpower", "itemlist", "times" }, new string[] { "integer primary key AUTOINCREMENT", "varchar2(100) not null", "varchar2(100) not null", "int not null", "varchar2(100) not null", "int not null" });
            //创建名为table1的数据表
            DataTable dt = sql.ExecuteQuery("select count(*) from userTB");
            int count = Convert.ToInt32(dt.Rows[0][0].ToString());
            if (count == 0)
            {
                //是新表，需要创建管理员账户
                sql.InsertValues("userTB", new string[] { null, "admin", "123456", "2", "1,1,1,1", "0" });

            }
        }

        private void runLoginKeyDownCommand(Login obj)
        {
            obj.username.Focus();

            obj.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.F1)
                {
                    MessageBox.Show("haha");
                }

                if (e.Key == Key.Enter)
                {
                    obj.loginbtn.Focus();
                    runLoginCommand(obj);
                }



            };
        }

        private void runLoginCommand(Login window)
        {
            if (Passwd == null || Username == null)
            {
                return;
            }
            if (isGen2)
            {
                if (Passwd == "guest" && Username == "guest")
                {
                    //客户权限
                    //五个权限分别对应实时信息、配置页面（读取配置和更改配置其中一个可见即为可见）、读取配置、更改配置、追溯码
                    Gen2MainWindow m = new Gen2MainWindow(new string[] { "Visible", "Hidden", "Hidden", "Hidden", "Hidden" });
                    m.Show();
                    window.Hide();
                }
                else if (Passwd == "production" && Username == "production")
                {
                    //产线
                    Gen2MainWindow m = new Gen2MainWindow(new string[] { "Visible", "Visible", "Visible", "Hidden", "Visible" });
                    m.Show();
                    window.Hide();
                }
                else if (Passwd == "ruineng" && Username == "ruineng")
                {
                    //锐能
                    Gen2MainWindow m = new Gen2MainWindow(new string[] { "Visible", "Visible", "Visible", "Visible", "Hidden" });
                    m.Show();
                    window.Hide();
                }
                else if (Passwd == "aftermarket" && Username == "aftermarket")
                {
                    Gen2MainWindow m = new Gen2MainWindow(new string[] { "Visible", "Visible", "Visible", "Visible", "Visible" });
                    m.Show();
                    window.Hide();
                }
                else if (Passwd == "abc@123" && Username == "Gotion")
                {
                    Gen2MainWindow m = new Gen2MainWindow(new string[] { "Visible", "Visible", "Visible", "Visible", "Visible" });
                    m.Show();
                    window.Hide();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ModernDialog.ShowMessage("用户名或密码错误", "提示", MessageBoxButton.OK);

                    });
                    return;
                }
            }
            else if (isGen3)
            {
                if (Passwd == "master" && Username == "master")
                {
                    //超级用户
                    ////DataTable dt=sql.ExecuteQuery("select times from userTB where username='master'");
                    //登录次数
                    // int count = Convert.ToInt32(dt.Rows[0][0].ToString());
                    //  String pwd = PasswordBuilder.calPwdByTimes('master',count);

                    window.Hide();
                    if (isGen3)
                    {
                        MainWindow m = new MainWindow(new string[] { "Visible", "Visible", "Visible", "Visible", "Visible" }, 0);
                        m.Show();
                    }
                    //else if (isGen2) {
                    //    Gen2MainWindow m = new Gen2MainWindow();
                    //    m.Show();
                    //}
                    //AddUser m = new AddUser(0);
                    //m.Show();
                }
                else
                {
                    DataTable result = sql.ExecuteQuery("select * from userTB where username=@username and passwd=@passwd",
                        new Dictionary<string, string>() { { "@username", Username }, { "@passwd", Passwd } });
                    bool ifHasRows = (result.Rows.Count != 0);
                    User u = new User();
                    if (ifHasRows)
                    {
                        u.Username = result.Rows[0]["username"].ToString();
                        u.Passwd = result.Rows[0]["passwd"].ToString();
                        u.Itemlist = result.Rows[0]["itemlist"].ToString();
                        u.Times = Convert.ToInt32(result.Rows[0]["times"].ToString());
                        u.Userpower = Convert.ToInt32(result.Rows[0]["userpower"].ToString());
                    }
                    if (ifHasRows)
                    {
                        //登陆成功
                        if (u.Username == "admin")
                        {
                            //临时用户登陆
                            sql.ExecuteNonQuery("update userTB set times=(select times from userTB where username='admin')+1 where username='admin'");
                            //弹出建立用户界面
                            //window.Hide();
                            AddUser m = new AddUser(2);
                            m.Show();

                        }
                        else
                        {
                            //普通用户或管理员登陆

                            sql.ExecuteNonQuery("update userTB set times=(select times from userTB where username=@username)+1 where username=@username;",
                                new Dictionary<string, string> { { "@username", Username } });

                            if (u.Userpower == 1)
                            {
                                //管理员登陆       
                                string[] sArray = u.Itemlist.ToString().Split(',');
                                for (int i = 0; i < sArray.Length; i++)
                                {
                                    sArray[i] = sArray[i] == "0" ? "Hidden" : "Visible";
                                }
                                window.Hide();
                                MainWindow m = new MainWindow(sArray, 1);
                                m.Show();
                            }
                            else if (u.Userpower == 3)
                            {
                                //普通用户登陆
                                string[] sArray = u.Itemlist.ToString().Split(',');
                                for (int i = 0; i < sArray.Length; i++)
                                {
                                    sArray[i] = sArray[i] == "0" ? "Collapsed" : "Visible";
                                }
                                window.Hide();
                                MainWindow m = new MainWindow(sArray, 3);
                                m.Show();
                            }
                        }
                    }
                    else
                    {
                        //登陆失败
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ModernDialog.ShowMessage("用户名或密码错误", "提示", MessageBoxButton.OK);

                        });
                        return;
                    }

                }
            }
            else if (IsUDS)
            {
                if (Passwd == "master" && Username == "master")
                {
                    window.Hide();
                    UDSTestMainWindow m = new UDSTestMainWindow();
                    m.Show();
                }
            }
            else if (IsTooling) {
                if (Passwd == "abc@123" && Username == "GotionTooling")
                {
                    window.Hide();
                    ToolingMainWindow m = new ToolingMainWindow();
                    m.Show();
                }
            }
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
