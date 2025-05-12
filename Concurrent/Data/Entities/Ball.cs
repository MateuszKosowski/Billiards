using Abstractions;
using System.Diagnostics;
using System.Numerics;

namespace Data.Entities
{
    public class Ball : IBall
    {
        private readonly object _lock = new();
        private readonly ICollisionService _collisionService;
        private readonly Timer _timer;
        private Stopwatch stopwatch = new Stopwatch();

        private const int IntervalMs = 16;

        private int _radius { get; init; }
        private string _color { get; init; }
        private int _number { get; init; }
        private int _weight { get; init; } = 1;

        private float _positionX { get; set; }
        private float _positionY { get; set; }
        private float _velocityX { get; set; }
        private float _velocityY { get; set; }

        public int Radius => _radius;
        public string Color => _color;
        public int Number => _number;
        public int Weight => _weight;

        public float PositionX
        {
            get => _positionX;
        }

        public float PositionY
        {
            get => _positionY;
        }

        public float VelocityX
        {
            get => _velocityX;
        }

        public float VelocityY
        {
            get => _velocityY;
        }

        public Vector2 Position
        {
            get
            {
                lock (_lock)
                {
                    return new Vector2(_positionX, _positionY);
                }
            }
        }

        public Vector2 Velocity
        {
            get
            {
                lock (_lock)
                {
                    return new Vector2(_velocityX, _velocityY);
                }
            }
        }

        public void UpdateState(float? posX = null, float? posY = null, float? velX = null, float? velY = null)
        {
            lock (_lock)
            {
                if (posX.HasValue) _positionX = posX.Value;
                if (posY.HasValue) _positionY = posY.Value;
                if (velX.HasValue) _velocityX = velX.Value;
                if (velY.HasValue) _velocityY = velY.Value;
            }
        }

        // Konstruktor do testów (bez collisionService)
        public Ball(int radius, string color, int number, float positionX, float positionY, float velocityX, float velocityY)
        {
            _radius = radius;
            _color = color;
            _number = number;
            _positionX = positionX;
            _positionY = positionY;
            _velocityX = velocityX;
            _velocityY = velocityY;
        }

        // Konstruktor do symulacji (z collisionService)
        public Ball(int radius, string color, int number, float positionX, float positionY, float velocityX, float velocityY, ICollisionService collisionService)
        {
            _radius = radius;
            _color = color;
            _number = number;
            _positionX = positionX;
            _positionY = positionY;
            _velocityX = velocityX;
            _velocityY = velocityY;
            _collisionService = collisionService;

            _timer = new Timer(_ => Tick(), null, Timeout.Infinite, IntervalMs);
        }

        public void Start()
        {
            stopwatch.Restart();
            _timer?.Change(0, IntervalMs);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            stopwatch.Stop();
        }

        private void Tick()
        {
            lock (_lock)
            {
                double timeDelta = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                _positionX += (float)(_velocityX * timeDelta);
                _positionY += (float)(_velocityY * timeDelta);
            }

            _collisionService?.ReportPosition(this);
        }
    }
}
