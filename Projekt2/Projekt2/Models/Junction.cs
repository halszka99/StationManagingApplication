using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Junction
    {
        public List<Track> EntryTracks { get; set; }
        public bool IsEmpty { get; set; }
        Mutex JunctionMutex = new Mutex();
        public Junction()
        {
            // tu będziemy tworzyć entry tracki do junctiona
        }
        public void Reserve()
        {
            // tu będziemy wprowadzać na junction do wjazdu
        }
        public void Free()
        {
            // tu będziemy wprowadzać na junction do wjazdu
        }
    }
}
