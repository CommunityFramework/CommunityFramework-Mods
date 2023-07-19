using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CF_Firewall
{
    public class AsnRange
    {
        public int rangeStart;
        public int rangeEnd;
        public AsnRange(int rangeStart, int RangeEnd = -1)
        {
            this.rangeStart = rangeStart;
            if (RangeEnd == -1)
                this.rangeEnd = rangeStart;
            else this.rangeEnd = RangeEnd;
        }
    }
}
