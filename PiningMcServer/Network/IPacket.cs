using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Network
{
    public interface IPacket
    {
        public abstract byte[] GetData();
    }
}
