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

    /// <summary>
    /// ViewModel dla głównego okna aplikacji realizujący wzorzec MVVM (Model-View-ViewModel).
    /// Odpowiada za pośredniczenie między interfejsem użytkownika (View) a logiką symulacji (Model).
    /// Implementuje INotifyPropertyChanged do obsługi wiązania danych WPF.
    /// </summary>
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IPoolProcessor _poolProcessor;

        // Komenda do uruchamiania symulacji, powiązana z przyciskiem Start w interfejsie.
        private ICommand _startSimulationCommand;

        private ICommand _stopSimulationCommand;

        private string _ballCountInput;

        // Model stołu bilardowego zawierający wymiary stołu i kolekcję kul.
        private readonly PoolTableModel _poolTableModel;

        // Kolekcja kul bilardowych wyświetlanych w interfejsie użytkownika.
        // Właściwość jest bezpośrednio powiązana z kolekcją w modelu stołu.
        public ObservableCollection<BallModel> Balls => _poolTableModel.Balls;

        // Szerokość stołu bilardowego w pikselach.
        public double PWidth => _poolTableModel.PWidth;
        public double PHeight => _poolTableModel.PHeight;

        // Zdarzenie wywoływane gdy właściwość jakiego obiektu WPF się zmieni
        public event PropertyChangedEventHandler? PropertyChanged;

        // Metoda pomocnicza do wywoływania zdarzenia PropertyChanged.
        // Powiadamia system wiązania danych WPF o zmianie wartości właściwości,
        // co powoduje aktualizację interfejsu użytkownika.
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Właściwość do przechowywania i wyświetlania komunikatów o błędach w interfejsie.
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
            // Wymiary stołu AxB
            int A = 1000;
            int B = 600;

            // Ustawienie wymiarów stołu "Logika"
            _poolProcessor = new PoolProcessor();
            _poolProcessor.CreateTable(A, B);

            // Ustawienie wymiarów stołu "Widok"
            _poolTableModel = new PoolTableModel();
            _poolTableModel.PHeight = B;
            _poolTableModel.PWidth = A;

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
            // Walidacja wprowadzonej liczby kul (1-12)
            if (int.TryParse(BallCountInput, out int ballCount) && ballCount >= 1 && ballCount <= 12)
            {
                // Czyszczenie poprzedniego stanu i komunikatów o błędach
                ErrorMessage = string.Empty;
                Balls.Clear();
                _poolProcessor.ClearTable();

                // Dodanie kul do stołu
                _poolProcessor.AddBalls(ballCount);
                var balls = _poolProcessor.GetAllBallsFromTable().ToList();

                // Dodanie kul do modelu i aktualizacja ich pozycji
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
                // Rozpoczęcie symulacji
                _poolProcessor.Start();
            } else
            {
                ErrorMessage = "Liczba kul musi być liczbą całkowitą z zakresu 1-12.";
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
            // Kluczowa zasada w WPF mówi, że tylko wątek, który stworzył dany element UI (lub obiekt z nim ściśle powiązany), może go bezpośrednio modyfikować.
            // Kiedy w PoolProcessor.Update zmieniamy pozycję IBall i wywołujemy zdarzenie BallMoving,
            // handler OnBallMoving w MainWindowViewModel jest wykonywany na tym samym wątku roboczym timera, a nie na wątku UI.
            // Aby wszystko działało poprawnie, musimy użyć Dispatcher.
            // Działa on jak koordynator i kolejka zadań dla tego wątku UI.
            // Gdy kod wykonywany na innym wątku (np. wątku timera) potrzebuje wykonać jakąś operację,
            // która musi zajść na wątku UI (jak aktualizacja BallModel.X, która powiadomi UI),
            // używa Dispatchera wątku UI, aby zlecić wykonanie tej operacji.
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
