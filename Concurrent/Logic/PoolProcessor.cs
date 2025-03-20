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

        public bool BallCollisionDetected { get; private set; } = false;
        public event EventHandler<BallCollisionEventArgs> BallCollision;

        public class BallCollisionEventArgs : EventArgs
        {
            public Ball Ball1 { get; set; }
            public Ball Ball2 { get; set; }
            public DateTime CollisionTime { get; set; }
        }

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

        public void Stop()
        {
            timer.Stop();
            stopwatch.Stop();
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
                HandleWallCollision(ball);
                IsAnotherBallColliding(ball);
            }


            // Roboczo do wyświetlenia pozycji kul
            foreach (var ball in _poolTable.Balls)
            {
                Console.WriteLine($"Kula {ball.Color} w pozycji ({ball.PositionX:F2}, {ball.PositionY:F2})");
            }
        }

        // Funkcja sprawdzająca czy kula uderzyła w ścianę
        public void HandleWallCollision(Ball ball)
        {
            // Zderzenia na X
            if (ball.PositionX + ball.Radius >= _poolTable.Width)
            {
                ball.VelocityX = -ball.VelocityX;
                ball.PositionX = _poolTable.Width - ball.Radius - 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od prawej ściany");
            }
            else if (ball.PositionX - ball.Radius <= 0)
            {
                ball.VelocityX = -ball.VelocityX;
                ball.PositionX = ball.Radius + 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od lewej ściany");
            }

            // Zderzenia na Y
            if (ball.PositionY + ball.Radius >= _poolTable.Height)
            {
                ball.VelocityY = -ball.VelocityY;
                ball.PositionY = _poolTable.Height - ball.Radius - 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od dolnej ściany");
            }
            else if(ball.PositionY - ball.Radius <= 0)
            {
                ball.VelocityY = -ball.VelocityY;
                ball.PositionY = ball.Radius + 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od górnej ściany");
            }
        }

        public bool IsAnotherBallColliding(Ball ball)
        {
            foreach (Ball otherBall in _poolTable.Balls)
            {
                // Nie prowadź testu kolizji z samą sobą
                if (otherBall == ball) continue;

                // obliczenie odległości między kulami
                double dx = ball.PositionX - otherBall.PositionX;
                double dy = ball.PositionY - otherBall.PositionY;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                // jeśli dystans jest mniejszy niż suma promieni kul, to znaczy, że kule się zderzają
                if (distance < (ball.Radius + otherBall.Radius))
                {
                    Console.WriteLine($"Kula {ball.Color} zderzyła się z kulą {otherBall.Color}");

                    BallCollisionDetected = true;

                    BallCollision?.Invoke(this, new BallCollisionEventArgs
                    {
                        Ball1 = ball,
                        Ball2 = otherBall,
                        CollisionTime = DateTime.Now
                    });

                    ball.VelocityX = -ball.VelocityX;
                    ball.VelocityY = -ball.VelocityY;
                    otherBall.VelocityY = -otherBall.VelocityY;
                    otherBall.VelocityX = -otherBall.VelocityX;
                    return true;
                }
            }

            return false;
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