using Prism.Events;
using SkyPC_AutoMusic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic.Event
{
    public class SongSwitchEvent : PubSubEvent<Player>
    {
    }
}
