using Data.Api;
using Data.Entities;
using System.Diagnostics;
using System.Timers;
using System.Collections.Concurrent;
using Abstractions;

namespace Logic
{
    public class PoolProcessor : IPoolProcessor, ICollisionService
    {
        private readonly IDataApi _dataApi;
        private readonly object _lock = new();
        private readonly List<Ball> _balls = new();
        private bool _isRunning = false;
        private readonly BufferedBilliardLogger _logger;

        public event EventHandler<BallsCollisionEventArgs> BallsCollision;
        public event EventHandler<WallsCollisionEventArgs> WallsCollision;
        public event EventHandler<IBall> BallMoving;
        private List<string> _allColors = new List<string> { "Red", "Blue", "Lime", "Pink", "Brown", "Yellow", "Orange", "Purple", "Gold", "Green", "Black", "White" };

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

        public PoolProcessor()
        {
            _dataApi = new DataApi();
            _logger = new BufferedBilliardLogger($"logs/billiards-{DateTime.Now:yyyy-MM-dd}.txt");
        }

        ~PoolProcessor()
        {
            _logger?.Dispose();
        }

        public void AddBalls(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Ball ball = (Ball)_dataApi.CreateBall(_allColors[i], 20, i + 1, 1);
                ball = new Ball(ball.Radius, ball.Color, ball.Number, ball.Position[0], ball.Position[1], ball.Velocity[0], ball.Velocity[1], this);
                _balls.Add(ball);
                _dataApi.AddBallToTable(ball);
                RegisterBall(ball);
            }
        }

