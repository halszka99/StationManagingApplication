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
        public List<Thread> trainThreads = new List<Thread>();
        public static int maxStayTime = 900;
        public static TimeSpan arrivalTime = new TimeSpan(0,0,0,0,600);
        public static TimeSpan overTime = new TimeSpan(0,0,0,2);

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
                Platforms.Add(new Platform(temp));

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
                    Train tr = Trains.Find(t => t.CurrentTrack == track);
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
                    else
                        SetTextBox(junction.TextBox, "Train");

                    foreach (var track in junction.EntryTracks)
                    {
                        if (track.IsEmpty)
                            SetTextBox(track.TextBox, "Free");
                        else
                            SetTextBox(track.TextBox, "Train");
                    }

                    
                    junction.EntryTracks.ForEach(track => UpdateTrackLabel(track));

                }

                foreach (var platform in Platforms)
                {

                    if (platform.TrackDown.IsEmpty)
                        SetTextBox(platform.TrackDown.TextBox, "Free");
                    else
                        SetTextBox(platform.TrackDown.TextBox, "Train");

                    if (platform.TrackTop.IsEmpty)
                        SetTextBox(platform.TrackTop.TextBox, "Free");
                    else
                        SetTextBox(platform.TrackTop.TextBox, "Train");

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
            foreach (var train in trainThreads)
            {
                train.Abort();
            }
        }
        public void GenerateTrain()
        {
            while (Go)
            {
                Thread.Sleep(100);
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
                    Train train = new Train(this, trackToGenerateTrain,TrainId++);
                    Trains.Add(train);
                    trainThreads.Add(new Thread(train.Run));
                    trainThreads.Last().Start(); 
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
    }
}
