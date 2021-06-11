using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Projekt2; 

namespace Projekt2.Models
{
    class Platform
    {
        public Int32 Id { get; set; }
        public Track TrackTop { get; set; }
        public Track TrackDown { get; set; }
        public List<Train> TrainsQueue { get; set; } = new List<Train>();
        
        public Platform(List<TextBox> textBoxes, Int32 id)
        {
            TrackTop = new Track(textBoxes[0],id.ToString()); 
            TrackDown = new Track(textBoxes[1],(id+1).ToString());
        }
        public Track TryReserve()
        {
            if(TrackDown.TryReserve())
                return TrackDown; 
            else if(TrackTop.TryReserve())
                return TrackTop;
            return null; 
        }
        public void Free(Track track)
        {
            track.Free();
        }
    }
}