        public void ClearTable()
        {
            lock (_lock)
            {
                foreach (var ball in _balls)
                {
                    ball.Stop();
                }
                _balls.Clear();
            }
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

        public void CreateTable(float width, float height)
        {
            _dataApi.CreatePoolTable(width, height);
        }

        public void Start()
        {
            lock (_lock)
            {
                _isRunning = true;
                _logger.LogSimulationStart(_balls.Count);
                foreach (var ball in _balls)
                {
                    ball.Start();
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _isRunning = false;
                _logger.LogSimulationStop();
                foreach (var ball in _balls)
                {
                    ball.Stop();
                }
            }
        }

        public void RegisterBall(IBall ball)
        {
            // Możesz dodać dodatkową logikę rejestracji jeśli potrzeba
        }

        public void UnregisterBall(IBall ball)
        {
            lock (_lock)
            {
                if (ball is Ball concreteBall)
                {
                    concreteBall.Stop();
                }
                _balls.Remove(ball as Ball);
            }
        }

        // Dodatkowa metoda do sprawdzenia statusu bufora
        public int GetLogBufferSize()
        {
            return _logger.BufferSize;
        }

        // Metoda do wymuszenia zapisu bufora (przydatne do debugowania)
        public void FlushLogs()
        {
            _logger.ForceFlush();
        }

        public void ReportPosition(IBall moving)
        {
            if (!_isRunning) return;

            _logger.LogBallPosition(moving);

            // Najpierw bezpiecznie zaktualizuj stan kuli (pozycja i prędkość)
            moving.UpdateState(moving.Position.X, moving.Position.Y, moving.Velocity.X, moving.Velocity.Y);

            // Następnie zaktualizuj warstwę danych
            _dataApi.UpdateBall(moving, moving.Position.X, moving.Position.Y, moving.Velocity.X, moving.Velocity.Y);

            // Obsługa kolizji ze ścianami
            HandleWallCollision(moving);

            // Obsługa kolizji z innymi kulami
            IsAnotherBallColliding(moving);

            // Powiadom o ruchu kuli (do UI)
            BallMoving?.Invoke(this, moving);
        }

        public void HandleWallCollision(IBall ball)
        {
            if (IsWallCollision(ball))
            {

                var collisionTime = DateTime.Now;
                _logger.LogWallCollision(ball, collisionTime);

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
            var tableSize = _dataApi.GetTableSize();

            // Zderzenia na X
            if (ball.Position[0] + ball.Radius >= tableSize[0])
            {
                float newVX = -ball.Velocity[0];
                float newX = tableSize[0] - ball.Radius - 0.01f;

                ball.UpdateState(newX, ball.Position[1], newVX, null);
                _dataApi.UpdateBall(ball, newX, ball.Position[1], newVX, null);

                isCollision = true;
            }
            else if (ball.Position[0] - ball.Radius <= 0)
            {
                float newVX = -ball.Velocity[0];
                float newX = ball.Radius + 0.01f;

                ball.UpdateState(newX, ball.Position[1], newVX, null);
                _dataApi.UpdateBall(ball, newX, ball.Position[1], newVX, null);

                isCollision = true;
            }

            // Zderzenia na Y
            if (ball.Position[1] + ball.Radius >= tableSize[1])
            {
                float newVY = -ball.Velocity[1];
                float newY = tableSize[1] - ball.Radius - 0.01f;

                ball.UpdateState(ball.Position[0], newY, null, newVY);
                _dataApi.UpdateBall(ball, ball.Position[0], newY, null, newVY);

                isCollision = true;
            }
            else if (ball.Position[1] - ball.Radius <= 0)
            {
                float newVY = -ball.Velocity[1];
                float newY = ball.Radius + 0.01f;

                ball.UpdateState(ball.Position[0], newY, null, newVY);
                _dataApi.UpdateBall(ball, ball.Position[0], newY, null, newVY);

                isCollision = true;
            }

            return isCollision;
        }

        public bool IsAnotherBallColliding(IBall ball)
        {
            foreach (IBall otherBall in _dataApi.GetAllBallsFromTable())
            {
                if (otherBall == ball) continue;

                double dx = ball.Position[0] - otherBall.Position[0];
                double dy = ball.Position[1] - otherBall.Position[1];
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < (ball.Radius + otherBall.Radius))
                {

                    var collisionTime = DateTime.Now;
                    _logger.LogBallCollision(ball, otherBall, collisionTime);

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
            double nx = distanceX / distance;
            double ny = distanceY / distance;

            double m1 = ballA.Weight;
            double m2 = ballB.Weight;

            double v1x = ballA.Velocity[0];
            double v1y = ballA.Velocity[1];
            double v2x = ballB.Velocity[0];
            double v2y = ballB.Velocity[1];

            double v1n = v1x * nx + v1y * ny;
            double v2n = v2x * nx + v2y * ny;

            double v1t = -v1x * ny + v1y * nx;
            double v2t = -v2x * ny + v2y * nx;

            double v1nAfter = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
            double v2nAfter = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

            double v1xAfter = v1nAfter * nx - v1t * ny;
            double v1yAfter = v1nAfter * ny + v1t * nx;
            double v2xAfter = v2nAfter * nx - v2t * ny;
            double v2yAfter = v2nAfter * ny + v2t * nx;

            double overlap = (ballA.Radius + ballB.Radius) - distance;
            if (overlap > 0)
            {
                double totalMass = m1 + m2;
                double moveA = overlap * (m2 / totalMass);
                double moveB = overlap * (m1 / totalMass);

                float newAX = (float)(ballA.Position[0] + nx * moveA);
                float newAY = (float)(ballA.Position[1] + ny * moveA);
                float newBX = (float)(ballB.Position[0] - nx * moveB);
                float newBY = (float)(ballB.Position[1] - ny * moveB);

                ballA.UpdateState(newAX, newAY, null, null);
                ballB.UpdateState(newBX, newBY, null, null);

                _dataApi.UpdateBall(ballA, newAX, newAY, null, null);
                _dataApi.UpdateBall(ballB, newBX, newBY, null, null);
            }

            ballA.UpdateState(null, null, (float)v1xAfter, (float)v1yAfter);
            ballB.UpdateState(null, null, (float)v2xAfter, (float)v2yAfter);

            _dataApi.UpdateBall(ballA, ballA.Position[0], ballA.Position[1], (float)v1xAfter, (float)v1yAfter);
            _dataApi.UpdateBall(ballB, ballB.Position[0], ballB.Position[1], (float)v2xAfter, (float)v2yAfter);
        }
    }
}
