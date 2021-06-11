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

        public enum Status
        {
            ArrivingToStation,
            WaitingForPlatform,
            GoingToPlatform,
            UnloadingOnPlatform,
            WaitingForExitTrack,
            GoingToExitTrack,
            Departing,
            Departed
        }

        public Status TrainStatus { get; set; }

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
            TrainStatus = Status.ArrivingToStation;
            Id = id;

            thread = new Thread(Run);
            thread.Start();
        }
        public void Run()
        {
            while(true)
                switch (TrainStatus)
                {
                    case Status.ArrivingToStation:
                        ArriveToStation();
                        break;
                    case Status.WaitingForPlatform:
                        GoToPlatformTrack();
                        break;
                    case Status.UnloadingOnPlatform:
                        StayOnTrack();
                        break;
                    case Status.WaitingForExitTrack:
                        GoToExitTrack();
                        break;
                    case Status.Departing:
                        DepartFromStation();
                        break;
                    case Status.Departed:
                        thread.Abort(); 
                        return;
                }
        }

        public void ArriveToStation()
        {
            Thread.Sleep(Station.arrivalTime);
            TrainStatus = Status.WaitingForPlatform;
        }

        public void GoToPlatformTrack()
        {
            Track platformTrack = DestinationPlatform.TryReserve();
            if(platformTrack == null)
                return;
            Junction parentJunction = station.GetParentJunction(CurrentTrack);
            TrainStatus = Status.GoingToPlatform;
            while (!parentJunction.Reserve(this)); 
            
            Thread.Sleep(Station.junctionTime);
            Track temp = CurrentTrack;
            CurrentTrack = platformTrack;
            DestinationPlatform.TrainsQueue.Remove(this); 
            parentJunction.Free();
            temp.Free();
            TrainStatus = Status.UnloadingOnPlatform;
        }
        public void StayOnTrack()
        {
            Thread.Sleep(WaitTime);
            TrainStatus = Status.WaitingForExitTrack;
        }
        public void GoToExitTrack()
        {
            if(!ExitTrack.Reserve())
                return;
            Junction parentJunction = station.GetParentJunction(ExitTrack);
            TrainStatus = Status.GoingToExitTrack;
            while (!parentJunction.Reserve(this));

            Thread.Sleep(5000);
            Track temp = CurrentTrack;
            CurrentTrack = ExitTrack;

            parentJunction.Free();
            temp.Free();
            TrainStatus = Status.Departing;
        }
        public void DepartFromStation()
        {
            Thread.Sleep(Station.arrivalTime);
            CurrentTrack.Free();
            station.Trains.Remove(this);
            TrainStatus = Status.Departed;
        }
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
    }
}
