using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Data.Api
{
    class DataApi : IDataApi
    {
        private readonly Random _random = new Random(); // Do generowania pozycji

        public void AddBallToTable(IBall ball, IPoolTable table)
        {
            table.AddBall(ball);
        }

        public IBall CreateBall(string color, int radius, int number, float vx, float vy, IPoolTable table)
        {
            float xTable = table.Width;
            float yTable = table.Height;

            // Generuj losową pozycję w granicach stołu, uwzględniając promień [source: 8, source: 6]
            float x = (float)_random.NextDouble() * (xTable - 2 * radius) + radius;
            float y = (float)_random.NextDouble() * (yTable - 2 * radius) + radius;

            Ball ball = new Ball(radius, color, number, x, y, vx, vy);
            return ball;
        }

        public IPoolTable CreatePoolTable(float width, float height)
        {
            PoolTable table = new PoolTable(width, height);
            return table;
        }

        public void DeleteBallFromTable(IBall ball, IPoolTable table)
        {
            table.DeleteBall(ball);
        }

        public IEnumerable<IBall> GetAllBallsFromTable(IPoolTable table)
        {
            return table.GetAllBalls();
        }

        public void UpdateBall(IBall ball, float? x, float? y, float? vx, float? vy)
        {
            if (x != null && y != null)
            {
                ball.Position = new Vector2(x.Value, y.Value);
            }

            if (vx != null && vy != null)
            {
                ball.Velocity = new Vector2(vx.Value, vy.Value);
            }
        }
    }
}
