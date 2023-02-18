﻿using Realms;

namespace McServersScanner.IO.DB
{

    /// <summary>
    /// Store info about minecraft server
    /// </summary>
    public class ServerInfo : RealmObject
    {
        [PrimaryKey]
        public int ID { get; set; }
     
        /// <summary>
        /// Target server ip
        /// </summary>
        public string IP { get; private set; } = string.Empty;
        
        /// <summary>
        /// Received answer
        /// </summary>
        public string JsonInfo { get; set; } = string.Empty;

        public ServerInfo() { }

        public ServerInfo(string jsonInfo, string ip) 
        {
            IP = ip;
            JsonInfo= jsonInfo;
        }
    }
}
