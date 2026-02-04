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
        public string title;          // Título de la transición (ej: "Victoria", "Nivel Completado", "Has Muerto")
        public string text;
        public float displayTime;
    }
}