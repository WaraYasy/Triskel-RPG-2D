using System;
using System.Collections.Generic;

namespace Triskel.Core
{
    [Serializable]
    public class DiaryData
    {
        public List<DiaryEntry> entries = new List<DiaryEntry>();
    }

    [Serializable]
    public class DiaryEntry
    {
        public string id;
        public int level;
        public string title;
        public string text;
    }
}
