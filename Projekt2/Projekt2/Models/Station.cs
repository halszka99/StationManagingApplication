using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public Station(int platforms, int trains, int entryTracks)
        {
            Random random = new Random();
            for (int i = 0; i < 1; i++)
            {
                Junctions.Add(new Junction(entryTracks)); 
            }
            for (int i = 0; i < platforms; i++)
            {
                Platforms.Add(new Platform());
            }
            for (int i = 0; i < trains; i++)
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
            stationManager = new Thread(StationManaging);
            trainManager = new Thread(GenerateTrain);
        }
        public void StartSimulation()
        {
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
        }
        public void GenerateTrain()
        {
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
            Track trackToGenerateTrain = emptyTracks.ElementAt(random.Next(0, emptyTracks.Count));
            Train train = new Train(this, trackToGenerateTrain); 
            Trains.Add(train);
            trainThreads.Add(new Thread(train.Run));
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
