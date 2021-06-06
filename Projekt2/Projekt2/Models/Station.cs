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
        public Thread simulationManager; 
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
                List <TextBox> temp = new List<TextBox>();

                temp.Add(platforms[2*i]);
                temp.Add(platforms[2*i+1]);
                Platforms.Add(new Platform(temp));
            }

            stationManager = new Thread(StationManaging);
            trainManager = new Thread(GenerateTrain);
            simulationManager = new Thread(SimulationManaging);
        }

        private void SimulationManaging()
        {
            while (true)
            {
                foreach (var junction in Junctions)
                {
                    if (junction.IsEmpty)
                    {
                        junction.TextBox.Invoke((Action)delegate
                        {

                            junction.TextBox.Text = "Free";

                        });
                    }
                    else
                    {
                        junction.TextBox.Invoke((Action)delegate
                        {

                            junction.TextBox.Text = "Train";

                        });
                    }
                    foreach (var track in junction.EntryTracks)
                    {
                        if (track.IsEmpty)
                        {
                            track.TextBox.Invoke((Action)delegate
                            {

                                track.TextBox.Text = "Free";

                            });
                        }
                        else
                        {
                            track.TextBox.Invoke((Action)delegate
                            {

                                track.TextBox.Text = "Train";

                            });
                        }
                    }
                }
                foreach (var platform in Platforms)
                {
                    if (platform.TrackDown.IsEmpty)
                    {
                        platform.TrackDown.TextBox.Invoke((Action)delegate
                        {

                            platform.TrackDown.TextBox.Text = "Free";

                        });
                    }
                    else
                    {
                        platform.TrackDown.TextBox.Invoke((Action)delegate
                        {

                            platform.TrackDown.TextBox.Text = "Train";

                        });
                    }

                    if (platform.TrackTop.IsEmpty)
                    {
                        platform.TrackTop.TextBox.Invoke((Action)delegate
                        {

                            platform.TrackTop.TextBox.Text = "Free";

                        });
                    }
                    else
                    {
                        platform.TrackTop.TextBox.Invoke((Action)delegate
                        {

                            platform.TrackTop.TextBox.Text = "Train";

                        });
                    };
                }
            }
        }
        public void StartSimulation()
        {
            stationManager.Start();
            trainManager.Start();
            simulationManager.Start(); 
        }
        public void EndSimulation()
        {
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
            while (true)
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
                    Train train = new Train(this, trackToGenerateTrain);
                    Trains.Add(train);
                    trainThreads.Add(new Thread(train.Run));
                    trainThreads[trainThreads.Count - 1].Start();
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
