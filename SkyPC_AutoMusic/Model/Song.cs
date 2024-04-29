using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic.Model
{
    public enum NoteKey
    {
        _1Key0, _1Key1, _1Key2, _1Key3, _1Key4, _1Key5, _1Key6, _1Key7, _1Key8, _1Key9, _1Key10, _1Key11, _1Key12, _1Key13, _1Key14,
        _2Key0, _2Key1, _2Key2, _2Key3, _2Key4, _2Key5, _2Key6, _2Key7, _2Key8, _2Key9, _2Key10, _2Key11, _2Key12, _2Key13, _2Key14
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
