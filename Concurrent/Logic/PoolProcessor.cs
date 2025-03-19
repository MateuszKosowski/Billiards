using Data;
using System.Diagnostics;
using System.Timers;
using System;

namespace Logic
{
    public class PoolProcessor
    {
        private Data.PoolTable _poolTable { get; init; }
        private Stopwatch stopwatch = new Stopwatch();
        private readonly System.Timers.Timer timer;

        // Konstruktor
        public PoolProcessor(Data.PoolTable poolTable)
        {
            _poolTable = poolTable;
            timer = new System.Timers.Timer(10);
            timer.Elapsed += Update;
            timer.AutoReset = true;
        }

        // Dodanie kuli do stołu
        public void AddBall(Ball ball)
        {
            _poolTable.Balls.Add(ball);
        }

        // Rozpoczęcie symulacji
        public void Start()
        {
            timer.Start();
            stopwatch.Start();
     
        }

        // Funkcja aktualizująca pozycje kul co 10ms
        public void Update(object? sender, ElapsedEventArgs e)
        {
            double timeDelta = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            foreach (var ball in _poolTable.Balls)
            {
                ball.PositionX += ball.VelocityX * timeDelta;
                ball.PositionY += ball.VelocityY * timeDelta;
                isWallHit(ball);
            }


            // Roboczo do wyświetlenia pozycji kul
            foreach (var ball in _poolTable.Balls)
            {
                Console.WriteLine($"Kula {ball.Color} w pozycji ({ball.PositionX:F2}, {ball.PositionY:F2})");
            }
        }

        // Funkcja sprawdzająca czy kula uderzyła w ścianę
        public void isWallHit(Ball ball)
        {

            if (ball.PositionX + ball.Radius >= _poolTable.Width || ball.PositionX - ball.Radius <= 0)
            {
                ball.VelocityX = -ball.VelocityX;
                Console.WriteLine($"Kula {ball.Color} odbiła się od ściany na X");
            }
            if (ball.PositionY + ball.Radius >= _poolTable.Height || ball.PositionY - ball.Radius <= 0)
            {
                ball.VelocityY = -ball.VelocityY;
                Console.WriteLine($"Kula {ball.Color} odbiła się od ściany na Y");
            }
        }

        static void Main(string[] args)
        {
            PoolProcessor proc = new PoolProcessor(new PoolTable(100, 100));

            // Dodaj dwie kule z różnymi prędkościami
            proc.AddBall(new Ball(1, "red", 2, 1, 1, 1, 1));
            proc.AddBall(new Ball (1, "blue", 5, 3, 3, 2, 2));

            // Uruchom symulację
            proc.Start();

        }
    }
}