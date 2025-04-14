using Data.Entities;

namespace Logic
{
    interface IPoolProcessor
    {
        void Start();

        void Stop();

        void AddBalls(int _amount);

        void ClearTable();

        void CreateTable(float _width, float _height);

        IEnumerable<IBall> GetAllBallsFromTable();
    }
}
