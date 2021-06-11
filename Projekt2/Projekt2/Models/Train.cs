using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projekt2.Models
{
    /// <summary>
    /// Class representing train
    /// </summary>
    class Train
    {
        // Train id
        public Int32 Id  { get; set; }
        // Station that train is arriving to
        readonly Station station;
        // Track that train is on
        public Track CurrentTrack { get; set; }
        // Train exit track
        public Track ExitTrack { get; set; }
        // Platform that train should 
        public Platform DestinationPlatform { get; set; }
        // Time that train will spend on platform
        public TimeSpan WaitTime { get; set; }
        // Train arriving time
        public DateTime CurrentTime { get; set; }
        // Train thread
        public Thread thread;
        /// <summary>
        /// Train constructor
        /// </summary>
        /// <param name="station"> Station that train is arriving to </param>
        /// <param name="entry"> Track that train is arriving to </param>
        /// <param name="id"> Train id </param>
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
        /// <summary>
        /// Method to simulate behavior of train
        /// </summary>
        public void Run()
        {
            ArriveToStation();
            GoToPlatformTrack();
            StayOnTrack();
            GoToExitTrack();
            DepartFromStation();
        }
        /// <summary>
        /// Method to simulate arriving train to station
        /// </summary>
        public void ArriveToStation()
        {
            Thread.Sleep(Station.arrivalTime);
        }
        /// <summary>
        /// Method to simulate going from entry track to platform track
        /// </summary>
        public void GoToPlatformTrack()
        {
            Track platformTrack;
            while ((platformTrack = DestinationPlatform.TryReserve()) == null) ;
            Junction parentJunction = station.GetParentJunction(CurrentTrack);

            while (!parentJunction.Reserve(this)) ;

            Thread.Sleep(Station.junctionTime);
            Track temp = CurrentTrack;
            CurrentTrack = platformTrack; 
            parentJunction.Free();
            temp.Free();
        }
        /// <summary>
        /// Method to simulate staying on track 
        /// </summary>
        public void StayOnTrack()
        {
            Thread.Sleep(WaitTime);
        }
        /// <summary>
        /// Method to simulate going from platform to exit track
        /// </summary>
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
        /// <summary>
        /// Method to simulate departing from station
        /// </summary>
        public void DepartFromStation()
        {
            Thread.Sleep(Station.arrivalTime);
            CurrentTrack.Free();
            station.Trains.Remove(this);
            thread.Abort(); 
        }
        /// <summary>
        /// Method to simulate train maneuver
        /// </summary>
        public void Maneuver()
        {
            // tu manewr wyjazdowy zrobimy
        }
    }
}
