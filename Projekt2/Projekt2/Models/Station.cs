using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt2.Models
{
    class Station
    {
        public List<Platform> Platforms { get; set; }
        public List<Junction> Junctions { get; set; }
        public List<Train> Trains { get; set; }

        public Thread stationManager; 
        public Thread trainManager; 
        public List<Thread> trainThreads = new List<Thread>();
        public static int maxStayTime = 300;
        public static TimeSpan arrivalTime = new TimeSpan(0,0,0,0,200);
        public static TimeSpan overTime = new TimeSpan(0,0,0,2);

        public Form1 MyForm;

        public Station(Form1 form)
        {
            this.MyForm = form;
            var junctions = this.MyForm.JunctionTextBoxes(); 
            var platforms = this.MyForm.PlatformTracksTextBoxes(); 
            var entryTracksLeft = this.MyForm.LeftEntryTracksTextBoxes();
            var entryTracksRight = this.MyForm.RightEntryTracksTextBoxes();
            Platforms = new List<Platform>();
            Junctions = new List<Junction>();
            Trains = new List<Train>(); 

            Junctions.Add(new Junction(junctions[0], entryTracksLeft));

            Junctions.Add(new Junction(junctions[1], entryTracksRight));
            for (int i = 0; i < platforms.Count / 2; i++)
            {
                Platforms.Add(new Platform(platforms));
            }

            stationManager = new Thread(StationManaging);
            trainManager = new Thread(GenerateTrain);
        }
        public void StartSimulation()
        {
            Random random = new Random();
            for (int i = 0; i < 4; i++)
            {
                while (true)
                {
                    Junction junction = Junctions.ElementAt(random.Next(0, Junctions.Count));
                    Track track = junction.EntryTracks.ElementAt(random.Next(0, junction.EntryTracks.Count));
                    if (track.IsEmpty)
                    {
                        Trains.Add(new Train(this, track));
                        break;
                    }
                }
            }

            stationManager.Start();
            trainManager.Start();
            foreach (var train in Trains)
            {
                trainThreads.Add(new Thread(train.Run));
            }
        }
        public void EndSimulation()
        {
            trainManager.Abort();
            stationManager.Abort();
            foreach (var train in trainThreads)
            {
                train.Abort();
            }
            MyForm.Ready(); 
        }
        public void GenerateTrain()
        {
            Thread.Sleep(300);
            List<Track> emptyTracks = new List<Track>();
            foreach (var junction in Junctions)
            {
                foreach (var track in junction.EntryTracks)
                {
                    if (track.TrackMutex.WaitOne(10))
                    {
                        if (track.IsEmpty)
                            emptyTracks.Add(track);
                        else
                            track.TrackMutex.ReleaseMutex();
                    }
                }
            }

            Random random = new Random();
            if (emptyTracks.Count > 1)
            {
                Track trackToGenerateTrain = emptyTracks.ElementAt(random.Next(0, emptyTracks.Count));
                Train train = new Train(this, trackToGenerateTrain);
                Trains.Add(train);
                trainThreads.Add(new Thread(train.Run));
            }

            foreach (var track in emptyTracks)
            {
                track.TrackMutex.ReleaseMutex(); 
            }
        }
        public void StationManaging()
        {
            foreach (var train in Trains)
            {
                if (DateTime.Now.Subtract(train.CurrentTime) - train.WaitTime > overTime)
                    train.Maneuver(); 
            }
        }
    }
}
