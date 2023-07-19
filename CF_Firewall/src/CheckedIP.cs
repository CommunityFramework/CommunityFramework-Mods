using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CF_Firewall
{
    public class CheckedIP
    {
        public DateTime last { get; set; }
        public bool ipBan { get; set; } = false;
        public IPHubResponse data { get; set; } = null;
        public CheckedIP(IPHubResponse response)
        {
            this.data = response;
            this.last = DateTime.Now;
        }
        public CheckedIP()
        {
            this.ipBan = true;
            this.last = DateTime.Now;

        }
    }
}