using System.Numerics;

namespace Data.Entities
{
    public interface IBall
    {
        // Właściwości kuli
        string Color { get; }
        int Radius { get; }
        int Number { get; }
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
    }
}
