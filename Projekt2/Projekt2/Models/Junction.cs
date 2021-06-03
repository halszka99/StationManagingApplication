using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Junction
    {
        public List<Track> EntryTracks { get; set; }
        public bool IsEmpty { get; set; }
        public Mutex JunctionMutex = new Mutex();
        public Junction(int entryTracks)
        {
            for (int i = 0; i < entryTracks; i++)
            {
                EntryTracks.Add(new Track()); 
            }
        }
        public void Reserve()
        {
            JunctionMutex.WaitOne(); 
            if(IsEmpty)
                IsEmpty= false;
            JunctionMutex.ReleaseMutex(); 
        }
        public void Free()
        {
            JunctionMutex.WaitOne();
            IsEmpty = true;
            JunctionMutex.ReleaseMutex();
        }
    }
}
