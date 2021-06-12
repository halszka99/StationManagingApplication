﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt2.Models
{
    /// <summary>
    /// Class representing station 
    /// </summary>
    class Station
    {
        // Number of trains that have passed trough the station 
        Int32 TrainId = 1;
        // List of platforms belonging to station
        public List<Platform> Platforms { get; set; }
        // List of junctions belonging to station
        public List<Junction> Junctions { get; set; }
        // List of trains on the station
        public ReaderWriterLockSlim TrainsLock = new ReaderWriterLockSlim();
        public List<Train> Trains { get; set; }
        // Variable that define if simulation still last
        public bool Go { get; set; }
        // Thread of station manager - is responsible for avoiding deadlocks
        public Thread stationManager;
        // Thread of train manager - is responsible for generating trains
        public Thread trainManager; 
        // Thread of simulation manager - is responsible for updating text boxes
        public Thread simulationManager; 
        // Value indicating max time that train can spend on platform in miliseconds
        public static int maxStayTime = 3;
        // Value indicating time that train spend in junction 
        public static TimeSpan junctionTime = new TimeSpan(0,0,0,1);
        // Value indicating time that train spend on entry track
        public static TimeSpan arrivalTime = new TimeSpan(0,0,0,2);
        // Value indicating max train overtime on platform
        public static TimeSpan overTime = new TimeSpan(0,0,0,10);
        // Value indicating min time of random time to generate train in miliseconds
        public static int minCheckTime = 2000;
        // Value indicating max  time of random time to generate train in miliseconds
        public static int maxCheckTime = 5000;
        // Form of simulation
        public Form1 MyForm;

        /// <summary>
        /// Station constructor
        /// </summary>
        /// <param name="form"> Form of simulation </param>
        public Station(Form1 form)
        {
            MyForm = form;
            // Getting TextBoxes from form 
            var junctions = MyForm.JunctionTextBoxes(); 
            var platforms = MyForm.PlatformTracksTextBoxes(); 
            var entryTracksLeft = MyForm.LeftEntryTracksTextBoxes();
            var entryTracksRight = MyForm.RightEntryTracksTextBoxes();

            // Creating lists of platforms, junctions and trains
            Platforms = new List<Platform>();
            Junctions = new List<Junction>();
            Trains = new List<Train>();

            // Adding junctions 
            Junctions.Add(new Junction(junctions[0], entryTracksLeft, "L"));
            Junctions.Add(new Junction(junctions[1], entryTracksRight, "R"));
            // Adding platforms and assigning TextBoxes to platforms
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
            // Creating threads 
            stationManager = new Thread(StationManaging);
            trainManager = new Thread(GenerateTrain);
            simulationManager = new Thread(SimulationManaging);
        }
        /// <summary>
        /// Method to update track TextBoxes
        /// </summary>
        /// <param name="track"> Track that TextBox is updating </param>
        void UpdateTrackLabel(Track track)
        {
            track.TextBox.Invoke((Action)delegate
            {
                if (track.IsEmpty)
                    track.TextBox.Text = track.Id + " Free";
                else
                {
                    Train tr = null;
                    TrainsLock.EnterReadLock();
                    tr = Trains.Find(t => t.CurrentTrack == track);
                    TrainsLock.ExitReadLock();
                    if(tr != null)
                        track.TextBox.Text = track.Id + " T" + tr.Id;
                    else
                        track.TextBox.Text = track.Id + " Reserved";
                }

            });
        }
        /// <summary>
        /// Method to managing simulation 
        /// </summary>
        private void SimulationManaging()
        {
            while (Go)
            {
                foreach (var junction in Junctions)
                {
                    // Updating junction TextBoxes
                    if (junction.IsEmpty)
                        SetTextBox(junction.TextBox, "Free");
                    else if (junction.OccupiedBy != null) 
                        SetTextBox(junction.TextBox, "T"+junction.OccupiedBy.Id);
                    else
                        SetTextBox(junction.TextBox, "Reserved");
                    // Updating EntryTracks TextBoxes
                    junction.EntryTracks.ForEach(track => UpdateTrackLabel(track));

                }
                // Updating platform tracks TextBoxes
                foreach (var platform in Platforms)
                {
                    UpdateTrackLabel(platform.TrackDown);
                    UpdateTrackLabel(platform.TrackTop);
                }
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// Method to update indicated TextBox with text
        /// </summary>
        /// <param name="textBox"> TextBox that is updating </param>
        /// <param name="text"> Text to fill TextBox </param>
        private void SetTextBox(TextBox textBox, string text)
        {
            textBox.Invoke((Action)delegate
            {
                textBox.Text = text;
            });
        }
        /// <summary>
        /// Method to start simulation - start threads
        /// </summary>
        public void StartSimulation()
        {
            Go = true; 
            stationManager.Start();
            trainManager.Start();
            simulationManager.Start(); 
        }
        /// <summary>
        /// Method to end simulation - abort threads
        /// </summary>
        public void EndSimulation()
        {
            Go = false; 
            simulationManager.Abort();
            trainManager.Abort();
            stationManager.Abort();
            TrainsLock.EnterWriteLock();
            foreach (var train in Trains)
                train.thread.Abort();
            TrainsLock.ExitWriteLock();
            
        }
        /// <summary>
        /// Method to generate trains
        /// </summary>
        public void GenerateTrain()
        {
            while (Go)
            {
                Random random = new Random();
                int sleep = random.Next(minCheckTime, maxCheckTime);
                Thread.Sleep(sleep);
                // Making list of empty tracks 
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
                // Adding train on random empty track if there is more than 1 empty track
                if (emptyTracks.Count > 1)
                {
                    Track trackToGenerateTrain = emptyTracks.ElementAt(random.Next(0, emptyTracks.Count));
                    Train train = new Train(this, trackToGenerateTrain, TrainId++);
                    TrainsLock.EnterWriteLock();
                    Trains.Add(train);
                    TrainsLock.ExitWriteLock();
                }
                // Releasing mutex of empty tracks
                foreach (var track in emptyTracks)
                {
                    track.TrackMutex.ReleaseMutex();
                }
            }
        }
        /// <summary>
        /// Method to managing trains in case of deadlock
        /// </summary>
        public void StationManaging()
        {
            while (Go)
            {
                TrainsLock.EnterReadLock();
                var lockedTrains = Trains.FindAll(t =>
                        (t.TrainStatus == Train.Status.WaitingForExitTrack) &&
                        (DateTime.Now.Subtract(t.DepartTime) > overTime));
                TrainsLock.ExitReadLock();
                foreach (var trainX in lockedTrains)
                {
                    Track deadlock_peron_track = trainX.CurrentTrack;
                    Track deadlock_exit_track = trainX.ExitTrack;
                    Platform deadlock_platform = trainX.DestinationPlatform;
                    Junction deadlock_junction = GetParentJunction(deadlock_exit_track);

                    TrainsLock.EnterReadLock();
                    Train trainY = Trains.Find(t => t.CurrentTrack == deadlock_exit_track);
                    TrainsLock.ExitReadLock();
                    if (trainY == null || trainY.TrainStatus == Train.Status.Departing || trainY.TrainStatus == Train.Status.Departed)
                        continue;

                    trainX.DestinationPlatform.TrainsQueueLock.EnterWriteLock();
                    trainX.DestinationPlatform.TrainsQueue.Remove(trainX);
                    trainX.DestinationPlatform.TrainsQueueLock.ExitWriteLock();

                    trainY.DestinationPlatform.TrainsQueueLock.EnterWriteLock();
                    trainY.DestinationPlatform.TrainsQueue.Remove(trainY);
                    trainY.DestinationPlatform.TrainsQueueLock.ExitWriteLock();

                    while (!deadlock_junction.TryReserve()) ;

                    TrainsLock.EnterReadLock();
                    foreach (var train in Trains)
                        train.ForceMoveFlag = true;
                    TrainsLock.ExitReadLock();

                    Track emptyExitTrack;
                    while ((emptyExitTrack = GetEmptyExitTrack()) == null) ;
                    trainX.ForceMove(emptyExitTrack, deadlock_junction);
                    trainY.ForceMove(deadlock_peron_track, deadlock_junction);
                    trainX.ForceMove(deadlock_exit_track, deadlock_junction);
                    trainY.TrainStatus = Train.Status.UnloadingOnPlatform;
                    trainX.TrainStatus = Train.Status.Departing;

                    deadlock_junction.Free();
                    TrainsLock.EnterReadLock();
                    foreach (var train in Trains)
                        train.ForceMoveFlag = false;
                    TrainsLock.ExitReadLock();
                }
            }
        }
        /// <summary>
        /// Method to find empty exit track 
        /// </summary>
        /// <returns> exit(entry) track  without train, but with acquired reservation</returns>
        public Track GetEmptyExitTrack()
        {
            Track ret;
            foreach (var j in Junctions)
            {
                ret = j.EntryTracks.Find(t => t.TryReserve());
                if(ret != null)
                    return ret;
            }
            return null;
        }
        /// <summary>
        /// Method to find junction that track is belonging to 
        /// </summary>
        /// <param name="track"> Track that parent junction is being searched for </param>
        /// <returns> Junction that indicated track is belonging to </returns>
        public Junction GetParentJunction(Track track)
        {
            foreach (var j in Junctions)
                if(j.EntryTracks.Contains(track))
                    return j;
            return null;
        }
    }
}
