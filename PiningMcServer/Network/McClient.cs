using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Network
{
    public class McCLient : IDisposable
    {
        private string ip { get; set; }
        private ushort port { get; set; }

       
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
