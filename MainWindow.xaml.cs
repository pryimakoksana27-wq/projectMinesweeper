using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Minesweeper
{
    public class MineCell
    {
        public Button VisualButton { get; set; } = null!;
        public bool Mine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        public int MinesAround { get; set; }
    }
    public partial class MainWindow : Window
    {
        private int mapSize = 9;
        private int mineCount = 10;
        private MineCell[,] board = null!;
        private bool gameOver = false;
        private bool firstClick = true;
        private DispatcherTimer timer = null!;
        private int seconds = 0;
        private int placeFlags = 0;
        private int deathCount = 3;
        private Random random = new Random();
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
        private void UpdateMines()
        {
            int left = mineCount - placeFlags;
            minesLeftText.Text = "Flags left: " + left;
        }
        private void UpdateLives()
        {
            livesText.Text = "Lives: " + deathCount;
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
        private void InitGame()
        {
            timer.Stop();
            gameOver = false;
            firstClick = true;
            board = new MineCell[mapSize, mapSize];
            gameGrid.Children.Clear();
            gameGrid.Rows = mapSize;
            gameGrid.Columns = mapSize;
            seconds = 0;
            placeFlags = 0;
            deathCount = 3;
            UpdateMines();
            UpdateLives();
            timerText.Text = "Time: 0";
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    Button button = CreateCell(x, y);
                    MineCell cell = new MineCell();
                    cell.VisualButton = button;
                    board[x, y] = cell;

                    gameGrid.Children.Add(button);
                }
            }
            Resize();
        }
//game logic
        private void Resize()
        {
            this.Width = Math.Max(600, mapSize * 32 + 60);
            this.Height = Math.Max(600, mapSize * 32 + 180);
        }
        // mines
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
        private void CellClick(object sender, RoutedEventArgs e)
        {
            if (gameOver) return;
            Button button = (Button)sender;
            Point position = (Point)button.Tag;
            int x = (int)position.X;
            int y = (int)position.Y;
            RevealCell(x, y);

        }
        // cells` logic
        private void RevealCell(int x, int y) {
            if (gameOver) return;
            if (firstClick)
            {
                firstClick = false;
                PlaceMines(x, y);
                AllMinesAround();
                timer.Start();
            }
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(x, y));
            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int cx = (int)p.X;
                int cy = (int)p.Y;
                if (cx < 0 || cx >= mapSize || cy < 0 || cy >= mapSize) continue;
                MineCell cell = board[cx, cy];
                if (cell.IsRevealed) continue;
                if (cell.IsFlagged) continue;

                cell.IsRevealed = true;
                cell.VisualButton.IsEnabled = false;
                if (cell.Mine)
                {
                    cell.VisualButton.Content = "💣";
                    deathCount--;
                    UpdateLives();
                    if (deathCount <= 0)
                    {
                        GameOver();
                        ShowExplosion();
                        return;
                    }
                    continue;
                }
                if (cell.MinesAround > 0)
                {
                    cell.VisualButton.Content = cell.MinesAround.ToString();
                    cell.VisualButton.Foreground = NumberColor(cell.MinesAround);
                }
                else
                {
                    cell.VisualButton.Content = "";
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
                                    queue.Enqueue(new Point(nx, ny));
                                }
                            }
                        }
                    }
                }
            }
            CheckWin();
        }
        private void FlagCell(object sender, MouseButtonEventArgs e)
        {
            if (gameOver) return;
            Button button = (Button)sender;
            Point position = (Point)button.Tag;
            int x = (int)position.X;
            int y = (int)position.Y;

            MineCell cell = board[x, y];
            if (cell.IsRevealed) return;
            if (!cell.IsFlagged)
            {
                if (placeFlags >= mineCount) return;
                cell.IsFlagged = true;
                button.Content = "🚩";
                placeFlags++;
            }
            else
            {
                cell.IsFlagged = false;
                button.Content = "";
                placeFlags--;
            }
            UpdateMines();
            e.Handled = true;
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
        // the ending of the game, win conditions
        private void CheckWin()
        {
            if (gameOver) return;
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    MineCell cell = board[x, y];
                    if (!cell.Mine && !cell.IsRevealed) return;
                }
            }
            WinGame();
        }
        private void WinGame()
        {
            gameOver = true;
            timer.Stop();
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    MineCell cell = board[x, y];
                    cell.VisualButton.IsEnabled = false;
                    if (cell.Mine)
                    {
                        cell.VisualButton.Content = "🚩";
                    }
                }
            }
            MessageBox.Show("You win!🥳");
        }
        private void GameOver()
        {
            gameOver = true;
            timer.Stop();
            for (int x = 0;  x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    MineCell cell = board[x, y];
                    cell.VisualButton.IsEnabled = false;
                    if (cell.Mine)
                    {
                        cell.VisualButton.Content = "💣";
                    } else if (cell.IsFlagged)
                    {
                        cell.VisualButton.Content = "❌";
                    }
                }
            }
        }
//interface
        // number`s colors
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
        // buttons
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
    }
}