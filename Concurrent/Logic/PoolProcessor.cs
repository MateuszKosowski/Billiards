using Data.Api;
using Data.Entities;
using System.Diagnostics;
using System.Timers;

namespace Logic
{
    public class PoolProcessor : IPoolProcessor
    {
        private readonly IDataApi _dataApi;

        public event EventHandler<IBall> BallMoving;
        private List<String> _allColors = new List<String> { "Red", "Blue", "Lime", "Pink", "Brown", "Yellow", "Orange", "Purple", "Gold", "Green", "Black", "White"};

        // Konstruktor
        public PoolProcessor()
        {
            _dataApi = new DataApi();
        }

        // Rozpoczęcie symulacji
        public void Start()
        {
            IPoolTable table = _dataApi.GetPoolTableInstance(); // Pobierz instancję stołu

            var balls = _dataApi.GetAllBallsFromTable();
            foreach (var ball in balls)
            {
                ball.StartUpdating(table);
            }

        }

        public void Stop()
        {
            var balls = _dataApi.GetAllBallsFromTable(); // Pobierz kopię listy
            foreach (var ball in balls)
            {
                ball.StopUpdating(); // Zatrzymaj aktualizację dla każdej kuli
            }
        }

        public void AddBalls(int _amount)
        {
           for(int i = 0; i < _amount; i++)
            {
                IBall ball = _dataApi.CreateBall(_allColors[i], 20, i + 1);
                _dataApi.AddBallToTable(ball);
                ball.BallMoved += HandleBallMoved;
            }
        }

        private void HandleBallMoved(object? sender, BallMovedEventArgs e)
        {
            if (sender is IBall ball)
            {
                // Przekaż zdarzenie dalej do warstwy wyższej (ViewModel)
                BallMoving?.Invoke(this, ball);
            }
        }

        public void ClearTable()
        {
            var balls = _dataApi.GetAllBallsFromTable().ToList();
            foreach (var ball in balls)
            {
                ball.StopUpdating(); // Zatrzymaj timer przed usunięciem
                _dataApi.DeleteBallFromTable(ball);
                ball.BallMoved -= HandleBallMoved; // Odsubskrybuj zdarzenie
            }
        }

        public IEnumerable<IBall> GetAllBallsFromTable()
        {
            return _dataApi.GetAllBallsFromTable();
        }

        public void CreateTable(float _width, float _height)
        {
            _dataApi.CreatePoolTable(_width, _height);
        }
    }
}