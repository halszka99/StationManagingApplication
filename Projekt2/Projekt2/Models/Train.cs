using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Train
    {
        public int Id { get; set; }
        public EntryTrack EntryTrack { get; set; }
        public EntryTrack ExitTrack { get; set; }
        public Track DestinationTrack { get; set; }
        public Track CurrentTrack { get; set; }

    }
}
