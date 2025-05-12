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
        internal readonly object _stateLock = new object(); // <- Zamek dla stanu kuli
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

            // --- Logika kolizja z innymi kulami
            var otherBalls = _tableContext.GetAllBalls(); // Pobiera bezpieczną kopię listy

            foreach (var otherBallRef in otherBalls)
            {
                // Sprawdź, czy to nie ta sama kula
                if (otherBallRef.Number == this.Number) continue;

                // Bezpiecznie pobierz pozycję drugiej kuli
                Vector2 otherPosition = otherBallRef.Position; // Używa lock wewnątrz gettera Position

                // Oblicz dystans
                double dx = newX - otherPosition.X;
                double dy = newY - otherPosition.Y;
                double distanceSquared = dx * dx + dy * dy; // Lepiej porównywać kwadraty
                double radiiSum = this.Radius + otherBallRef.Radius;
                double radiiSumSquared = radiiSum * radiiSum;

                // Jeśli dystans jest mniejszy niż suma promieni
                if (distanceSquared < radiiSumSquared)
                {

                    // --- Implementacja ResolveCollision z blokowaniem ---
                    IBall ball1 = this;
                    IBall ball2 = otherBallRef;

                    // Lolejność blokowania, aby uniknąć zakleszczenia (deadlock)
                    // Zawsze blokujemy najpierw kulę z mniejszym numerem.
                    object lock1 = ball1.Number < ball2.Number ? ((Ball)ball1)._stateLock : ((Ball)ball2)._stateLock;
                    object lock2 = ball1.Number < ball2.Number ? ((Ball)ball2)._stateLock : ((Ball)ball1)._stateLock;

                    double currentDistance = 0;
                    double nx = 0;
                    double ny = 0;

                    lock (lock1)
                    {
                        lock (lock2)
                        {
                            // ---- Sekcja Krytyczna: Mamy blokadę na OBU kulach ----
                            // Teraz możemy bezpiecznie odczytać i zmodyfikować stan obu kul.

                          
                            Vector2 v1 = ball1.Velocity; // Używa gettera, który ma lock
                            Vector2 v2 = ball2.Velocity;
                            Vector2 pos1 = ball1.Position; 
                            Vector2 pos2 = ball2.Position;

                            // Wektor normalny kolizji (od środka ball1 do środka ball2)
                           
                            dx = pos2.X - pos1.X;
                            dy = pos2.Y - pos1.Y;
                            currentDistance = Math.Sqrt(dx * dx + dy * dy);
                      
                            if (currentDistance == 0) continue;

                            nx = dx / currentDistance;
                            ny = dy / currentDistance;

                            // Masa kul
                            double m1 = ball1.Weight;
                            double m2 = ball2.Weight;

                            // Składowe normalne prędkości
                            double v1n = v1.X * nx + v1.Y * ny;
                            double v2n = v2.X * nx + v2.Y * ny;

                            // Składowe styczne prędkości
                            double v1t = -v1.X * ny + v1.Y * nx;
                            double v2t = -v2.X * ny + v2.Y * nx;

                            // Nowe składowe normalne po zderzeniu sprężystym
                            double v1nAfter = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
                            double v2nAfter = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

                            // Nowe prędkości w układzie x, y
                            float v1xAfter = (float)(v1nAfter * nx - v1t * ny);
                            float v1yAfter = (float)(v1nAfter * ny + v1t * nx);
                            float v2xAfter = (float)(v2nAfter * nx - v2t * ny);
                            float v2yAfter = (float)(v2nAfter * ny + v2t * nx);

                            // Ustaw nowe prędkości (używa settera, który ma lock)
                            ball1.Velocity = new Vector2(v1xAfter, v1yAfter);
                            ball2.Velocity = new Vector2(v2xAfter, v2yAfter);

                            // ---- Koniec Sekcji Krytycznej ----

                        } // Zwolnienie blokady lock2
                    } // Zwolnienie blokady lock1


                    // --- Rozdzielenie kul (zapobieganie zakleszczeniu) ---

                    double overlap = (ball1.Radius + ball2.Radius) - currentDistance;
                    if (overlap > 0.01) // Mały margines błędu
                    {
                        // Proste rozdzielenie wzdłuż linii kolizji
                        double moveFactor = overlap / 2.0; // Rozdziel równo
                        Vector2 moveVector = new Vector2((float)(nx * moveFactor), (float)(ny * moveFactor));

                        // Aktualizacja pozycji wymaga osobnych blokad, co jest OK, bo robimy to sekwencyjnie
                        ball1.Position -= moveVector;
                        ball2.Position += moveVector;

                        // Po rozdzieleniu, nowa pozycja tej kuli (this/ball1) mogła się zmienić,
                        // więc zaktualizujmy newX, newY używane dalej w UpdateSelf
                        Vector2 correctedPos1 = ball1.Position;
                        newX = correctedPos1.X;
                        newY = correctedPos1.Y;
                    }


                    // Ważne: Ponieważ zmieniliśmy prędkość 'this' (ball1), musimy
                    // zaktualizować lokalną zmienną 'currentVelocity' używaną do
                    // dalszych obliczeń pozycji w tym kroku UpdateSelf.
                    lock (_stateLock) // Odczytaj nową prędkość bezpiecznie
                    {
                        currentVelocity = new Vector2(_velocityX, _velocityY);
                    }

                    break;

                }
            }

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

        }

    }
}
