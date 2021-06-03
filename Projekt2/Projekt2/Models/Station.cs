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
        public bool Go { get; set; }

        public Thread stationManager;
        public List<Thread> trainThreads = new List<Thread>();
        public TimeSpan arrivalTime = new TimeSpan(0,0,0,0,200);
        public TimeSpan overTime = new TimeSpan(0,0,0,2);

        public Station(int platforms, int tracks, int trains, int entryTracks)
        {
            // tu będziemy tworzyć najpierw  junctiony 
            // potem tracki 
            // potem platformy 
            // a trainy to na początku symulacji postawimy na trackach wiec nie tu
            // station magnager też do stworzenia 
        }
        public void StartSimulation()
        {
            // no i tu zaczynamy symulacje czyli stawaimy pociągi na torach i lista wątków pociągów
        }
        public void EndSimulation()
        {
            // tu kończymy symulacje, wszystkie wątki killujemy itd. 
        }
        public void GenerateTrain()
        {
            // tu generujemy losowo jakiś pociąg na torze jeśli jest wolne więcej niż 1
        }
        public void StationManaging()
        {
            // tu generujemy losowo jakiś pociąg na torze jeśli jest wolne więcej niż 1
        }
    }
}
