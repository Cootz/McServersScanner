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

    /// <summary>
    /// Store info about minecraft server
    /// </summary>
    //[Index(nameof(ServerInfo.IP), IsUnique = true)]
    public class ServerInfo
    {
        public int ID { get; set; }
        public string IP { get; private set; } = string.Empty;
        public string JsonInfo { get; set; } = string.Empty;

        public ServerInfo() { }

        public ServerInfo(string jsonInfo, string ip) 
        {
            IP = ip;
            JsonInfo= jsonInfo;
        }
    }
}
