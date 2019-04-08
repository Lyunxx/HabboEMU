using System.Collections;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Rooms.Wired
{
    public interface IWiredCycler
    {
        Queue ToWork { get; set; }

        ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; } 

        bool OnCycle();
    }
}