using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Model
{
    class BallModel : INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        private string _color;
        private int _number;
        private int _diameter;
        private int _radius;

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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
