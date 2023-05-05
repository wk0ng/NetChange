using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetChange
{
    public partial class MenuForm : UIForm
    {
        SettingForm SettingForm = null;
        ManagementObjectSearcher lastResult = null;

        public MenuForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            // 加载配置文件
            Setting.Current.Load();

            // 不在任务栏显示窗体
            this.ShowInTaskbar = false;
            // 禁用最大化窗口、最小化窗口
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // 启动后仅显示托盘图标
            this.WindowState = FormWindowState.Minimized;

            #region 控件事件绑定
            // mainNotifyIcon 事件绑定
            this.mainNotifyIcon.Click += MainNotifyIcon_Click;
            // 窗体尺寸发生变化
            this.Resize += MenuForm_Resize;
            // 点击关闭按钮
            this.FormClosing += MenuForm_FormClosing;
            #endregion
        }

        #region 事件
        // 窗体加载事件
        private void MenuForm_Load(object sender, EventArgs e)
        {
            // 刷新网络适配器状态
            ReloadNetworkState();
        }
        // 窗体失去焦点事件
        private void MenuForm_LostFocus(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        // 窗体尺寸发生变化
        private void MenuForm_Resize(object sender, EventArgs e)
        {
            // 会卡顿，取消
            //ReloadNetworkState();
            //改为延时处理
            this.timer1.Enabled = true;
        }
        // 窗体关闭事件
        private void MenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 判断关闭事件reason来源于窗体按钮，否则用菜单退出时无法退出
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 取消关闭窗体
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                return;
            }
        }
        // 时间控件触发
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ReloadNetworkState();
        }
        // 托盘图标点击事件
        private void MainNotifyIcon_Click(object sender, EventArgs e)
        {
            // 窗体处于最小化时，显示窗口
            if(this.WindowState == FormWindowState.Minimized)
            {
                // 获取当前鼠标点击坐标
                int x = Cursor.Position.X;
                int y = Cursor.Position.Y;

                // 根据托盘图标的大小调整窗体位置
                x -= 312;// this.Width;
                y -= 456;// this.Height;
                y = y - 10;

                // 设置当前窗口坐标
                this.Location = new Point(x, y);
                // 将窗体至于顶层
                this.TopMost = true;
                // 恢复窗体正常显示状态
                this.WindowState = FormWindowState.Normal;
            }
        }
        // 设置按钮点击事件
        private void SettingBtn_Click(object sender, EventArgs e)
        {
            if(this.SettingForm != null)
            {
                this.SettingForm.Close();
                this.SettingForm = null;
            }

            this.SettingForm = new SettingForm(this);
            this.SettingForm.Show();
        }
        // 退出按钮点击事件
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region 方法
        // 根据配置文件刷新当前网络设配器清单
        // 可通过外部访问
        public void ReloadNetworkState()
        {
            // 最小化状态不刷新
            if(this.WindowState != FormWindowState.Minimized) 
            {
                // 通过WMI获取网络适配器列表
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
                if(searcher != this.lastResult)
                {
                    this.lastResult = searcher;
                    // 先清空当前列表
                    this.splitContainer1.Panel1.Controls.Clear();

                    // 获取配置文件显示清单
                    String[] showList = Setting.Current.ShowNetworkName.Split('|');

                    int num = 1;

                    foreach (ManagementObject adapter in searcher.Get())
                    {
                        if (showList.Contains((string)adapter["Name"]))
                        {
                            Boolean active = false;
                            if (adapter["NetEnabled"] != null && adapter["NetConnectionStatus"].ToString() != "0")
                            {
                                active = true;
                            }
                            addNet(num, (string)adapter["Name"], active);
                            num++;
                        }
                    }
                }
                
            }
        }
        // 向panel1添加网络适配器
        private void addNet(int indexNum, String name, Boolean active = false)
        {
            int top = 0 + 29 * (indexNum - 1) + (15 * indexNum - 1);

            UILabel labelName = new UILabel();
            labelName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            labelName.Location = new System.Drawing.Point(3, top);
            labelName.Name = "uiLabel" + name;
            labelName.Size = new System.Drawing.Size(220, 23);
            labelName.TabIndex = 1;
            labelName.Text = name;
            labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            UISwitch sw = new UISwitch();
            sw.Name = name;
            sw.Enabled = true;
            sw.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            sw.Location = new System.Drawing.Point(235, top);
            sw.MinimumSize = new System.Drawing.Size(1, 1);
            sw.Size = new System.Drawing.Size(75, 29);
            sw.TabIndex = indexNum - 1;
            sw.Text = "uiSwitch1";
            sw.Tag = name;
            sw.Active = active;
            sw.ValueChanged += Sw_ValueChanged;

            this.splitContainer1.Panel1.Controls.Add(labelName);
            this.splitContainer1.Panel1.Controls.Add(sw);
        }
        private void Sw_ValueChanged(object sender, bool value)
        {
            UISwitch obj = (UISwitch)sender;
            String netName = obj.Tag.ToString();

            ChangeNetworkEnableByName(netName, value);
        }
        // 启用或禁用网络适配器
        private void ChangeNetworkEnableByName(String name, Boolean value)
        {
            // 获取网络适配器
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_NetworkAdapter WHERE Name='" + name + "'");  // 假设适配器名为eth0
            ManagementObjectCollection adapters = searcher.Get();

            foreach (ManagementObject adapter in adapters)
            {
                // 判断适配器状态
                if (value)
                {
                    // 启用适配器
                    adapter.InvokeMethod("Enable", null);
                }
                else
                {
                    // 禁用适配器
                    adapter.InvokeMethod("Disable", null);
                }
            }
        }
        #endregion
    }
}
