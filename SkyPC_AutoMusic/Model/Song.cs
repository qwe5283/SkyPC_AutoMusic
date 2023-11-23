using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic.Model
{
    public enum NoteKey
    {
        Key100, Key101, Key102, Key103, Key104, Key105, Key106, Key107, Key108, Key109, Key110, Key111, Key112, Key113, Key114,
        Key200, Key201, Key202, Key203, Key204, Key205, Key206, Key207, Key208, Key209, Key210, Key211, Key212, Key213, Key214
    }

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
        //所有节拍
        public List<Beat> Beats { get; set; }
    }

    //单拍
    public class Beat
    {
        public int Time { get; set; }
        public List<NoteKey> Keys { get; set; }
    }
}
