using Minesweeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper
{
    public class MineCell
    {
        public bool Mine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        public int MinesAround { get; set; }
    }
    // game logic class
    public class GameBoard
    {
        private int mapSize;
        private int mineCount;
        private MineCell[,] board;
        private Random random = new Random();
        private bool gameOver = false;
        private bool firstClick = true;
        private int placeFlags = 0;
        private int deathCount = 3;
        private bool isWon = false;
        // => we make public with this name
        public MineCell[,] Board => board;
        public int PlaceFlags => placeFlags;
        public int DeathCount => deathCount;
        public bool GameOverState => gameOver;
        public bool IsWon => isWon;
        public bool FirstClick => firstClick;
        public GameBoard(int mapSize, int mineCount)
        {
            this.mapSize = mapSize;
            this.mineCount = mineCount;
            board = new MineCell[mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    board[x, y] = new MineCell();
                }
            }
        }
        public void InitGame()
        {
            gameOver = false;
            firstClick = true;
            placeFlags = 0;
            deathCount = 3;
            board = new MineCell[mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    board[x, y] = new MineCell();
                }
            }
        }
        // mines
        public void SetSizeAndMines(int mapSize, int mineCount)
        {
            this.mapSize = mapSize;
            this.mineCount = mineCount;
            board = new MineCell[mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    board[x, y] = new MineCell();
                }
            }

        }
        private void PlaceMines(int startX, int startY)
        {
            int placed = 0;
            while (placed < mineCount)
            {
                int x = random.Next(mapSize);
                int y = random.Next(mapSize);
                if (!board[x, y].Mine && !(x == startX && y == startY))
                {
                    board[x, y].Mine = true;
                    placed++;
                }
            }
        }
        private int CountMinesAround(int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < mapSize && ny >= 0 && ny < mapSize)
                    {
                        if (board[nx, ny].Mine)
                            count++;
                    }
                }
            }
            return count;
        }
        private void AllMinesAround()
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    board[x, y].MinesAround = CountMinesAround(x, y);
                }
            }
        }
        // flags
        private int AllFlagsAround(int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < mapSize && ny >= 0 && ny < mapSize)
                    {
                        if (board[nx, ny].IsFlagged)
                            count++;
                    }
                }
            }
            return count;
        }
        public void OpenNeighbours(int x, int y)
        {
            if (gameOver) return;
            MineCell cell = board[x, y];
            if (!cell.IsRevealed || cell.MinesAround == 0) return;
            int flags = AllFlagsAround(x, y);
            if (flags != cell.MinesAround) return;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= mapSize || ny < 0 || ny >= mapSize) continue;
                    MineCell neighbour = board[nx, ny];
                    if (!neighbour.IsFlagged && !neighbour.IsRevealed)
                    {
                        neighbour.IsRevealed = true;
                        if (neighbour.Mine)
                        {
                            deathCount--;
                            if (deathCount <= 0)
                            {
                                GameOver();
                                return;
                            }
                        }
                    }
                }
            }
        }
        // logic of opening cell
        public void RevealCell(int x, int y)
        {
            if (gameOver) return;
            if (firstClick)
            {
                firstClick = false;
                PlaceMines(x, y);
                AllMinesAround();
            }
            Queue<(int X, int Y)> queue = new Queue<(int X, int Y)>();
            queue.Enqueue((x, y));
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                int cx = (int)p.X;
                int cy = (int)p.Y;
                if (cx < 0 || cx >= mapSize || cy < 0 || cy >= mapSize) continue;
                MineCell cell = board[cx, cy];
                if (cell.IsRevealed) continue;
                if (cell.IsFlagged) continue;

                cell.IsRevealed = true;
                if (cell.Mine)
                {
                    deathCount--;
                    if (deathCount <= 0)
                    {
                        GameOver();
                        return;
                    }
                    continue;
                }
                if (cell.MinesAround == 0)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;
                            int nx = cx + dx;
                            int ny = cy + dy;
                            if (nx >= 0 && nx < mapSize && ny >= 0 && ny < mapSize)
                            {
                                if (!board[nx, ny].IsRevealed && !board[nx, ny].Mine)
                                {
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
            }
            CheckWin();
        }
        public void FlagCell(int x, int y)
        {
            if (gameOver) return;
            MineCell cell = board[x, y];
            if (cell.IsRevealed) return;
            if (!cell.IsFlagged)
            {
                if (placeFlags >= mineCount) return;
                cell.IsFlagged = true;
                placeFlags++;
            }
            else
            {
                cell.IsFlagged = false;
                placeFlags--;
            }
        }
        // checking of the winning conditions
        private void CheckWin()
        {
            if (gameOver) return;
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    MineCell cell = board[x, y];
                    if (!board[x, y].Mine && !board[x, y].IsRevealed) return;
                }
            }
            WinGame();
        }
        private void WinGame()
        {
            gameOver = true;
            isWon = true;
        }
        private void GameOver()
        {
            gameOver = true;
            isWon = false;
        }
    }
    // interface
    public partial class MainWindow : Window
    {
        private int mapSize = 9;
        private int mineCount = 10;
        private DispatcherTimer? timer;
        private GameBoard? gameBoard;
        private Button[,]? buttons;
        private int seconds = 0;
        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            InitGame();
        }
        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
        }
        private void TimerTick(object? sender, EventArgs e)
        {
            seconds++;
            timerText.Text = "Time: " + seconds;
        }
        private void InitGame()
        {
            timer?.Stop();
            seconds = 0;
            timerText.Text = "Time: 0";
            gameBoard = new GameBoard(mapSize, mineCount);
            gameBoard.InitGame();
            gameGrid.Children.Clear();
            gameGrid.Rows = mapSize;
            gameGrid.Columns = mapSize;
            buttons = new Button[mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Button button = CreateCell(x, y);
                    buttons[x, y] = button;
                    gameGrid.Children.Add(button);
                }
            }
            Resize();
            RenderBoard();
        }
        private Button CreateCell(int x, int y)
        {
            Button button = new Button();
            button.Tag = new Point(x, y);
            button.Click += CellClick;
            button.MouseRightButtonUp += FlagCell;
            button.Width = 32;
            button.Height = 32;
            button.Padding = new Thickness(0);
            button.Margin = new Thickness(0);
            button.BorderThickness = new Thickness(1);
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
            button.VerticalContentAlignment = VerticalAlignment.Center;
            button.FocusVisualStyle = null;
            return button;
        }
        private void CellClick(object sender, RoutedEventArgs e)
        {
            if (gameBoard == null) return;
            bool wasFirstClick = gameBoard.FirstClick;
            Button button = (Button)sender;
            Point position = (Point)button.Tag;
            int x = (int)position.X;
            int y = (int)position.Y;
            gameBoard.RevealCell(x, y);
            if (gameBoard.Board[x, y].IsRevealed) gameBoard.OpenNeighbours(x, y);
            if (wasFirstClick) timer?.Start();
            RenderBoard();
        }
        private void FlagCell(object sender, RoutedEventArgs e)
        {
            if (gameBoard == null) return;
            Button button = (Button)sender;
            Point position = (Point)button.Tag;
            int x = (int)position.X;
            int y = (int)position.Y;
            gameBoard.FlagCell(x, y);
            RenderBoard();
            e.Handled = true;
        }
        private void RenderBoard()
        {
            if (buttons == null || gameBoard == null) return;
            MineCell[,] board = gameBoard.Board;
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    MineCell cell = board[x, y];
                    Button button = buttons[x, y];
                    if (cell.IsRevealed)
                    {
                        button.Background = Brushes.White;
                        if (cell.Mine)
                        {
                            button.Content = "💣";
                        }
                        else if (cell.MinesAround > 0)
                        {
                            button.Content = cell.MinesAround.ToString();
                            button.Foreground = NumberColor(cell.MinesAround);
                        }
                        else button.Content = "";
                    }
                    else
                    {
                        button.IsEnabled = true;
                        button.Content = cell.IsFlagged ? "🚩" : "";
                    }
                }
            }
            UpdateMines();
            UpdateLives();
            if (gameBoard.GameOverState)
            {
                timer?.Stop();
                if (gameBoard.IsWon) MessageBox.Show("You win!");
                else ShowExplosion();
            }
        }

        private void UpdateMines()
        {
            if (gameBoard == null) return;
            int left = mineCount - gameBoard.PlaceFlags;
            minesLeftText.Text = "Flags left: " + left;
        }
        private void UpdateLives()
        {
            if (gameBoard == null) return;
            livesText.Text = "Lives: " + gameBoard.DeathCount;
        }
        private Brush NumberColor(int number)
        {
            switch (number)
            {
                case 1: return Brushes.Blue;
                case 2: return Brushes.Green;
                case 3: return Brushes.Red;
                case 4: return Brushes.DarkBlue;
                case 5: return Brushes.DarkRed;
                case 6: return Brushes.Teal;
                case 7: return Brushes.Black;
                case 8: return Brushes.Gray;
                default: return Brushes.Black;
            }
        }
        private void EasyClick(object sender, RoutedEventArgs e)
        {
            mapSize = 10;
            mineCount = 10;
            InitGame();
        }
        private void MediumClick(object sender, RoutedEventArgs e)
        {
            mapSize = 12;
            mineCount = 25;
            InitGame();
        }
        private void HardClick(object sender, RoutedEventArgs e)
        {
            mapSize = 16;
            mineCount = 50;
            InitGame();
        }
        private void RestartClick(object sender, RoutedEventArgs e)
        {
            InitGame();
        }
        private void Resize()
        {
            this.Width = Math.Max(600, mapSize * 32 + 60);
            this.Height = Math.Max(600, mapSize * 32 + 180);
        }
        private void ShowExplosion()
            {
                Window explosion = new Window
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 200,
                    Height = 200,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.ToolWindow,
                    Title = "Boom!",
                    Content = new TextBlock
                    {
                        Text = "💥",
                        FontSize = 60,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    }
                };
                explosion.ShowDialog();
            }
        }
    }
