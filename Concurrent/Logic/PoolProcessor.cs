using Data.Api;
using Data.Entities;
using System.Diagnostics;
using System.Timers;

namespace Logic
{
    public class PoolProcessor : IPoolProcessor
    {
        private readonly IDataApi _dataApi;

        private System.Timers.Timer _updateTimer;
        private Stopwatch stopwatch = new Stopwatch();
        public event EventHandler<BallsCollisionEventArgs> BallsCollision;
        public event EventHandler<WallsCollisionEventArgs> WallsCollision;
        public event EventHandler<IBall> BallMoving;
        private List<String> _allColors = new List<String> { "Red", "Blue", "Lime", "Pink", "Brown", "Yellow", "Orange", "Purple", "Gold", "Green", "Black", "White"};

        public class BallsCollisionEventArgs : EventArgs
        {
            public IBall Ball1 { get; set; }
            public IBall Ball2 { get; set; }
            public DateTime CollisionTime { get; set; }
        }

        public class WallsCollisionEventArgs : EventArgs
        {
            public IBall Ball { get; set; }
            public DateTime CollisionTime { get; set; }
        }

        // Konstruktor
        public PoolProcessor()
        {
            _dataApi = new DataApi();
            _updateTimer = new System.Timers.Timer(10);
            _updateTimer.Elapsed += Update;
            _updateTimer.AutoReset = true;
        }

        // Rozpoczęcie symulacji
        public void Start()
        {
            _updateTimer.Start();
            stopwatch.Start();

        }

        public void Stop()
        {
            _updateTimer.Stop();
            stopwatch.Stop();
        }

        // Funkcja aktualizująca pozycje kul co 10ms
        public void Update(object? sender, ElapsedEventArgs e)
        {
            double timeDelta = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            var balls = _dataApi.GetAllBallsFromTable().ToList();
            foreach (var ball in balls)
            {
                double newX = (double)ball.Position[0];
                newX += ball.Velocity[0] * timeDelta;
                double newY = (double)ball.Position[1];
                newY += ball.Velocity[1] * timeDelta;

                _dataApi.UpdateBall(ball, (float)newX, (float)newY, null, null);

                HandleWallCollision(ball);

                BallMoving?.Invoke(this, ball);

                IsAnotherBallColliding(ball);
            }
        }

        // Funkcja sprawdzająca czy kula uderzyła w ścianę
        public void HandleWallCollision(IBall ball)
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

        public bool IsWallCollision(IBall ball)
        {
            bool isCollision = false;

            // Zderzenia na X
            if (ball.Position[0] + ball.Radius >= _dataApi.GetTableSize()[0])
            {
                float newVX = ball.Velocity[0];
                newVX = -newVX;

                double newX = (double)ball.Position[0];
                newX = _dataApi.GetTableSize()[0] - ball.Radius - 0.01;

                _dataApi.UpdateBall(ball, (float)newX, ball.Position[1], newVX, null);

                Console.WriteLine($"Kula {ball.Color} odbiła się od prawej ściany");
                isCollision = true;

            }
            else if (ball.Position[0] - ball.Radius <= 0)
            {
                float newVX = ball.Velocity[0];
                newVX = -newVX;

                double newX = (double)ball.Position[0];
                newX = ball.Radius + 0.01;

                _dataApi.UpdateBall(ball, (float)newX, ball.Position[1], newVX, null);

                Console.WriteLine($"Kula {ball.Color} odbiła się od lewej ściany");
                isCollision = true;
            }

            // Zderzenia na Y
            if (ball.Position[1] + ball.Radius >= _dataApi.GetTableSize()[1])
            {
                float newVY = ball.Velocity[1]; // Zmień z ball.Velocity[0] na ball.Velocity[1]
                newVY = -newVY;

                double newY = (double)ball.Position[1];
                newY = _dataApi.GetTableSize()[1] - ball.Radius - 0.01;

                _dataApi.UpdateBall(ball, ball.Position[0], (float)newY, null, newVY);

                Console.WriteLine($"Kula {ball.Color} odbiła się od dolnej ściany");
                isCollision = true;
            }
            else if (ball.Position[1] - ball.Radius <= 0)
            {
                float newVY = ball.Velocity[1]; // Zmień z ball.Velocity[0] na ball.Velocity[1]
                newVY = -newVY;

                double newY = (double)ball.Position[1];
                newY = ball.Radius + 0.01;

                _dataApi.UpdateBall(ball, ball.Position[0], (float)newY, null, newVY);

                Console.WriteLine($"Kula {ball.Color} odbiła się od górnej ściany");
                isCollision = true; // Zmień return true na isCollision = true
            }


            return isCollision;
        }

