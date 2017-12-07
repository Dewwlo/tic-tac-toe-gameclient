using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Övningstenta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientSocket _clientSocket;
        public static int MultiplayerGameId;
        private string _gameMode = "";
        public static GameStatus GameStatus;
        public static bool IsPlayerOne;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GridButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            if (_clientSocket.GetPlayerInfo().PlayerSign != GameStatus.Turn) return;
            var result = UpdateScoreGrid(button.Name, _clientSocket.GetPlayerInfo().PlayerSign);
            if (!result) return;

            _clientSocket.UpdateGrid(GameStatus.Grid);
            GameStatus.Turn = _clientSocket.GetOpponentInfo().PlayerSign;
            _clientSocket.ClientSend($"TURN;{JsonConvert.SerializeObject(GameStatus)}");
        }
        
        private void NewGame_OnClick(object sender, RoutedEventArgs e)
        {
            ResetGame();
            GameGrid.IsEnabled = true;
        }

        private void CloseGame_OnClick(object sender, RoutedEventArgs e)
        {
            ResetGame();
            GameGrid.IsEnabled = false;
        }

        private void ResetGame()
        {
            GameStatus = null;
            ZeroZero.Content = null;
            ZeroOne.Content = null;
            ZeroTwo.Content = null;
            OneZero.Content = null;
            OneOne.Content = null;
            OneTwo.Content = null;
            TwoZero.Content = null;
            TwoOne.Content = null;
            TwoTwo.Content = null;
        }

        public bool UpdateScoreGrid(string buttonName, string playerSign)
        {
            var result = false;

            var index = GridIndexDictList[buttonName].Split(',');

            if (GameStatus.Grid[int.Parse(index[0]), int.Parse(index[1])] == null)
            {
                GameStatus.Grid[int.Parse(index[0]), int.Parse(index[1])] = playerSign;
                result = true;
            }
        
            return result;
        }

        public static Dictionary<string, string> GridIndexDictList = new Dictionary<string, string>
        {
            {"ZeroZero", "0,0"},
            {"ZeroOne", "0,1"},
            {"ZeroTwo", "0,2"},
            {"OneZero", "1,0"},
            {"OneOne", "1,1"},
            {"OneTwo", "1,2"},
            {"TwoZero", "2,0"},
            {"TwoOne", "2,1"},
            {"TwoTwo", "2,2"},
        };

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            //_clientSocket = new ClientSocket(InputIpAddress.Text, InputPortNumber.Text, out _contentLoaded);
            _clientSocket = new ClientSocket("192.168.1.14", "8081", out _contentLoaded);

            ConnectionText.Visibility = Visibility.Visible;
            ConnectionText.Text = _contentLoaded ? "You are now connected to server" : "Connection to server failed";
            if (_contentLoaded)
            {
                
                GameMenuContainer.Visibility = Visibility.Visible;
                ConnectContainer.Visibility = Visibility.Hidden;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _clientSocket.ClientSend("GETGAMES;");
        }

        private void CreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            _clientSocket.ClientSend("CREATE;");
        }

        private void JoinGame_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GameListBox.SelectedItem;
            if (selectedItem != null)
            {
                _clientSocket.ClientSend($"JOIN;{GameListBox.SelectedItem}");
            }
        }

        private void GameMode_Click(object sender, RoutedEventArgs e)
        {
            var content = (sender as Button)?.Content.ToString();
            if (content == "Singleplayer")
            {
                GameGrid.Visibility = Visibility.Visible;
                GameGrid.IsEnabled = true;
                GameModeContainer.Visibility = Visibility.Hidden;
                _gameMode = "Singleplayer";
            }
            else
            {
                ConnectContainer.Visibility = Visibility.Visible;
                GameModeContainer.Visibility = Visibility.Hidden;
                _gameMode = "Multiplayer";
            }
        }

        private void LeaveGame_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContainer.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Hidden;
            MultiplayerGameInfoContainer.Visibility = Visibility.Hidden;
            ResetGame();
            _clientSocket.ClientSend($"LEAVE;{MultiplayerGameId}");
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            _clientSocket.ClientSend($"START;{MultiplayerGameId}");
        }
    }
}
