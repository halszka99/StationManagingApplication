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
        public Mutex TrackMutex = new Mutex();
        public bool IsEmpty { get; set; }
        public Track()
        {
            IsEmpty = true; 
        }
        public void Reserve()
        {
            TrackMutex.WaitOne();
            if (IsEmpty)
                IsEmpty = false;
            TrackMutex.ReleaseMutex();
        }
        public void Free()
        {
            TrackMutex.WaitOne();
            IsEmpty = true;
            TrackMutex.ReleaseMutex();
        }
    }
}
