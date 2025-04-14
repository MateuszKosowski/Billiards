using System.Numerics;

namespace Data.Entities
{
    public class Ball : IBall
    {
        // Pola prywatne z właściwościami tylko do odczytu (init)
        private int _radius { get; init; }
        private string _color { get; init; }
        private int _number { get; init; }

        // Pola prywatne z możliwością modyfikacji
        private float _positionX { get; set; }
        private float _positionY { get; set; }
        private float _velocityX { get; set; }
        private float _velocityY { get; set; }

        // Publiczne właściwości z getterami do pól init-only
        public int Radius => _radius;
        public string Color => _color;
        public int Number => _number;

        // Publiczne właściwości z getterami i setterami
        public float PositionX
        {
            get => _positionX;
            set => _positionX = value;
        }

        public float PositionY
        {
            get => _positionY;
            set => _positionY = value;
        }

        public float VelocityX
        {
            get => _velocityX;
            set => _velocityX = value;
        }

        public float VelocityY
        {
            get => _velocityY;
            set => _velocityY = value;
        }

        // Właściowości z interfejsu
        public Vector2 Position
        {
            get => new Vector2(_positionX, _positionY);
            set
            {
                _positionX = value.X;
                _positionY = value.Y;
            }
        }

        public Vector2 Velocity
        {
            get => new Vector2(_velocityX, _velocityY);
            set
            {
                _velocityX = value.X;
                _velocityY = value.Y;
            }
        }


        // Konstruktor
        public Ball(int radius, string color, int number, float positionX, float positionY, float velocityX, float velocityY)
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
