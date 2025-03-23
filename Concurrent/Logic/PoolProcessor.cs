using Data;
using System.Diagnostics;
using System.Timers;
using System;
using System.Numerics;

namespace Logic
{
    public class PoolProcessor
    {
        private Data.PoolTable _poolTable { get; init; }
        private Stopwatch stopwatch = new Stopwatch();
        private readonly System.Timers.Timer timer;
        public event EventHandler<BallsCollisionEventArgs> BallsCollision;
        public event EventHandler<WallsCollisionEventArgs> WallsCollision;


        public class BallsCollisionEventArgs : EventArgs
        {
            public Ball Ball1 { get; set; }
            public Ball Ball2 { get; set; }
            public DateTime CollisionTime { get; set; }
        }

        public class WallsCollisionEventArgs : EventArgs
        {
            public Ball Ball { get; set; }
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
            if (IsWallCollision(ball))
            {
                WallsCollision?.Invoke(this, new WallsCollisionEventArgs
                {
                    Ball = ball,
                    CollisionTime = DateTime.Now
                });
            }
        }

        public bool IsWallCollision(Ball ball)
        {
            bool isCollision = false;
            // Zderzenia na X
            if (ball.PositionX + ball.Radius >= _poolTable.Width)
            {
                ball.VelocityX = -ball.VelocityX;
                ball.PositionX = _poolTable.Width - ball.Radius - 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od prawej ściany");
                isCollision = true;

            }
            else if (ball.PositionX - ball.Radius <= 0)
            {
                ball.VelocityX = -ball.VelocityX;
                ball.PositionX = ball.Radius + 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od lewej ściany");
                isCollision = true;
            }

            // Zderzenia na Y
            if (ball.PositionY + ball.Radius >= _poolTable.Height)
            {
                ball.VelocityY = -ball.VelocityY;
                ball.PositionY = _poolTable.Height - ball.Radius - 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od dolnej ściany");
                isCollision = true;
            }
            else if (ball.PositionY - ball.Radius <= 0)
            {
                ball.VelocityY = -ball.VelocityY;
                ball.PositionY = ball.Radius + 0.01;
                Console.WriteLine($"Kula {ball.Color} odbiła się od górnej ściany");
                return true;
            }

            return isCollision;
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

                    BallsCollision?.Invoke(this, new BallsCollisionEventArgs
                    {
                        Ball1 = ball,
                        Ball2 = otherBall,
                        CollisionTime = DateTime.Now
                    });


                    ResolveCollision(ball, otherBall, dx, dy, distance);

                    return true;
                }
            }

            return false;
        }

        public void ResolveCollision(Ball ballA, Ball ballB, double distanceX, double distanceY, double distance)
        {     
            // Normalizacja wektora
            distanceX /= distance;
            distanceY /= distance;

            // Wektor styczny (prostopadły do normalnego)
            double tangentX = -distanceY;
            double tangentY = -distanceX;

            // Rzutowanie prędkości na osie normalnej i stycznej
            double velocityANormal = ballA.VelocityX * distanceX + ballA.VelocityY * distanceY;
            double velocityATangent = ballA.VelocityX * tangentX + ballA.VelocityY * tangentY;
            double velocityBNormal = ballB.VelocityX * distanceX + ballB.VelocityY * tangentY;
            double velocityBTangent = ballB.VelocityX * tangentX + ballB.VelocityY * tangentY;

            // Zmiana składowych normalnych
            double velocityANormalNew = velocityBNormal;
            double velocityBNormalNew = velocityANormal;

            Console.WriteLine("Predkosc kuli A: " + ballA.VelocityX + " " + ballA.VelocityY);
            Console.WriteLine("Predkosc kuli B: " + ballB.VelocityX + " " + ballB.VelocityY);

            // Trnsformacja na układ globaly
            ballA.VelocityX = velocityANormalNew * distanceX + velocityATangent * tangentX;
            ballA.VelocityY = velocityANormalNew * distanceY + velocityATangent * tangentY;
            ballB.VelocityX = velocityBNormalNew * distanceX + velocityBTangent * tangentX;
            ballB.VelocityY = velocityBNormalNew * distanceY + velocityBTangent * tangentY;
            
            Console.WriteLine("Nowa predkosc kuli A: " + ballA.VelocityX + " " + ballA.VelocityY);
            Console.WriteLine("Nowa predkosc kuli B: " + ballB.VelocityX + " " + ballB.VelocityY);


        }
    }
}