        public void AddBalls(int _amount)
        {
           for(int i = 0; i < _amount; i++)
            {
                IBall ball = _dataApi.CreateBall(_allColors[i], 20, i + 1);
                _dataApi.AddBallToTable(ball);
            }
        }

        public void ClearTable()
        {
            var balls = _dataApi.GetAllBallsFromTable().ToList();
            foreach (var ball in balls)
            {
                _dataApi.DeleteBallFromTable(ball);
            }
        }

        public IEnumerable<IBall> GetAllBallsFromTable()
        {
            return _dataApi.GetAllBallsFromTable();
        }

        public void CreateTable(float _width, float _height)
        {
            _dataApi.CreatePoolTable(_width, _height);
        }


        //-----------------------NA KOLEJNY ETAP-------------------------------------

        public bool IsAnotherBallColliding(IBall ball)
        {
            foreach (IBall otherBall in _dataApi.GetAllBallsFromTable())
            {
                // Nie prowadź testu kolizji z samą sobą
                if (otherBall == ball) continue;

                // obliczenie odległości między kulami
                double dx = ball.Position[0] - otherBall.Position[0];
                double dy = ball.Position[1] - otherBall.Position[1];
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

        public void ResolveCollision(IBall ballA, IBall ballB, double distanceX, double distanceY, double distance)
        {
            // Normalizacja wektora kolizji
            double nx = distanceX / distance;
            double ny = distanceY / distance;

            // Masa kul (waga)
            double m1 = ballA.Weight;
            double m2 = ballB.Weight;

            // Prędkości przed zderzeniem
            double v1x = ballA.Velocity[0];
            double v1y = ballA.Velocity[1];
            double v2x = ballB.Velocity[0];
            double v2y = ballB.Velocity[1];

            // Składowe normalne prędkości
            double v1n = v1x * nx + v1y * ny;
            double v2n = v2x * nx + v2y * ny;

            // Składowe styczne prędkości (nie zmieniają się przy zderzeniu sprężystym)
            double v1t = -v1x * ny + v1y * nx;
            double v2t = -v2x * ny + v2y * nx;

            // Nowe składowe normalne po zderzeniu (wzory dla zderzenia sprężystego)
            double v1nAfter = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
            double v2nAfter = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

            // Przekształcenie z powrotem na składowe x, y
            double v1xAfter = v1nAfter * nx - v1t * ny;
            double v1yAfter = v1nAfter * ny + v1t * nx;
            double v2xAfter = v2nAfter * nx - v2t * ny;
            double v2yAfter = v2nAfter * ny + v2t * nx;

            // Rozdziel kule, aby nie zachodziły na siebie
            double overlap = (ballA.Radius + ballB.Radius) - distance;
            if (overlap > 0)
            {
                // Proporcjonalnie do masy
                double totalMass = m1 + m2;
                double moveA = overlap * (m2 / totalMass);
                double moveB = overlap * (m1 / totalMass);

                // Przesuń kule wzdłuż wektora normalnego
                _dataApi.UpdateBall(ballA,
                    (float)(ballA.Position[0] + nx * moveA),
                    (float)(ballA.Position[1] + ny * moveA),
                    null, null);

                _dataApi.UpdateBall(ballB,
                    (float)(ballB.Position[0] - nx * moveB),
                    (float)(ballB.Position[1] - ny * moveB),
                    null, null);
            }

            // Zaktualizuj prędkości
            _dataApi.UpdateBall(ballA, ballA.Position[0], ballA.Position[1], (float)v1xAfter, (float)v1yAfter);
            _dataApi.UpdateBall(ballB, ballB.Position[0], ballB.Position[1], (float)v2xAfter, (float)v2yAfter);

            Console.WriteLine("Nowa predkosc kuli A: " + v1xAfter + " " + v1yAfter);
            Console.WriteLine("Nowa predkosc kuli B: " + v2xAfter + " " + v2yAfter);
        }


    }
}