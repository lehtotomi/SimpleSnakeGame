using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

public class MainForm : Form
{
    private List<Point> snake = new List<Point>();
    //private List<Point> snake2 = new List<Point>(); // Player 2 Snake
    private List<Point> obstacles = new List<Point>();
    private List<Point> traps = new List<Point>();
    private Timer gameTimer = new Timer();
    private int direction = 0; // 0: right, 1: down, 2: left, 3: up
    private const int CellSize = 20;
    private int score = 0;
    private int currentLevel = 1;
    private bool isPaused = false;
    private int remainingTime = 60; // Timer challenge mode
    private SoundPlayer foodSound = new SoundPlayer("food.wav");
    private SoundPlayer gameOverSound = new SoundPlayer("gameover.wav");

    private enum FoodType { Normal, DoublePoints, SlowTimer, SpeedUp, ReverseControls }
    private FoodType currentFoodType;
    private Point food;

    public MainForm()
    {
        // Form setup
        this.ClientSize = new Size(400, 400);
        this.Text = "Snake Game - Level: 1 - Score: 0";
        this.DoubleBuffered = true; // Enable double buffering
        // Initialize snakes
        snake.Add(new Point(5, 5));

        // Spawn initial food and obstacles
        SpawnObstacles();
        SpawnTraps();
        SpawnFood();

        // Timer setup
        gameTimer.Interval = 200; // Initial game speed
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();

        // Event handlers
        this.Paint += MainForm_Paint;
        this.KeyDown += MainForm_KeyDown;
    }

    private void SpawnFood()
    {
        Random random = new Random();
        do
        {
            // Generate random coordinates within the grid
            food = new Point(
                random.Next(this.ClientSize.Width / CellSize),
                random.Next(this.ClientSize.Height / CellSize)
            );
        }
        while (
            snake.Contains(food) ||          // Avoid snake segments
            obstacles.Contains(food) ||      // Avoid obstacles
            traps.Contains(food)             // Avoid traps
        ); // Repeat until food is in a valid position
        Console.WriteLine($"Food spawned at: {food.X}, {food.Y}");
    }

    private void SpawnObstacles()
    {
        Random random = new Random();
        obstacles.Clear();
        for (int i = 0; i < currentLevel * 5; i++) // Increase obstacles by level
        {
            Point obstacle;
            do
            {
                obstacle = new Point(
                    random.Next(this.ClientSize.Width / CellSize),
                    random.Next(this.ClientSize.Height / CellSize)
                );
            } while (snake.Contains(obstacle) || obstacle == food);
            obstacles.Add(obstacle);
        }
    }

    private void SpawnTraps()
    {
        Random random = new Random();
        traps.Clear();
        for (int i = 0; i < currentLevel * 3; i++) // Increase traps by level
        {
            traps.Add(new Point(
                random.Next(this.ClientSize.Width / CellSize),
                random.Next(this.ClientSize.Height / CellSize)
            ));
        }
    }

    private void ApplyFoodEffect()
    {
        switch (currentFoodType)
        {
            case FoodType.DoublePoints:
                score += 20;
                break;
            case FoodType.SlowTimer:
                gameTimer.Interval += 50;
                break;
            case FoodType.SpeedUp:
                if (gameTimer.Interval > 50) gameTimer.Interval -= 20;
                break;
            case FoodType.ReverseControls:
                direction = (direction + 2) % 4; // Reverse direction
                break;
            default:
                score += 10;
                break;
        }
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
        if (isPaused) return;

        MoveSnake(snake, ref direction);
        CheckLevelProgression();
        this.Text = $"Snake Game - Level: {currentLevel} - Score: {score} - Time Left: {remainingTime}s";
    }

