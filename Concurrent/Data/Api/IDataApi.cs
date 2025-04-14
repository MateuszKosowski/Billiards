using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Api
{
    interface IDataApi
    {
        IBall CreateBall(string color, int radius, int number, float vx, float vy, IPoolTable table);

        void UpdateBall(IBall ball, float? x, float? y, float? vx, float? vy);

        IPoolTable CreatePoolTable(float width, float height);

        void AddBallToTable(IBall ball, IPoolTable table);

        void DeleteBallFromTable(IBall ball, IPoolTable table);

        IEnumerable<IBall> GetAllBallsFromTable(IPoolTable table);
    }
}
