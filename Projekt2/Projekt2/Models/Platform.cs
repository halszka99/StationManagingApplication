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
        public Track TrackTop { get; set; }
        public Track TrackDown { get; set; }
        public List<Train> TrainsQueue { get; set; }
        
        public Platform(List<TextBox> textBoxes)
        {
            TrackTop = new Track(textBoxes[0]); 
            TrackDown = new Track(textBoxes[1]);
        }
        public Track TryReserve()
        {
            if (TrackDown.IsEmpty)
            {
                TrackDown.Reserve();
                return TrackDown; 
            }
            if (TrackTop.IsEmpty)
            {
                TrackTop.Reserve();
                return TrackTop;
            }
            return null; 
        }
        public void Free(Track track)
        {
            track.Free();
            track.TextBox.Text = "Free";
        }
    }
}
