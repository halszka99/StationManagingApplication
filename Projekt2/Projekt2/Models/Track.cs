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
    /// Class representing track
    /// </summary>
    class Track
    {
        // Track id with side if it is entry track
        public String Id  { get; set; }
        // Track mutex
        public Mutex TrackMutex = new Mutex();
        // Variable defining if track is empty
        public bool IsEmpty { get; set; }
        // TextBox belonging to track
        public TextBox TextBox { get; set; }

        /// <summary>
        /// Track constructor 
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="id"></param>
        public Track(TextBox textBox, String id)
        {
            IsEmpty = true;
            TextBox = textBox;
            Id = id;
        }

        /// <summary>
        /// Method to reserve track 
        /// </summary>
        /// <returns> Returns true if track has been reserved </returns>
        public bool TryReserve()
        {
            bool reserved = false;
            TrackMutex.WaitOne();
            if (IsEmpty)
            {
                IsEmpty = false;
                reserved = true; 
            }
            TrackMutex.ReleaseMutex();
            return reserved; 
        }

        /// <summary>
        /// Method to free occupied track
        /// </summary>
        public void Free()
        {
            TrackMutex.WaitOne();
            if (!IsEmpty)
                IsEmpty = true;
            TrackMutex.ReleaseMutex();
        }
    }
}
