using System.Numerics;

namespace Abstractions
{
    public interface IBall
    {
        // Właściwości kuli
        string Color { get; }
        int Radius { get; }
        int Number { get; }
        int Weight { get; }
        Vector2 Position { get; }
        Vector2 Velocity { get; }
        void UpdateState(float? posX = null, float? posY = null, float? velX = null, float? velY = null);
    }
}
