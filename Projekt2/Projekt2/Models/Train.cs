using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    class Train
    {
        public Int32 Id  { get; set; }
        Station Station;
        public Track CurrentTrack { get; set; }
        public Track ExitTrack { get; set; }
        public Platform DestinationPlatform { get; set; }
        public TimeSpan WaitTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public Thread Thread;
        public Train(Station station, Track entry, Int32 id)
        {
            Random random = new Random(); 
            this.Station = station;
            this.CurrentTrack = entry;
            CurrentTrack.Reserve(); 
            this.DestinationPlatform = station.Platforms.ElementAt(random.Next(0, station.Platforms.Count));
            Junction junction = Station.Junctions.ElementAt(random.Next(0, Station.Junctions.Count));
            this.ExitTrack = junction.EntryTracks.ElementAt(random.Next(0, junction.EntryTracks.Count));
            this.WaitTime = new TimeSpan(0,0,0,0,random.Next(0, Station.maxStayTime));
            this.CurrentTime = DateTime.Now;
            this.Id = id;
            this.Thread = new Thread(Run);
            Thread.Start();
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
            while((platformTrack = DestinationPlatform.TryReserve()) == null);

            Junction parentJunction = Station.GetParentJunction(CurrentTrack);
            
            parentJunction.Reserve(this);
            
            Thread.Sleep(Station.junctionTime);
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
            ExitTrack.Reserve();

            Junction parentJunction = Station.GetParentJunction(ExitTrack);
            
            parentJunction.Reserve(this);
            
            Thread.Sleep(Station.junctionTime);
            Track temp = CurrentTrack;
            CurrentTrack = ExitTrack;

            parentJunction.Free();
            temp.Free();

        }
        public void DepartFromStation()
        {
            Thread.Sleep(Station.arrivalTime);
            CurrentTrack.Free();
            Station.Trains.Remove(this);
        }
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
    }
}
