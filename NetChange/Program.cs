using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetChange
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 检查当前用户是否具有管理员权限
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            // 如果当前用户不具有管理员权限，则使用管理员身份重新启动该应用程序
            if (!isAdmin)
            {
                // 以管理员身份重新启动该应用程序
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Application.ExecutablePath;
                psi.Verb = "runas";
                try
                {
                    Process.Start(psi);
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // 取消 UAC 弹出窗口时会抛出异常，可以不处理
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MenuForm());
        }
    }
}
