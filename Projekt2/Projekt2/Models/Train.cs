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
        readonly Station station;
        public Track CurrentTrack { get; set; }
        public Track ExitTrack { get; set; }
        public Platform DestinationPlatform { get; set; }
        public TimeSpan WaitTime { get; set; }
        public DateTime CurrentTime { get; set; }
        public Thread thread;
        public Train(Station station, Track entry, Int32 id)
        {
            Random random = new Random(); 
            this.station = station;
            CurrentTrack = entry;
            while(!CurrentTrack.Reserve()); 
            DestinationPlatform = station.Platforms.ElementAt(random.Next(0, station.Platforms.Count));
            DestinationPlatform.TrainsQueue.Add(this); 
            Junction junction = station.Junctions.ElementAt(random.Next(0, station.Junctions.Count));
            ExitTrack = junction.EntryTracks.ElementAt(random.Next(0, junction.EntryTracks.Count));
            WaitTime = new TimeSpan(0,0,0,0,random.Next(0, Station.maxStayTime));
            CurrentTime = DateTime.Now;
            Id = id;
            thread = new Thread(Run);
            thread.Start();
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
            while (DestinationPlatform.TrainsQueue.First() != this) ;

            Track platformTrack;
            while ((platformTrack = DestinationPlatform.TryReserve()) == null) ;
            Junction parentJunction = station.GetParentJunction(CurrentTrack);

            while (!parentJunction.Reserve(this)) ;

            Thread.Sleep(Station.junctionTime);
            Track temp = CurrentTrack;
            CurrentTrack = platformTrack;
            DestinationPlatform.TrainsQueue.Remove(this); 
            parentJunction.Free();
            temp.Free();
        }
        public void StayOnTrack()
        {
            Thread.Sleep(WaitTime);
        }
        public void GoToExitTrack()
        {
            while(!ExitTrack.Reserve());
            Junction parentJunction = station.GetParentJunction(ExitTrack);
            while (!parentJunction.Reserve(this));

            Thread.Sleep(5000);
            Track temp = CurrentTrack;
            CurrentTrack = ExitTrack;

            parentJunction.Free();
            temp.Free();

        }
        public void DepartFromStation()
        {
            Thread.Sleep(Station.arrivalTime);
            CurrentTrack.Free();
            station.Trains.Remove(this);
            thread.Abort(); 
        }
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
    }
}