    private void MoveSnake(List<Point> snake, ref int direction)
    {
        Point head = snake[0];
        Point newHead = snake[0]; // Get the current head position

        switch (direction)
        {
            case 0: newHead.X++; break; // Right
            case 1: newHead.Y++; break; // Down
            case 2: newHead.X--; break; // Left
            case 3: newHead.Y--; break; // Up
        }

        if (CheckCollision(newHead, snake))
        {
            GameOver();
            return;
        }

        snake.Insert(0, newHead);

        if (newHead == food)
        {
            ApplyFoodEffect();
            SpawnFood();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1); // Remove tail
        }

        this.Invalidate(); // Trigger repaint
    }

    private bool CheckCollision(Point newHead, List<Point> snake)
    {
        return newHead.X < 0 || newHead.Y < 0 || // Out of bounds (left/top)
               newHead.X >= this.ClientSize.Width / CellSize || // Out of bounds (right)
               newHead.Y >= this.ClientSize.Height / CellSize || // Out of bounds (bottom)
               obstacles.Contains(newHead) || // Hit an obstacle
               traps.Contains(newHead) || // Hit a trap
               snake.Contains(newHead); // Hit itself
    }

    private void CheckLevelProgression()
    {
        if (score >= currentLevel * 100) // Level up every 100 points
        {
            currentLevel++;
            SpawnObstacles();
            SpawnTraps();
            SpawnFood();
            this.ClientSize = new Size(400 + currentLevel * 50, 400 + currentLevel * 50); // Expand play area
        }
    }

    private void GameOver()
    {
        gameTimer.Stop();
        MessageBox.Show($"Game Over! Final Score: {score}");

        File.AppendAllText("highscores.txt", $"{score}\n"); // Save high score

        score = 0;
        snake.Clear();
        snake.Add(new Point(5, 5));
        
        SpawnObstacles();
        SpawnTraps();
        SpawnFood();
        gameTimer.Interval = 200;
        remainingTime = 60;
        this.Text = "Snake Game - Level: 1 - Score: 0";
        gameTimer.Start();
    }

    private void MainForm_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        DrawGrid(g);
        DrawSnake(g, snake, Brushes.Green);
        DrawFood(g); // Ensure food is being drawn
        DrawObstacles(g);
        DrawTraps(g);
    }

    private void DrawGrid(Graphics g)
    {
        for (int x = 0; x < this.ClientSize.Width; x += CellSize)
        {
            g.DrawLine(Pens.Black, x, 0, x, this.ClientSize.Height);
        }
        for (int y = 0; y < this.ClientSize.Height; y += CellSize)
        {
            g.DrawLine(Pens.Black, 0, y, this.ClientSize.Width, y);
        }
    }

    private void DrawSnake(Graphics g, List<Point> snake, Brush brush)
    {
        foreach (Point segment in snake)
        {
            g.FillRectangle(brush, segment.X * CellSize, segment.Y * CellSize, CellSize, CellSize);
        }
    }

    private void DrawFood(Graphics g)
    {
        Brush brush = Brushes.Cyan; // Color of the food
        g.FillEllipse(brush, food.X * CellSize, food.Y * CellSize, CellSize, CellSize); // Draw food as a circle
    }


    private void DrawObstacles(Graphics g)
    {
        foreach (Point obstacle in obstacles)
        {
            g.FillRectangle(Brushes.Gray, obstacle.X * CellSize, obstacle.Y * CellSize, CellSize, CellSize);
        }
    }

    private void DrawTraps(Graphics g)
    {
        foreach (Point trap in traps)
        {
            g.FillRectangle(Brushes.DarkRed, trap.X * CellSize, trap.Y * CellSize, CellSize, CellSize);
        }
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.P)
        {
            isPaused = !isPaused;
            this.Text = isPaused ? "Snake Game - Paused" : $"Snake Game - Level: {currentLevel} - Score: {score}";
        }

        if (!isPaused)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: if (direction != 1) direction = 3; break; // Up for Player 1
                case Keys.Down: if (direction != 3) direction = 1; break; // Down for Player 1
                case Keys.Left: if (direction != 0) direction = 2; break; // Left for Player 1
                case Keys.Right: if (direction != 2) direction = 0; break; // Right for Player 1
            }
        }
    }
}