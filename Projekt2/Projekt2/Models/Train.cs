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
            this.CurrentTime = DateTime.Now;
        }
        public Train(Station station, Track entry)
        {
            Random random = new Random(); 
            this.Junctions = station.Junctions;
            this.CurrentTrack = entry;
            this.DestinationPlatform = station.Platforms.ElementAt(random.Next(0, station.Platforms.Count));
            Junction junction = Junctions.ElementAt(random.Next(0, Junctions.Count));
            this.ExitTrack = junction.EntryTracks.ElementAt(random.Next(0, junction.EntryTracks.Count));
            this.WaitTime = new TimeSpan(0,0,0,0,random.Next(0, Station.maxStayTime));
            this.CurrentTime = DateTime.Now;


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
            CurrentTrack.TextBox.Text = "Train"; 
            Thread.Sleep(Station.arrivalTime);
        }

        public void GoToPlatformTrack()
        {
            Track platformTrack;
            while((platformTrack = DestinationPlatform.TryReserve()) == null);

            Junction parentJunction = GetParentJunction(CurrentTrack);
            
            parentJunction.Reserve();
            
            //TODO delay on junction crossing
            Track temp = CurrentTrack;
            CurrentTrack.TextBox.Text = "Free";
            CurrentTrack = platformTrack;
            CurrentTrack.TextBox.Text = "Train";
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

            Junction parentJunction = GetParentJunction(ExitTrack);
            
            parentJunction.Reserve();
            
            //TODO delay on junction crossing
            Track temp = CurrentTrack;
            CurrentTrack.TextBox.Text = "Free"; 
            CurrentTrack = ExitTrack;
            CurrentTrack.TextBox.Text = "Train"; 

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
        Junction GetParentJunction(Track track)
        {
            foreach (var j in Junctions)
                if(j.EntryTracks.Contains(track))
                    return j;
            // TODO: throw error, tried to check platform track
            return null;
        }
    }
}
