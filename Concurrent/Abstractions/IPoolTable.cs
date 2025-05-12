namespace Abstractions
{
    public interface IPoolTable
    {
        float Width { get; }
        float Height { get; }

        // Przekazujemy obiekt jakieś klasy, która implementuje IBall, czyli np Ball
        void AddBall(IBall ball);

        void DeleteBall(IBall ball);

        IEnumerable<IBall> GetAllBalls();

    }
}
