using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Övningstenta
{
    public class ClientSocket
    {
        MainWindow Form = Application.Current.Windows[0] as MainWindow;
        private readonly TcpClient _client = new TcpClient();
        private NetworkStream _stream;
        private readonly IPEndPoint _endPoint;
        private static CancellationTokenSource _tokenSource;
        public CancellationToken Ct;

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
                new Task(ClientRecieve).Start();
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

        public void ClientSend(string data)
        {
            var bytesToSend = Encoding.UTF8.GetBytes(data);
            _stream.Write(bytesToSend, 0, bytesToSend.Length);
            if (data == "LEAVE;")
                _tokenSource.Cancel();

        }

        public void ClientRecieve()
        {
            while (true)
            {
                if (Ct.IsCancellationRequested)
                    Ct.ThrowIfCancellationRequested();

                var data = new byte[1024];
                var recv = 0;

                try
                {
                    recv = _stream.Read(data, 0, data.Length);
                }
                catch (Exception)
                {
                    break;
                }

                if (recv == 0) break;

                var receivedData = Encoding.UTF8.GetString(data, 0, recv).Split(';');
                ParseCommand(receivedData[0], receivedData[1]);
            }
        }

        private void ParseCommand(string cmd, string data)
        {
            switch (cmd)
            {
                case "GETGAMES":
                    Form.Dispatcher.Invoke(() => { Form.DataContext = JsonConvert.DeserializeObject<List<int>>(data); }, DispatcherPriority.ContextIdle);
                    break;
                case "CREATE":
                    MainWindow.MultiplayerGameId = JsonConvert.DeserializeObject<int>(data);
                    CreateMultiplayerGame();
                    break;
                case "JOIN":
                    if (bool.Parse(data))
                        JoinMultiplayerGame();
                    break;
                case "OPPONENTJOIN":
                    Form.Dispatcher.Invoke(() =>
                    {
                        Form.GameStatusLabel.Content = "A player has joined your game.";
                        Form.StartGame.IsEnabled = true;
                    }, DispatcherPriority.ContextIdle);
                    break;
                case "START":
                    InitGame(JsonConvert.DeserializeObject<GameStatus>(data));
                    break;
                case "TURN":
                    UpdateGame(JsonConvert.DeserializeObject<GameStatus>(data));
                    break;
                case "GAMEOVER":
                    AnnounceWinner(data);
                    break;
                case "LEAVE":
                    break;
            }
        }

        private void InitGame(GameStatus gameStatus)
        {
            Form.Dispatcher.Invoke(() =>
            {
                MainWindow.GameStatus = gameStatus;
                Form.GameGrid.IsEnabled = GetPlayerInfo().PlayerSign == gameStatus.Turn;
                Form.GameStatusLabel.Content = "Game has started.";
                Form.StartGame.IsEnabled = false;
                Form.Client.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetPlayerInfo().PlayerColor));
                Form.Opponent.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(GetOpponentInfo().PlayerColor));
            }, DispatcherPriority.ContextIdle);
        }

        private void UpdateGame(GameStatus gameStatus)
        {
            Form.Dispatcher.Invoke(() =>
            {
                MainWindow.GameStatus = gameStatus;
                Form.GameGrid.IsEnabled = GetPlayerInfo().PlayerSign == gameStatus.Turn;
                UpdateGrid(MainWindow.GameStatus.Grid);
            }, DispatcherPriority.ContextIdle);
        }

        private void AnnounceWinner(string winner)
        {
            Form.Dispatcher.Invoke(() =>
            {
                Form.GameGrid.IsEnabled = false;
                //var result = MessageBox.Show($"Player {winner} won!!", "Tic tac toe", MessageBoxButton.OK); ;
                MessageBox.Show(winner != "" ? $"Player {winner} won!!" : "Noone was able to win this game",
                    "Tic tac toe", MessageBoxButton.OK);
                //switch (result)
                //{
                //    case MessageBoxResult.Yes:

                //        break;
                //    case MessageBoxResult.No:

                //        break;
                //}
            }, DispatcherPriority.ContextIdle);
        }

        public void UpdateGrid(string[,] grid)
        {
            for (int vertical = 0; vertical < grid.GetLength(0); vertical++)
            {
                for (int horizontal = 0; horizontal < grid.GetLength(1); horizontal++)
                {
                    if (grid[vertical, horizontal] != null)
                    {
                        var button = (Button)Form.FindName(ButtonDictList[$"{vertical},{horizontal}"]);
                        if (button == null) return;
                        button.Content = grid[vertical, horizontal];
                        button.Foreground = grid[vertical, horizontal] == GetPlayerInfo().PlayerSign ? Form.Client.Fill : Form.Opponent.Fill;
                        button.FontSize = 50;
                    }
                }
            }
        }

        public PlayerInfo GetPlayerInfo()
        {
            return MainWindow.IsPlayerOne ? MainWindow.GameStatus.PlayerOne : MainWindow.GameStatus.PlayerTwo;
        }
        public PlayerInfo GetOpponentInfo()
        {
            return MainWindow.IsPlayerOne ? MainWindow.GameStatus.PlayerTwo : MainWindow.GameStatus.PlayerOne;
        }

        public void CreateMultiplayerGame()
        {
            if (MainWindow.MultiplayerGameId != 0)
            {
                Form.Dispatcher.Invoke(() =>
                {
                    Form.GameMenuContainer.Visibility = Visibility.Hidden;
                    Form.GameGrid.Visibility = Visibility.Visible;
                    Form.MultiplayerGameInfoContainer.Visibility = Visibility.Visible;
                    MainWindow.IsPlayerOne = true;
                }, DispatcherPriority.ContextIdle);
            }
        }

        public void JoinMultiplayerGame()
        {
            Form.Dispatcher.Invoke(() =>
            {
                Form.GameGrid.Visibility = Visibility.Visible;
                Form.GameMenuContainer.Visibility = Visibility.Hidden;
                Form.MultiplayerGameInfoContainer.Visibility = Visibility.Visible;
                MainWindow.IsPlayerOne = false;
                Form.GameStatusLabel.Content = "You are now connected to a game.";
            }, DispatcherPriority.ContextIdle);
        }

        public static Dictionary<string, string> ButtonDictList = new Dictionary<string, string>
        {
            {"0,0", "ZeroZero"},
            {"0,1", "ZeroOne"},
            {"0,2", "ZeroTwo"},
            {"1,0", "OneZero"},
            {"1,1", "OneOne"},
            {"1,2", "OneTwo"},
            {"2,0", "TwoZero"},
            {"2,1", "TwoOne"},
            {"2,2", "TwoTwo"},
        };
    }
}
