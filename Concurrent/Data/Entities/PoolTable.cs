namespace Data.Entities
{
    public class PoolTable : IPoolTable
    {
        private float _width { get; init; }
        private float _height { get; init; }
        private List<Ball> _balls { get; set; }

        // Zamek do synchronizacji dostępu do listy kul
        private readonly object _listLock = new object();

        public PoolTable(float width, float height)
        {
            _width = width;
            _height = height;
            _balls = new List<Ball>();
        }

        public float Width
        {
            get { return _width; }
        }

        public float Height
        {
            get { return _height; }
        }

        public void AddBall(IBall ball)
        {
            lock (_listLock)
            {
                _balls.Add((Ball)ball);
            }
        }

        public void DeleteBall(IBall ball)
        {
            lock (_listLock)
            {
                _balls.Remove((Ball)ball);
            }
        }

        // Widok tylko do odczytu, można się iterować
        public IEnumerable<IBall> GetAllBalls()
        {
            lock (_listLock) // <- Zablokuj dostęp do listy
            {
                // Zwróć KOPIĘ listy, aby wątek wywołujący
                // nie trzymał blokady podczas iteracji.
                return _balls.ToList();
            } // <- Zwolnij blokadę
        }
    }
}
