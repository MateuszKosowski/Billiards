using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    interface IPoolTable
    {
        float Width { get; }
        float Height { get; }

        // Przekazujemy obiekt jakieś klasy, która implementuje IBall, czyli np Ball
        void AddBall(IBall ball);

        void DeleteBall(IBall ball);

        IEnumerable<IBall> GetAllBalls();

    }
}
