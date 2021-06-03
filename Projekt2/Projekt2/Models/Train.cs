using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Train
    {
        List<Junction> Junctions;
        public Track CurrentTrack { get; set; }
        public Track ExitTrack { get; set; }
        public Platform DestinationPlatform { get; set; }
        public TimeSpan WaitTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public Train(List<Junction> junctions, Track entry, Platform platform, Track exit, TimeSpan waitTime)
        {
            this.Junctions = junctions;
            this.CurrentTrack = entry;
            this.DestinationPlatform = platform;
            this.ExitTrack = exit;
            this.WaitTime = waitTime;
            this.CurrentTime = DateTime.Now();
        }
        public void Run()
        {
            ArriveToStation();
            GoToPlatformTrack();
            StayOnTrack();
            GoToExitTrack();
            DepartFromStation();
        }

        public void ArriveToStation()
        {
            Thread.Sleep(Station.arrivalTime);
        }

        public void GoToPlatformTrack()
        {
            Track platformTrack;
            while((platformTrack = DestinationPlatform.tryReserve()) == null);

            Junction parentJunction = getParentJunction(CurrentTrack);
            
            parentJunction.Reserve();
            
            //TODO delay on junction crossing
            Track temp = CurrentTrack;

            CurrentTrack = platformTrack;

            parentJunction.Free();
            temp.Free();

        }
        public void StayOnTrack()
        {
            Thread.Sleep(WaitTime);
        }
        public void GoToExitTrack()
        {
            // tu wjeżdzać na track i wykorzystamy tu mutexa tracku do wyjazdu
            ExitTrack.Reserve();

            Junction parentJunction = getParentJunction(ExitTrack);
            
            parentJunction.Reserve();
            
            //TODO delay on junction crossing
            Track temp = CurrentTrack;

            CurrentTrack = ExitTrack;

            parentJunction.Free();
            temp.Free();

        }
        public void DepartFromStation()
        {
            Thread.Sleep(Station.arrivalTime);
        }
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
        public void Kill()
        {
            // tu zabijemy wątek pociągu
        }

        Junction getParentJunction(Track track)
        {
            foreach (var j in Junctions)
                if(j.ContainsTrack(track))
                    return j;
            // TODO: throw error, checked platform track
            return null;
        }
    }
}
