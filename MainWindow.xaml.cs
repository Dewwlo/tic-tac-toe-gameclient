using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Övningstenta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientSocket _clientSocket;
        private List<int> _games = new List<int>();
        private int _multiplayerGameId;
        private string _gameMode = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private int Counter { get; set; }

        private void GridButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;

            if (button == null || Counter >= 9) return;
            button.Content = Counter % 2 != 0 ? "X" : "O";
            button.FontSize = 50;
            button.Foreground = Brushes.Red;
            Counter += 1;

            if (CalculateVicotory())
            {
                var result = MessageBox.Show($"Player {button.Content} won!!", "Tic tac toe", MessageBoxButton.YesNo); ;
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ResetGame();
                        GameGrid.IsEnabled = true;
                        break;
                    case MessageBoxResult.No:
                        ResetGame();
                        GameGrid.IsEnabled = false;
                        break;
                }
            }
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
            ZeroZero.Content = null;
            ZeroOne.Content = null;
            ZeroTwo.Content = null;
            OneZero.Content = null;
            OneOne.Content = null;
            OneTwo.Content = null;
            TwoZero.Content = null;
            TwoOne.Content = null;
            TwoTwo.Content = null;

            Counter = 0;
        }

        private bool CalculateVicotory()
        {
            if (Equals(ZeroZero.Content, ZeroOne.Content) && Equals(ZeroZero.Content, ZeroTwo.Content) && ZeroZero.Content != null)
                return true;
            if (Equals(ZeroZero.Content, OneOne.Content) && Equals(ZeroZero.Content, TwoTwo.Content) && ZeroZero.Content != null)
                return true;
            if (Equals(ZeroZero.Content, ZeroOne.Content) && Equals(ZeroZero.Content, ZeroTwo.Content) && ZeroZero.Content != null)
                return true;
            if (Equals(OneZero.Content, OneOne.Content) && Equals(OneZero.Content, OneTwo.Content) && OneZero.Content != null)
                return true;
            if (Equals(TwoZero.Content, TwoOne.Content) && Equals(TwoZero.Content, TwoTwo.Content) && TwoZero.Content != null)
                return true;
            if (Equals(ZeroOne.Content, OneOne.Content) && Equals(ZeroOne.Content, TwoOne.Content) && ZeroOne.Content != null)
                return true;
            if (Equals(ZeroTwo.Content, OneTwo.Content) && Equals(ZeroTwo.Content, TwoTwo.Content) && ZeroTwo.Content != null)
                return true;
            if (Equals(ZeroTwo.Content, OneOne.Content) && Equals(ZeroTwo.Content, TwoZero.Content) && ZeroTwo.Content != null)
                return true;
            return false;
        }

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
            _clientSocket.ClientSend(new Command { CommandTerm = "GETGAMES", Data = null });
            DataContext = _clientSocket.ClientRecieveGames();
        }

        private void CreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            _clientSocket.ClientSend(new Command { CommandTerm = "CREATE", Data = null });
            _multiplayerGameId = _clientSocket.ClientRecieveGameId();

            if (_multiplayerGameId != 0)
            {
                GameMenuContainer.Visibility = Visibility.Hidden;
                GameGrid.Visibility = Visibility.Visible;
                MultiplayerGameInfoContainer.Visibility = Visibility.Visible;
                Client.Fill = Brushes.Red;
                Opponent.Fill = Brushes.Blue;
            }
        }

        private void JoinGame_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GameListBox.SelectedItem;
            if (selectedItem != null)
            {
                _clientSocket.ClientSend(new Command { CommandTerm = "JOIN", Data = GameListBox.SelectedItem });
                var response = _clientSocket.JoinGameSucceeded();
                if (response)
                {
                    GameGrid.Visibility = Visibility.Visible;
                    GameMenuContainer.Visibility = Visibility.Hidden;
                    MultiplayerGameInfoContainer.Visibility = Visibility.Visible;
                    Client.Fill = Brushes.Blue;
                    Opponent.Fill = Brushes.Red;
                }
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
            _clientSocket.ClientSend(new Command { CommandTerm = "LEAVE", Data = _multiplayerGameId});
        }
    }
}
