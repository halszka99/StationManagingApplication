using System;
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
            //trainManager = new Thread(GenerateTrain);
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
                    tr = Trains.Find(t => t.CurrentTrack == track);
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
            //trainManager.Start();
            simulationManager.Start(); 
        }
        /// <summary>
        /// Method to end simulation - abort threads
        /// </summary>
        public void EndSimulation()
        {
            Go = false; 
            simulationManager.Abort();
            //trainManager.Abort();
            stationManager.Abort();
            foreach (var train in Trains)
                train.thread.Abort();
            
        }
        public void StationManaging()
        {
            while (Go)
            {
                Thread.Sleep(1);
                Maneuver();
                GenerateTrain(); 
            }
        }
        /// <summary>
        /// Method to generate trains
        /// </summary>
        public void GenerateTrain()
        {
            Random random = new Random();
            //int sleep = random.Next(minCheckTime, maxCheckTime); 

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
                Train train = new Train(this, trackToGenerateTrain,TrainId++);
                Trains.Add(train);
            }
            // Releasing mutex of empty tracks
            foreach (var track in emptyTracks)
            {
                track.TrackMutex.ReleaseMutex();
            }
        }

        public void Maneuver()
        {
            Train trainX = Trains.Find(t =>
                    (t.TrainStatus == Train.Status.WaitingForExitTrack) &&
                    (DateTime.Now.Subtract(t.DepartTime) > overTime));

            if (trainX != null)
            {
                Track deadlock_peron_track = trainX.CurrentTrack;
                Track deadlock_exit_track = trainX.ExitTrack;
                Platform deadlock_platform = trainX.DestinationPlatform;
                Junction deadlock_junction = GetParentJunction(deadlock_exit_track);

                Train trainY = Trains.Find(t => t.CurrentTrack == deadlock_exit_track);
                if (trainY == null || trainY.TrainStatus == Train.Status.Departing || trainY.TrainStatus == Train.Status.Departed)
                    return;

                trainX.DestinationPlatform.TrainsQueueLock.EnterWriteLock();
                trainX.DestinationPlatform.TrainsQueue.Remove(trainX);
                trainX.DestinationPlatform.TrainsQueueLock.ExitWriteLock();

                trainY.DestinationPlatform.TrainsQueueLock.EnterWriteLock();
                trainY.DestinationPlatform.TrainsQueue.Remove(trainY);
                trainY.DestinationPlatform.TrainsQueueLock.ExitWriteLock();

                while (!deadlock_junction.TryReserve());

                foreach (var train in Trains)
                {
                    train.ForceMoveFlag = true;
                }

                Track emptyExitTrack;
                while ( (emptyExitTrack = GetEmptyExitTrack()) == null);
                trainX.ForceMove(emptyExitTrack, deadlock_junction);
                trainY.ForceMove(deadlock_peron_track, deadlock_junction);
                trainX.ForceMove(deadlock_exit_track, deadlock_junction);
                trainY.TrainStatus = Train.Status.UnloadingOnPlatform;
                trainX.TrainStatus = Train.Status.Departing;

                deadlock_junction.Free(); 
                foreach (var train in Trains)
                {
                    train.ForceMoveFlag = false;
                }
            }
        }
        /// <summary>
        /// Method to managing station - avoiding deadlocks
        /// </summary>
        public void Maneuver2()
        {
                //find all trains which are waiting for too long for exiting platform
                var trainX = Trains.Find(t =>
                    ((t.TrainStatus == Train.Status.WaitingForExitTrack) && (DateTime.Now.Subtract(t.DepartTime) > overTime)));

                //foreach (Train trainX in lockedTrains)
                if (trainX != null)
                {
                    Track deadlock_peron_track = trainX.CurrentTrack;
                    Track deadlock_exit_track = trainX.ExitTrack;
                    Platform deadlock_platform = trainX.DestinationPlatform;
                    Junction deadlock_junction = GetParentJunction(deadlock_exit_track);

                    //get train which locks X's exit track
                    Train trainY = Trains.Find(t => t.CurrentTrack == deadlock_exit_track);
                //start acting only when both are waiting for "track swap"
                if (trainY == null || trainY.TrainStatus != Train.Status.WaitingForPlatform)
                    return;

                    //override control
                    trainX.ForceMoveFlag = true;
                    trainY.ForceMoveFlag = true;

                    //manually managed junction traffic between X and Y trains
                    while (!deadlock_junction.TryReserve()) ;

                    //get and reserve empty peron track to move Y here (so X can go away) 
                    Track empty_peron_track = GetEmptyPeronTrack();
                    Track empty_exit_track = null;
                    Junction empty_exit_track_junction = null;
                    Train trainZ = null;
                    //no empty peron track found
                    if (empty_peron_track == null)
                    {
                        //wait for empty exit track and get one with reservation
                        while ((empty_exit_track = GetEmptyExitTrack()) == null) ;

                        //junction between peron and empty exit track
                        empty_exit_track_junction = GetParentJunction(empty_exit_track);
                        //using different junction than between X and Y
                        if (empty_exit_track_junction != deadlock_junction)
                            while (!empty_exit_track_junction.TryReserve()) ;

                        //get 2nd track from blocked platform (track neighbouring to X train)
                        empty_peron_track = (deadlock_platform.TrackDown != deadlock_peron_track ? deadlock_peron_track : deadlock_platform.TrackTop);
                        trainZ = Trains.Find(t => t.CurrentTrack == empty_peron_track);

                        //train departed meanwhile
                        if (trainZ != null)
                        {
                            trainZ.ForceMoveFlag = true;
                            //final state: X, Y without changes, Z moved to exit track, one platform is free.
                            trainZ.ForceMove(empty_exit_track);
                        }
                        else
                        {
                            while (!empty_peron_track.TryReserve()) ;
                            empty_exit_track.Free();
                            empty_exit_track_junction.Free();
                            empty_exit_track = null;
                            empty_exit_track_junction = null;
                        }
                    }

                    //move Y to empty platform track
                    trainY.ForceMove(empty_peron_track);

                    //move X to its exit track, wait for its departure
                    trainX.ForceMove(deadlock_exit_track);
                    trainX.TrainStatus = Train.Status.Departing;
                    trainX.DepartFromStation(false);

                    //move Y to X initial place - Y destination platform
                    trainY.ForceMove(deadlock_exit_track);
                    trainY.ForceMove(deadlock_peron_track);
                    //switch back to automated mode
                    trainY.TrainStatus = Train.Status.UnloadingOnPlatform;
                    trainY.ForceMoveFlag = false;
                    deadlock_exit_track.Free();

                    //empty exit track was needed - Z is waiting there
                    if (empty_exit_track != null)
                    {
                        //take Z back to its platform track
                        trainZ.ForceMove(empty_peron_track);
                        trainZ.ForceMoveFlag = false;

                        empty_exit_track.Free();
                        empty_exit_track_junction.Free();
                    }

                    if (empty_exit_track_junction != deadlock_junction)
                        deadlock_junction.Free();
                }
        }
        /// <summary>
        /// Method to find empty platform track
        /// </summary>
        /// <returns> Platform track without train, but with acquired reservation </returns>
        public Track GetEmptyPeronTrack()
        {
            foreach (var platform in Platforms)
            {   
                if(platform.TrackTop.TryReserve())
                    return platform.TrackTop;
                if(platform.TrackDown.TryReserve())
                    return platform.TrackDown;
            }
            return null;
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
