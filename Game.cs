using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Övningstenta
{
    public class Game
    {
        public int GameId { get; set; }
        public Socket PlayerOne { get; set; }
        public Socket PlayerTwo { get; set; }
        public bool IsRunning => PlayerOne != null && PlayerTwo != null;
        public bool IsStarted { get; set; }
    }
}
