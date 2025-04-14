using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

        public IBall CreateBall(string color, int radius, int number, float vx, float vy)
        {
            float xTable = _poolTable.Width;
            float yTable = _poolTable.Height;

            // Generuj losową pozycję w granicach stołu, uwzględniając promień [source: 8, source: 6]
            float x = (float)_random.NextDouble() * (xTable - 2 * radius) + radius;
            float y = (float)_random.NextDouble() * (yTable - 2 * radius) + radius;

            Ball ball = new Ball(radius, color, number, x, y, vx, vy);
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
            return new Vector2(_poolTable.Width,  _poolTable.Height);
        }
    }
}
