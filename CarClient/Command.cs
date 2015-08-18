using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarClient
{
    public struct Command
    {
        public Verb Verb { get; set; }
        public int Time { get; set; }

        public Command(Verb verb,int time)
        {
            Verb = verb;
            Time = time;
        }
    }

    public enum Verb
    {
        Stop,
        Fowerd,
        FowerdRight,
        FowerdLeft,
        Back,
        BackRight,
        BackLeft
    }
}
