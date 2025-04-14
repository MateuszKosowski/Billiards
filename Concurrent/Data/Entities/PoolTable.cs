using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class PoolTable : IPoolTable
    {
        private float _width { get; init; }
        private float _height { get; init; }
        private List<Ball> _balls { get; set; }

        public PoolTable(float width, float height)
        {
            _width = width;
            _height = height;
            _balls = new List<Ball>();
        }

        public float Width
        {
            get { return _width; }
        }

        public float Height
        {
            get { return _height; }
        }

        public void AddBall(IBall ball)
        {
            _balls.Add((Ball)ball);
        }

        public void DeleteBall(IBall ball)
        {
            _balls.Remove((Ball)ball);
        }

        // Widok tylko do odczytu, można się iterować
        public IEnumerable<IBall> GetAllBalls()
        {
            return _balls;
        }
    }
}
