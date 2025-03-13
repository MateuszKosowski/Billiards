using Data;
using System.Diagnostics;
using System.Timers;

namespace Logic
{
    public class PoolProcessor
    {
        private Data.PoolTable _poolTable { get; init; }
        private Stopwatch stopwatch = new Stopwatch();
        private readonly System.Timers.Timer timer;

        public PoolProcessor(Data.PoolTable poolTable)
        {
            _poolTable = poolTable;
            timer = new System.Timers.Timer(10);
            timer.Elapsed += Update;
            timer.AutoReset = true;
        }

        public void AddBall(Ball ball)
        {
            _poolTable.Balls.Add(ball);
        }

        public void Start()
        {
            timer.Start();
        }

        public void Update(object sender, ElapsedEventArgs e)
        {
            double timeDelta = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            foreach (var ball in _poolTable.Balls)
            {
                ball.PositionX += ball.VelocityX * timeDelta;
                ball.PositionY += ball.VelocityY * timeDelta;
            }

            foreach (var ball in _poolTable.Balls)
            {
                Console.WriteLine($"Kula w pozycji ({ball.PositionX:F2}, {ball.PositionY:F2})");
            }
        }

        static void Main(string[] args)
        {
            PoolProcessor proc = new PoolProcessor(new PoolTable(100, 100));

            // Dodaj dwie kule z różnymi prędkościami
            proc.AddBall(new Ball(1, "red", 2, 0, 0, 1.5f, 2));
            proc.AddBall(new Ball { PositionX = 10, PositionY = 10, VelocityX = -1, VelocityY = -0.5 });

            // Uruchom symulację
            proc.Start();

            // Utrzymaj program aktywny, aby zobaczyć wyniki
            Console.ReadLine();
        }
    }
}