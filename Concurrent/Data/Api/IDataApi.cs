using Data.Entities;
using System.Numerics;

namespace Data.Api
{
    public interface IDataApi
    {
        IBall CreateBall(string color, int radius, int number, int weight = 1);

        void UpdateBall(IBall ball, float x, float y, float? vx, float? vy);

        void CreatePoolTable(float width, float height);

        void AddBallToTable(IBall ball);

        void DeleteBallFromTable(IBall ball);

        Vector2 GetTableSize();

        IEnumerable<IBall> GetAllBallsFromTable();

        IPoolTable GetPoolTableInstance();
    }
}
