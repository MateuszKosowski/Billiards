using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Presentation.Model;
using Data.Entities;

namespace Presentation.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IPoolProcessor _poolProcessor;
        private ICommand _startSimulationCommand;
        private ICommand _stopSimulationCommand;
        private string _ballCountInput;
        private readonly PoolTableModel _poolTableModel;

        public ObservableCollection<BallModel> Balls => _poolTableModel.Balls;

        // Zdarzenie wywoływane gdy właściwość jakiego obiektu WPF się zmieni
        public event PropertyChangedEventHandler? PropertyChanged;

        // Wywołanie zdarzenia, argument - nazwa zmienionej właściowści. Dzięki temu UI sie odświeży
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        public MainWindowViewModel()
        {
            _poolProcessor = new PoolProcessor();
            _poolProcessor.CreateTable(1000, 600); // Ustawienie wymiarów stołu bilardowego
            _poolTableModel = new PoolTableModel();
            // Gotowa implementacja: Jak guzik będzie kliknięty to wykonaj funkcję
            _startSimulationCommand = new RelayCommand(StartSimulation);
            _stopSimulationCommand = new RelayCommand(StopSimulation);
            BallCountInput = "1";
            ErrorMessage = string.Empty;
            _poolProcessor.BallMoving += OnBallMoving;
        }

        public ICommand StartSimulationCommand
        {
            get { return _startSimulationCommand; }
        }

        public ICommand StopSimulationCommand
        {
            get { return _stopSimulationCommand; }
        }

        private void StartSimulation()
        {
            if (int.TryParse(BallCountInput, out int ballCount) && ballCount >= 1 && ballCount <= 7)
            {
                ErrorMessage = string.Empty;
                Balls.Clear();
                _poolProcessor.ClearTable();
                _poolProcessor.AddBalls(ballCount);
                var balls = _poolProcessor.GetAllBallsFromTable().ToList();
                foreach (var ball in balls)
                {
                    Balls.Add(new BallModel
                    {
                        X = ball.Position[0],
                        Y = ball.Position[1],
                        Color = ball.Color,
                        Number = ball.Number,
                        Radius = ball.Radius
                    });
                }
                _poolProcessor.Start();
            } else
            {
                ErrorMessage = "Liczba kul musi być liczbą całkowitą z zakresu 1-7.";
            }
        }

        private void StopSimulation()
        {
            _poolProcessor.Stop();
        }

        public string BallCountInput
        {
            get { return _ballCountInput; }
            set
            {
                if (_ballCountInput != value)
                {
                    _ballCountInput = value;
                    OnPropertyChanged(nameof(BallCountInput));
                }
            }
        }

        private void OnBallMoving(object sender, IBall ball)
        {
            // Aby wszystko działało poprawnie, musimy użyć Dispatcher, 
            // ponieważ timer działa w innym wątku niż UI
            App.Current.Dispatcher.Invoke(() =>
            {
                // Znajdź odpowiadający model kuli
                var ballModel = Balls.FirstOrDefault(b => b.Number == ball.Number);
                if (ballModel != null)
                {
                    // Aktualizuj położenie
                    ballModel.X = ball.Position[0];
                    ballModel.Y = ball.Position[1];
                }
            });
        }
    }
}
