using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Platform
    {
        public Track TrackTop { get; set; }
        public Track TrackDown { get; set; }
        public List<Train> TrainsQueue { get; set; }
        
        public Platform()
        {
            // no i tu byśmy przypisaywali tylko który jest dolny i który górny
        }

    }
}
