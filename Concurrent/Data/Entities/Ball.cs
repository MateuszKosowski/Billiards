using System.Diagnostics;
using System.Numerics;

namespace Data.Entities
{
    public class BallMovedEventArgs : EventArgs
    {
        public Vector2 NewPosition { get; }
        public BallMovedEventArgs(Vector2 newPosition) { NewPosition = newPosition; }
    }


    public class Ball : IBall
    {
        // Blokada do synchronizacji dostępu do pól
        private readonly object _stateLock = new object(); // <- Zamek dla stanu kuli
        private System.Threading.Timer _updateTimer; // <- Timer do aktualizacji pozycji kuli
        private Stopwatch _stopwatch = new Stopwatch(); // <- Zegar do pomiaru czasu
        private IPoolTable _tableContext; // <- Kontekst stołu bilardowego
        public event EventHandler<BallMovedEventArgs>? BallMoved; // Zdarzenie informujące o zmianie pozycji kuli


        // Pola prywatne z właściwościami tylko do odczytu (init)
        private int _radius { get; init; }
        private string _color { get; init; }
        private int _number { get; init; }
        private int _weight { get; init; } = 1; // Waga kuli, domyślnie 1

        // Pola prywatne z możliwością modyfikacji
        private float _positionX { get; set; }
        private float _positionY { get; set; }
        private float _velocityX { get; set; }
        private float _velocityY { get; set; }

        // Publiczne właściwości z getterami do pól init-only
        public int Radius => _radius;
        public string Color => _color;
        public int Number => _number;
        public int Weight => _weight;

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
            get
            {
                lock (_stateLock) // <- Zablokuj odczyt stanu
                {
                    return new Vector2(_positionX, _positionY);
                } // <- Zwolnij blokadę
            }
            set
            {
                lock (_stateLock) // <- Zablokuj zapis stanu
                {
                    _positionX = value.X;
                    _positionY = value.Y;
                } // <- Zwolnij blokadę
            }
        }

        public Vector2 Velocity
        {
            get
            {
                lock (_stateLock) // <- Zablokuj odczyt stanu
                {
                    return new Vector2(_velocityX, _velocityY);
                } // <- Zwolnij blokadę
            }
            set
            {
                lock (_stateLock) // <- Zablokuj zapis stanu
                {
                    _velocityX = value.X;
                    _velocityY = value.Y;
                } // <- Zwolnij blokadę
            }
        }


        // Konstruktor
        public Ball(int radius, string color, int number, float positionX, float positionY, float velocityX, float velocityY, int weight = 1)
        {
            _radius = radius;
            _color = color;
            _number = number;
            _positionX = positionX;
            _positionY = positionY;
            _velocityX = velocityX;
            _velocityY = velocityY;
            _weight = weight;
        }

        public void StartUpdating(IPoolTable tableContext)
        {
            _tableContext = tableContext ?? throw new ArgumentNullException(nameof(tableContext));
            // Używamy System.Threading.Timer, który używa wątków z puli.
            // interwał  10 ms. State = null, DueTime = 0 (start natychmiast), Period = 10.
            _updateTimer = new System.Threading.Timer(UpdateSelf, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            _stopwatch.Start();
        }

        public void StopUpdating()
        {
            _updateTimer?.Dispose(); // Zatrzymaj i zwolnij zasoby timera
            _updateTimer = null;
            _stopwatch.Stop();
        }

        private void UpdateSelf(object? state)
        {
            if (_tableContext == null || _updateTimer == null) return; // Jeśli zatrzymany lub nie zainicjalizowany

            double timeDelta = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            // --- Logika aktualizacji pozycji (przeniesiona z PoolProcessor) ---
            Vector2 currentPosition;
            Vector2 currentVelocity;

            lock (_stateLock) // Użyj zamka z poprzedniego kroku
            {
                currentPosition = new Vector2(_positionX, _positionY);
                currentVelocity = new Vector2(_velocityX, _velocityY);
            }

            double newX = currentPosition.X + currentVelocity.X * timeDelta;
            double newY = currentPosition.Y + currentVelocity.Y * timeDelta;

            // --- Logika kolizji ze ścianami (przeniesiona z PoolProcessor.IsWallCollision) ---
            // WAŻNE: Musi modyfikować LOKALNE ZMIENNE newX, newY i currentVelocity
            // Zamiast _dataApi.UpdateBall, aktualizujemy currentVelocity
            bool collisionDetected = false;
            Vector2 tableSize = new Vector2(_tableContext.Width, _tableContext.Height); // Pobierz wymiary

            // Zderzenia X
            if (newX + Radius >= tableSize.X)
            {
                currentVelocity.X = -currentVelocity.X;
                newX = tableSize.X - Radius - 0.01;
                collisionDetected = true;
            }
            else if (newX - Radius <= 0)
            {
                currentVelocity.X = -currentVelocity.X;
                newX = Radius + 0.01;
                collisionDetected = true;
            }

            // Zderzenia Y
            if (newY + Radius >= tableSize.Y)
            {
                currentVelocity.Y = -currentVelocity.Y;
                newY = tableSize.Y - Radius - 0.01;
                collisionDetected = true;
            }
            else if (newY - Radius <= 0)
            {
                currentVelocity.Y = -currentVelocity.Y;
                newY = Radius + 0.01;
                collisionDetected = true;
            }

            // TODO: Później dodać logikę kolizji między kulami

            // --- Aktualizacja stanu kuli (bezpieczna wątkowo) ---
            lock (_stateLock)
            {
                _positionX = (float)newX;
                _positionY = (float)newY;
                _velocityX = currentVelocity.X;
                _velocityY = currentVelocity.Y;
            }

            // --- Powiadomienie o zmianie pozycji ---
            // Przekazujemy aktualną pozycję po wszystkich obliczeniach
            BallMoved?.Invoke(this, new BallMovedEventArgs(new Vector2((float)newX, (float)newY)));

            // Można też dodać event dla kolizji ze ścianą, jeśli potrzebny
            // if (collisionDetected) { WallsCollision?.Invoke(...) }
        }

    }
}
