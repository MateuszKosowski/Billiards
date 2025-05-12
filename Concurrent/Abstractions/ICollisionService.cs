namespace Abstractions
{
    public interface ICollisionService
    {
        void RegisterBall(IBall ball);
        void UnregisterBall(IBall ball);
        void ReportPosition(IBall moving);
    }
}
