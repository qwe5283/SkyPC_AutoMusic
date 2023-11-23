using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_Auto_Music
{
    public class Song
    {
        public string name { get; set; }
        public string author { get; set; }
        public string transcribedBy { get; set; }
        public bool isComposed { get; set; }
        public int bpm { get; set; }
        public int bitsPerPage { get; set; }
        public int pitchLevel { get; set; }
        public bool isEncrypted { get; set; }
        public List<SongNote> songNotes { get; set; }
    }

    public class SongNote
    {
        public int time { get; set; }
        public string key { get; set; }
    }
}
