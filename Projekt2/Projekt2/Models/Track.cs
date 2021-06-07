using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt2.Models
{
    class Track
    {
        public String Id  { get; set; }
        public Mutex TrackMutex = new Mutex();
        public bool IsEmpty { get; set; }
        public TextBox TextBox { get; set; }
        public Track(TextBox textBox, String id)
        {
            IsEmpty = true;
            TextBox = textBox;
            Id = id;
        }
        public bool Reserve()
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
        public void Free()
        {
            TrackMutex.WaitOne();
            if (!IsEmpty)
                IsEmpty = true;
            TrackMutex.ReleaseMutex();
        }
    }
}
