using Data.Entities;
using System.Numerics;

namespace Data.Api
{
    public class DataApi : IDataApi
    {
        private readonly Random _random = new Random(); // Do generowania pozycji
        private IPoolTable _poolTable;

        public void AddBallToTable(IBall ball)
        {
            _poolTable.AddBall(ball);
        }

        public IBall CreateBall(string color, int radius, int number)
        {
            Console.WriteLine("Staram sie utworzyc kule");
            float xTable = _poolTable.Width;
            float yTable = _poolTable.Height;

            // Generuj losową pozycję w granicach stołu, uwzględniając promień [source: 8, source: 6]
            float x = (float)_random.NextDouble() * (xTable - 2 * radius) + radius;
            float y = (float)_random.NextDouble() * (yTable - 2 * radius) + radius;

            // Generuj losową prędkość w granicach -1 do 1
            float vx = (float)_random.NextDouble() * 2 - 1;
            float vy = (float)_random.NextDouble() * 2 - 1;

            Ball ball = new Ball(radius, color, number, x, y, vx, vy);
            Console.WriteLine("Utworzono kule " + ball.Color + " o numerze " + ball.Number);

            return ball;
        }

        public void CreatePoolTable(float width, float height)
        {
            _poolTable = new PoolTable(width, height);
        }

        public void DeleteBallFromTable(IBall ball)
        {
            _poolTable.DeleteBall(ball);
        }

        public IEnumerable<IBall> GetAllBallsFromTable()
        {
            return _poolTable.GetAllBalls();
        }

        public void UpdateBall(IBall ball, float x, float y, float? vx, float? vy)
        {
            ball.Position = new Vector2(x, y);

            if (vx != null && vy != null)
            {
                ball.Velocity = new Vector2(vx.Value, vy.Value);
            }
        }

        public Vector2 GetTableSize()
        {
            return new Vector2(_poolTable.Width, _poolTable.Height);
        }
    }
}
