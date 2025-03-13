using System.Runtime.CompilerServices;

namespace Data
{
    public class Ball
    {
        // Pola prywatne z właściwościami tylko do odczytu (init)
        private int _radius { get; init; }
        private string _color { get; init; }
        private int _number { get; init; }

        // Pola prywatne z możliwością modyfikacji
        private double _positionX { get; set; }
        private double _positionY { get; set; }
        private double _velocityX { get; set; }
        private double _velocityY { get; set; }

        // Publiczne właściwości z getterami do pól init-only
        public int Radius => _radius;
        public string Color => _color;
        public int Number => _number;

        // Publiczne właściwości z getterami i setterami
        public double PositionX
        {
            get => _positionX;
            set => _positionX = value;
        }

        public double PositionY
        {
            get => _positionY;
            set => _positionY = value;
        }

        public double VelocityX
        {
            get => _velocityX;
            set => _velocityX = value;
        }

        public double VelocityY
        {
            get => _velocityY;
            set => _velocityY = value;
        }


        public Ball(int radius, string color, int number, double positionX, double positionY, double velocityX, double velocityY)
        {
            _radius = radius;
            _color = color;
            _number = number;
            _positionX = positionX;
            _positionY = positionY;
            _velocityX = velocityX;
            _velocityY = velocityY;
        }

    }
}
