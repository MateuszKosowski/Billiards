using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Model
{
    /// <summary>
    /// Model reprezentujący kulę bilardową w interfejsie użytkownika.
    /// Implementuje INotifyPropertyChanged do obsługi wiązania danych WPF.
    /// </summary>
    class BallModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private string _color;
        private int _number;
        private int _diameter;
        private int _radius;
        private int _weight = 1; // Waga kuli, domyślnie 1

        // Właściwość X określa pozycję lewego górnego rogu kuli na Canvas.
        // Aby zgadzało się z naszym założeniem o środku w środku kuli - jest obliczana przez odjęcie promienia od środka kuli,
        // co pozwala na poprawne wyświetlenie kuli na canvasie.
        public double X
        {
            get { return _x - _radius; }
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public double Y
        {
            get { return _y - _radius; }
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        public int Diameter
        {
            get { return _diameter; }
        }

        public int Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _diameter = value * 2;
                OnPropertyChanged(nameof(_radius));
            }
        }

        public int Weight
        {
            get { return _weight; }
        }

        // Zdarzenie wywoływane, gdy właściwość modelu zostaje zmieniona.
        // Implementacja interfejsu INotifyPropertyChanged wymagana dla poprawnego wiązania danych WPF.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Metoda pomocnicza do wywoływania zdarzenia PropertyChanged.
        // Powiadamia system wiązania danych WPF o zmianie wartości właściwości,
        // co powoduje aktualizację interfejsu użytkownika.
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
