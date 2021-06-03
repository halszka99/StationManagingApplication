using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Train
    {
        public Track CurrentTrack { get; set; }
        public Track ExitTrack { get; set; }
        public Platform DestinationPlatform { get; set; }
        public TimeSpan WaitTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public Train()
        {
            // tutaj każdemu losowo CurrentTrack, ExitTrack, DestinationPlatform
        }

        public void GoToPlatformTrack()
        {
            // tu wjeżdzać na track i wykorzystamy tu mutexa tracku do wjazdu
        }
        public void StayOnTrack()
        {
            // tu będziemy sobie zajmować track przez losowy czas
        }
        public void GoToExitTrack()
        {
            // tu wjeżdzać na track i wykorzystamy tu mutexa tracku do wyjazdu
        }
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
        public void Kill()
        {
            // tu zabijemy wątek pociągu
        }

    }
}
