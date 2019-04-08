using System.Threading;

namespace Plus.HabboHotel.Roleplay.Jobs.Farming
{
    internal class respawnTimer
    {
        public respawnTimer(uint id, int x, int y, uint roomid, string type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.roomid = roomid;
            this.type = type;
        }

        private Timer timer;
        private readonly uint id;
        private readonly int x;
        private readonly int y;
        private readonly uint roomid;
        private readonly string type;

        public void startTimer()
        {
            var autoRE = new AutoResetEvent(false);

            TimerCallback timerCB = timerDone;

            timer = new Timer(timerCB, autoRE, 60000, Timeout.Infinite);
        }

        public void timerDone(object info)
        {
            try
            {
                var newFarmingSpot = new FarmingSpot(id, roomid, x, y, type);
                farmingManager.FarmingSpots.TryAdd(id, newFarmingSpot);
                timer.Dispose();
            }
            catch{}
        }
    }
}