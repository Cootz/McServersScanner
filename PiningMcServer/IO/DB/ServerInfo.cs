using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.IO.DB
{
    //[Index(nameof(ServerInfo.IP), IsUnique = true)]
    public class ServerInfo
    {
        public int ID { get; set; }
        public string IP { get; private set; }
        public string JsonInfo { get; set; }

        public ServerInfo() { }

        public ServerInfo(string jsonInfo, string ip) 
        {
            IP = ip;
            JsonInfo= jsonInfo;
        }
    }
}
