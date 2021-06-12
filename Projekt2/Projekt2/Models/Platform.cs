using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Projekt2; 

namespace Projekt2.Models
{
    /// <summary>
    /// Class representing platform
    /// </summary>
    class Platform
    {
        // Platform id
        public Int32 Id { get; set; }
        // Variable indicating top track of platform 
        public Track TrackTop { get; set; }
        // Variable indicating down track of platform 
        public Track TrackDown { get; set; }
        // Lock for trains queue
        public ReaderWriterLock TrainsQueueLock = new ReaderWriterLock();
        // List of trains that destination platform is this platform
        public List<Train> TrainsQueue { get; set; }
        /// <summary>
        /// Platform constructor
        /// </summary>
        /// <param name="textBoxes"> TextBox belonging to platform </param>
        /// <param name="id"> Platform id </param>
        public Platform(List<TextBox> textBoxes, Int32 id)
        {
            TrackTop = new Track(textBoxes[0],id.ToString()); 
            TrackDown = new Track(textBoxes[1],(id+1).ToString());
            TrainsQueue = new List<Train>(); 
        }

        /// <summary>
        /// Method to try reserve one of tracks belonging to platform
        /// </summary>
        /// <returns> Track that was reserved or null when none was reserved </returns>
        public Track TryReserve()
        {
            if(TrackDown.TryReserve())
                return TrackDown; 
            else if(TrackTop.TryReserve())
                return TrackTop;
            return null; 
        }

        /// <summary>
        /// Method to free occupied track belonging to platform 
        /// </summary>
        /// <param name="track"> Track to free </param>
        public void Free(Track track)
        {
            track.Free();
        }
    }
}
