using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Övningstenta
{
    public class ClientSocket
    {
        private readonly TcpClient _client = new TcpClient();
        private NetworkStream _stream;
        private readonly IPEndPoint _endPoint;
        private Task _activeGame;
        private static CancellationTokenSource _tokenSource;
        public CancellationToken _ct;

        public ClientSocket(string address, string port, out bool success)
        {
            _endPoint = RemoteEndPoint(address, port);
            success = Connect();
        }

        private bool Connect()
        {
            try
            {
                _client.Connect(_endPoint);
                _stream = _client.GetStream();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IPEndPoint RemoteEndPoint(string address, string port)
        {
            return new IPEndPoint(IPAddress.Parse(address), Int32.Parse(port));
        }

        public void ClientSend(Command cmd)
        {
            var bytesToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(cmd));
            _stream.Write(bytesToSend, 0, bytesToSend.Length);
            _stream.Flush();
            if (cmd.CommandTerm == "LEAVE")
                _tokenSource.Cancel();
                
        }

        public void ClientRecieve()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested)
                    _ct.ThrowIfCancellationRequested();

                var data = new byte[1024];
                var recv = _stream.Read(data, 0, data.Length);
                var temp = Encoding.ASCII.GetString(data, 0, recv);
                if (ValidateJson(temp))
                {
                    
                }
            }
        }

        public List<int> ClientRecieveGames()
        {
            var data = new byte[1024];
            var recv = _stream.Read(data, 0, data.Length);
            var games = Encoding.ASCII.GetString(data, 0, recv);
            return !ValidateJson(games) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(games);
        }

        public int ClientRecieveGameId()
        {
            var data = new byte[1024];
            var recv = _stream.Read(data, 0, data.Length);
            var game = Encoding.ASCII.GetString(data, 0, recv);
            var gameId = !ValidateJson(game) ? 0 : JsonConvert.DeserializeObject<int>(game);
            if (gameId != 0)
            {
                CreateTask();
                _activeGame.Start();
            }

            return gameId;
        }

        public bool JoinGameSucceeded()
        {
            var data = new byte[1024];
            var recv = _stream.Read(data, 0, data.Length);
            var game = Encoding.ASCII.GetString(data, 0, recv);
            var succeeded = ValidateJson(game) && JsonConvert.DeserializeObject<bool>(game);
            if (succeeded)
            {
                CreateTask();
                _activeGame.Start();
            }

            return succeeded;
        }

        private void CreateTask()
        {
            _tokenSource = new CancellationTokenSource();
            _ct = _tokenSource.Token;

            _activeGame = new Task(() =>
            {
                _ct.ThrowIfCancellationRequested();
                ClientRecieve();
            }, _tokenSource.Token);

        }

        private static bool ValidateJson(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(json);
                return true;
            }
            catch // not valid
            {
                return false;
            }
        }
    }
}
