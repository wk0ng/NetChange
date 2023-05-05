using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetChange
{
    [ConfigFile("Config.ini")]
    public class Setting : IniConfig<Setting>
    {
        [ConfigSection("System")]
        public Boolean AutoRun { get; set; }
        [ConfigSection("NetWork")]
        public String ShowNetworkName { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            AutoRun = false;
            ShowNetworkName = "";
        }
    }
}
