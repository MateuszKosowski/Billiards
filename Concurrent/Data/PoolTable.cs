using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class PoolTable
    {
        private float _width { get; init; }
        private float _height { get; init; }
        private List<Ball> _balls;

        public PoolTable(float width, float height)
        {
            _width = width;
            _height = height;
        }

        public List<Ball> Balls
        {
            get { return _balls; }
            set { _balls = value; }
        }
    }
}
