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
        Int32 TrainId = 1;
        public List<Platform> Platforms { get; set; }
        public List<Junction> Junctions { get; set; }
        public List<Train> Trains { get; set; }
        public bool Go { get; set; }
        public Thread stationManager; 
        public Thread trainManager; 
        public Thread simulationManager; 
        public static int maxStayTime = 3000;
        public static TimeSpan junctionTime = new TimeSpan(0,0,0,1);
        public static TimeSpan arrivalTime = new TimeSpan(0,0,0,2);
        public static TimeSpan overTime = new TimeSpan(0,0,0,2);
        public static int minCheckTime = 2000;
        public static int maxCheckTime = 5000;

        public Form1 MyForm;

        public Station(Form1 form)
        {
            MyForm = form;
            var junctions = MyForm.JunctionTextBoxes(); 
            var platforms = MyForm.PlatformTracksTextBoxes(); 
            var entryTracksLeft = MyForm.LeftEntryTracksTextBoxes();
            var entryTracksRight = MyForm.RightEntryTracksTextBoxes();

            Platforms = new List<Platform>();
            Junctions = new List<Junction>();
            Trains = new List<Train>();

            Junctions.Add(new Junction(junctions[0], entryTracksLeft, "L"));
            Junctions.Add(new Junction(junctions[1], entryTracksRight, "R"));
            for (int i = 0; i < platforms.Count / 2; i++)
            {
                List<TextBox> temp = new List<TextBox>
                {
                    platforms[2 * i],
                    platforms[2 * i + 1]
                };
                Platforms.Add(new Platform(temp,i*2+1));

            }
            Go = false; 
            stationManager = new Thread(StationManaging);
            trainManager = new Thread(GenerateTrain);
            simulationManager = new Thread(SimulationManaging);
        }

        void UpdateTrackLabel(Track track)
        {
            track.TextBox.Invoke((Action)delegate
            {
                if (track.IsEmpty)
                    track.TextBox.Text = track.Id + " Free";
                else
                {
                    Train tr = null;
                    tr = Trains.Find(t => t.CurrentTrack == track);
                    if(tr != null)
                        track.TextBox.Text = track.Id + " T" + tr.Id;
                    else
                        track.TextBox.Text = track.Id + " Reserved";
                }

            });
        }
        private void SimulationManaging()
        {
            while (Go)
            {
                foreach (var junction in Junctions)
                {

                    if (junction.IsEmpty)
                        SetTextBox(junction.TextBox, "Free");
                    else if (junction.OccupiedBy != null) 
                        SetTextBox(junction.TextBox, "T"+junction.OccupiedBy.Id);
                    else
                        SetTextBox(junction.TextBox, "Reserved");

                    junction.EntryTracks.ForEach(track => UpdateTrackLabel(track));

                }

                foreach (var platform in Platforms)
                {
                    UpdateTrackLabel(platform.TrackDown);
                    UpdateTrackLabel(platform.TrackTop);
                }
            }
        }

        private void SetTextBox(TextBox textBox, string text)
        {
            textBox.Invoke((Action)delegate
            {

                textBox.Text = text;

            });
        }

        public void StartSimulation()
        {
            Go = true; 
            stationManager.Start();
            trainManager.Start();
            simulationManager.Start(); 
        }
        public void EndSimulation()
        {
            Go = false; 
            trainManager.Abort();
            stationManager.Abort();
            simulationManager.Abort();
            foreach (var train in Trains)
                train.thread.Abort();
            
        }
        public void GenerateTrain()
        {
            Random random = new Random(); 
            while (Go)
            {
                int sleep = random.Next(minCheckTime, maxCheckTime); 
                Thread.Sleep(sleep);

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

                if (emptyTracks.Count > 1)
                {
                    Track trackToGenerateTrain = emptyTracks.ElementAt(random.Next(0, emptyTracks.Count));
                    Train train = new Train(this, trackToGenerateTrain,TrainId++);
                    Trains.Add(train);
                }

                foreach (var track in emptyTracks)
                {
                    track.TrackMutex.ReleaseMutex();
                }
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
        public Track GetEmptyPeronTrack()
        {
            foreach (var platform in Platforms)
            {
                if(platform.TrackTop.IsEmpty)
                    return platform.TrackTop;
                if(platform.TrackDown.IsEmpty)
                    return platform.TrackDown;
            }
            return null;
        }

        public Track GetEmptyExitTrack()
        {
            Track ret;
            foreach (var j in Junctions)
            {
                ret = j.EntryTracks.Find(t => t.IsEmpty);
                if(ret != null)
                    return ret;
            }
            return null;
        }
        public Junction GetParentJunction(Track track)
        {
            foreach (var j in Junctions)
                if(j.EntryTracks.Contains(track))
                    return j;
            // TODO: throw error, tried to check platform track
            return null;
        }
    }
}
