using System;
using System.Collections.Generic;

namespace _Project.Scripts.Core.Transition
{
    [Serializable]
    public class TransitionData
    {
        public List<TransitionEntry> transitions = new
            List<TransitionEntry>();
    }

    [Serializable]
    public class TransitionEntry
    {
        public string id; 
        public int level;             
        public string text;           
        public float displayTime;   
    }
}