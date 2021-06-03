using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Track
    {
        Mutex TrackMutex = new Mutex();
        public bool IsEmpty { get; set; }
        public Track()
        {
           // ustawimy każdmu isEmpty na true na początek
        }
    }
}
