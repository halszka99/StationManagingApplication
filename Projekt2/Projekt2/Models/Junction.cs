using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt2.Models
{
    /// <summary>
    /// Class representing junction
    /// </summary>
    class Junction
    {
        // List of entry tracks belonging to junction
        public List<Track> EntryTracks { get; set; }
        // Variable defining if junction is empty
        public bool IsEmpty { get; set; }
        // Variable defining which train junction is occupied by
        public Train OccupiedBy { get; set; }
        // TextBox belonging to junctions
        public TextBox TextBox { get; set; }
        // Junction mutex
        public Mutex junctionMutex = new Mutex();

        /// <summary>
        /// Junction constructor
        /// </summary>
        /// <param name="junction"> TextBox belonging to junction </param>
        /// <param name="tracks"> List of entry tracks belonging to junction </param>
        /// <param name="side"> Value defining if its left or right junction </param>
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

        /// <summary>
        /// Method to reserve junction. 
        /// </summary>
        /// <param name="train"> Train that is trying to reserve junction </param>
        /// <returns> Returns true if junction has been reserved </returns>
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

        /// <summary>
        /// Method to free occupied junction
        /// </summary>
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
