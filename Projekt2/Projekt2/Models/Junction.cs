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
        public Train OccupiedBy { get; set; }
        public TextBox TextBox { get; set; }
        public Mutex junctionMutex = new Mutex();
        public Junction(TextBox junction, List<TextBox> tracks, string side)
        {
            EntryTracks = new List<Track>();

            for (int i = 0; i < tracks.Count; i++)
            {
                EntryTracks.Add(new Track(tracks[i], (i+1)+side)); 
            }
            TextBox = junction;
            IsEmpty = true; 
        }
        public bool Reserve(Train train = null)
        {
            bool reserved = false; 
            junctionMutex.WaitOne(); 
            if(IsEmpty)
            {
                IsEmpty= false;
                OccupiedBy = train;
                reserved = true;
            }
            junctionMutex.ReleaseMutex();
            return reserved; 
        }
        public void Free()
        {
            junctionMutex.WaitOne();
            if (!IsEmpty)
            {
                IsEmpty = true;
                OccupiedBy = null;
            }
            junctionMutex.ReleaseMutex();
        }
    }
}
