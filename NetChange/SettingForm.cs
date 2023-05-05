using Microsoft.Win32;
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
    public partial class SettingForm : UIForm
    {
        MenuForm MenuForm = null;
        Boolean loadEnd = false;    // 防止添加控件时触发Change事件

        public SettingForm(MenuForm MainForm)
        {
            InitializeComponent();
            Setting.Current.Load();

            this.MenuForm = MainForm;
            // 加载配置文件
            Setting.Current.Load();

            // 不在任务栏显示窗体
            this.ShowInTaskbar = false;
            // 禁用最大化窗口、最小化窗口
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // 窗体屏幕居中
            this.StartPosition = FormStartPosition.CenterScreen;

            // 绑定事件
            this.FormClosing += SettingForm_FormClosing;
            this.uiSymbolLabel1.MouseHover += UiSymbolLabel1_MouseHover;
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            // 加载网络适配器信息
            ReloadNetList();
            // 初始化开机自启状态
            this.AutoRunSwitch.Active = false;
            if (Setting.Current.AutoRun == true)
            {
                this.AutoRunSwitch.Active = true;
            }
            this.loadEnd = true;
        }
        // 关闭事件，刷新主窗体状态
        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.MenuForm.ReloadNetworkState();
        }

        #region 方法
        private void ReloadNetList()
        {
            this.tabPage1.Controls.Clear();
            int networkNum = 1;
            String[] showList = Setting.Current.ShowNetworkName.Split('|');
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");

            foreach (ManagementObject adapter in searcher.Get())
            {
                Boolean active = false;
                if (showList.Contains((string)adapter["Name"]))
                {
                    active = true;
                }
                addNetInfo(networkNum, (string)adapter["Name"], active);
                networkNum++;
            }

            // panel占位
            Panel panel1 = new Panel();
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 380);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(599, 15);
            panel1.TabIndex = 0;
            this.tabPage1.Controls.Add(panel1);
        }

        private void addNetInfo(int indexNum, String name, Boolean Active = false)
        {
            int top = 10 + 29 * (indexNum - 1) + (15 * indexNum - 1);

            UILabel labelName = new UILabel();
            labelName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            labelName.Location = new System.Drawing.Point(20, top);
            labelName.Name = "uiLabel" + name;
            labelName.Size = new System.Drawing.Size(460, 23);
            labelName.TabIndex = 1;
            labelName.Text = name;
            labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            UISwitch sw = new UISwitch();
            sw.Name = name;
            sw.Enabled = true;
            sw.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            sw.Location = new System.Drawing.Point(500, top);
            sw.MinimumSize = new System.Drawing.Size(1, 1);
            sw.Size = new System.Drawing.Size(75, 29);
            sw.TabIndex = indexNum - 1;
            sw.Tag = name;
            sw.ActiveText = "显示";
            sw.InActiveText = "隐藏";
            sw.ValueChanged += Sw_ValueChanged;
            sw.Active = Active;

            this.tabPage1.Controls.Add(labelName);
            this.tabPage1.Controls.Add(sw);
        }

        private void Sw_ValueChanged(object sender, bool value)
        {
            if (this.loadEnd)
            {
                String[] showList = Setting.Current.ShowNetworkName.Split('|');
                UISwitch obj = (UISwitch)sender;
                String netName = obj.Tag.ToString();

                if (value)
                {
                    List<string> tmp = showList.ToList();
                    tmp.Add(netName);
                    showList = tmp.ToArray();
                }
                else
                {
                    showList = showList.Where(x => x != netName).ToArray();
                }

                Setting.Current.ShowNetworkName = string.Join("|", showList);
                Setting.Current.Save();
            }
        }
        #endregion

        private void AutoRunSwitch_ValueChanged(object sender, bool value)
        {
            if (value)
            {
                // 获取当前程序的路径
                string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

                // 创建注册表项
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.SetValue("NetChange", $"\"{exePath}\"", RegistryValueKind.String);

                // 设置启动项以管理员身份运行
                RegistryKey key2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key2.SetValue("NetChange", $"\"{exePath}\"", RegistryValueKind.String);
                key2.SetValue("NetChange", $"\"{exePath}\" --run-as-admin", RegistryValueKind.String);
            }
            else
            {
                // 删除启动项
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key.DeleteValue("NetChange");
            }
            if (this.loadEnd)
            {
                Setting.Current.AutoRun = value;
                Setting.Current.Save();
            }
        }

        private void UiSymbolLabel1_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(this.uiSymbolLabel1, "目前开机自启动会覆盖掉配置，还没找到原因。。");
        }
    }
}
