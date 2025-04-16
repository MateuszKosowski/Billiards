using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Model
{
    class PoolTableModel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public ObservableCollection<BallModel> Balls { get; } = [];
    }
}
