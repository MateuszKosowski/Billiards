using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace Presentation.Model
{
    /// <summary>
    /// Model reprezentujący stół bilardowy w interfejsie użytkownika.
    /// Przechowuje wymiary stołu oraz kolekcję kul znajdujących się na stole.
    /// </summary>
    class PoolTableModel
    {
        private double _pWidth;
        private double _pHeight;


        public double PWidth
        {
            get { return _pWidth; }
            set
            {
                _pWidth = value;
            }
        }

        public double PHeight
        {
            get { return _pHeight; }
            set
            {
                _pHeight = value;
            }
        }

        // Kolekcja kul znajdujących się na stole bilardowym.
        // Implementowana jako ObservableCollection, aby automatycznie
        // powiadamiać UI o zmianach w kolekcji (dodawanie, usuwanie kul)..
        public ObservableCollection<BallModel> Balls { get; } = [];
    }
}
