using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    interface IPoolProcessor
    {
        void Initialize();

        void StartSimulation();

        void StopSimulation();

        event EventHandler<BallsUpdateEventArgs> BallsUpdated;
    }
}
