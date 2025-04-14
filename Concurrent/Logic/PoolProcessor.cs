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
        private List<String> _allColors = new List<String> { "Red", "Blue", "Green", "Pink", "Brown", "Yellow", "Orange"};




        public class BallsCollisionEventArgs : EventArgs
        {
            public Ball Ball1 { get; set; }
            public Ball Ball2 { get; set; }
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

            foreach (var ball in _dataApi.GetAllBallsFromTable())
            {
                double newX = (double)ball.Position[0];
                newX += ball.Velocity[0] * timeDelta;
                double newY = (double)ball.Position[1];
                newY += ball.Velocity[1] * timeDelta;

                _dataApi.UpdateBall(ball, (float)newX, (float)newY, null, null);

                HandleWallCollision(ball);

                BallMoving?.Invoke(this, ball);

                //IsAnotherBallColliding(ball);
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
                float newVY = ball.Velocity[0];
                newVY = -newVY;

                double newY = (double)ball.Position[1];
                newY = _dataApi.GetTableSize()[1] - ball.Radius - 0.01;

                _dataApi.UpdateBall(ball, ball.Position[0], (float)newY, null, newVY);

                Console.WriteLine($"Kula {ball.Color} odbiła się od dolnej ściany");
                isCollision = true;
            }

            else if (ball.Position[1] - ball.Radius <= 0)
            {
                float newVY = ball.Velocity[0];
                newVY = -newVY;

                double newY = (double)ball.Position[1];
                newY = ball.Radius + 0.01;

                _dataApi.UpdateBall(ball, ball.Position[0], (float)newY, null, newVY);

                Console.WriteLine($"Kula {ball.Color} odbiła się od górnej ściany");
                return true;
            }

            return isCollision;
        }

        public void AddBalls(int _amount)
        {
           for(int i = 0; i < _amount; i++)
            {
                IBall ball = _dataApi.CreateBall(_allColors[i], 1, i + 1);
                _dataApi.AddBallToTable(ball);
            }
        }

        public void ClearTable()
        {
            foreach (var ball in _dataApi.GetAllBallsFromTable())
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

        //public bool IsAnotherBallColliding(Ball ball)
        //{
        //    foreach (Ball otherBall in _poolTable.Balls)
        //    {
        //        // Nie prowadź testu kolizji z samą sobą
        //        if (otherBall == ball) continue;

        //        // obliczenie odległości między kulami
        //        double dx = ball.PositionX - otherBall.PositionX;
        //        double dy = ball.PositionY - otherBall.PositionY;
        //        double distance = Math.Sqrt(dx * dx + dy * dy);

        //        // jeśli dystans jest mniejszy niż suma promieni kul, to znaczy, że kule się zderzają
        //        if (distance < (ball.Radius + otherBall.Radius))
        //        {
        //            Console.WriteLine($"Kula {ball.Color} zderzyła się z kulą {otherBall.Color}");

        //            BallsCollision?.Invoke(this, new BallsCollisionEventArgs
        //            {
        //                Ball1 = ball,
        //                Ball2 = otherBall,
        //                CollisionTime = DateTime.Now
        //            });


        //            ResolveCollision(ball, otherBall, dx, dy, distance);

        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //public void ResolveCollision(Ball ballA, Ball ballB, float distanceX, float distanceY, float distance)
        //{     
        //    // Normalizacja wektora
        //    distanceX /= distance;
        //    distanceY /= distance;

        //    // Wektor styczny (prostopadły do normalnego)
        //    float tangentX = -distanceY;
        //    float tangentY = -distanceX;

        //    // Rzutowanie prędkości na osie normalnej i stycznej
        //    float velocityANormal = ballA.VelocityX * distanceX + ballA.VelocityY * distanceY;
        //    float velocityATangent = ballA.VelocityX * tangentX + ballA.VelocityY * tangentY;
        //    float velocityBNormal = ballB.VelocityX * distanceX + ballB.VelocityY * tangentY;
        //    float velocityBTangent = ballB.VelocityX * tangentX + ballB.VelocityY * tangentY;

        //    // Zmiana składowych normalnych
        //    float velocityANormalNew = velocityBNormal;
        //    float velocityBNormalNew = velocityANormal;

        //    Console.WriteLine("Predkosc kuli A: " + ballA.VelocityX + " " + ballA.VelocityY);
        //    Console.WriteLine("Predkosc kuli B: " + ballB.VelocityX + " " + ballB.VelocityY);

        //    // Trnsformacja na układ globaly
        //    ballA.VelocityX = velocityANormalNew * distanceX + velocityATangent * tangentX;
        //    ballA.VelocityY = velocityANormalNew * distanceY + velocityATangent * tangentY;
        //    ballB.VelocityX = velocityBNormalNew * distanceX + velocityBTangent * tangentX;
        //    ballB.VelocityY = velocityBNormalNew * distanceY + velocityBTangent * tangentY;

        //    Console.WriteLine("Nowa predkosc kuli A: " + ballA.VelocityX + " " + ballA.VelocityY);
        //    Console.WriteLine("Nowa predkosc kuli B: " + ballB.VelocityX + " " + ballB.VelocityY);


        //}


    }
}