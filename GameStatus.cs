using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Övningstenta
{
    public class GameStatus
    {
        public int GameId { get; set; }
        public string[,] Grid { get; set; }
        public string Turn { get; set; }
        public PlayerInfo PlayerOne { get; set; }
        public PlayerInfo PlayerTwo { get; set; }
        public bool IsStarted { get; set; }
    }
}
