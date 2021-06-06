using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt2.Models
{
    class Junction
    {
        public List<Track> EntryTracks { get; set; }
        public bool IsEmpty { get; set; }
        public TextBox TextBox { get; set; }
        public Mutex JunctionMutex = new Mutex();
        public Junction(TextBox junction, List<TextBox> tracks)
        {
            EntryTracks = new List<Track>();

            for (int i = 0; i < tracks.Count; i++)
            {
                EntryTracks.Add(new Track(tracks[i])); 
            }
            TextBox = junction;
            IsEmpty = true; 
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
