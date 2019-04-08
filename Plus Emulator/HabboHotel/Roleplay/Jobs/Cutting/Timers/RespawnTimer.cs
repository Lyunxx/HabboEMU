#region Librarys

using System.Threading;

#endregion

namespace Plus.HabboHotel.Roleplay.Jobs.Cutting
{
    internal class respawnTimer
    {
        #region Constructor

        public respawnTimer(uint id, int x, int y, uint roomid, string data)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.roomid = roomid;
            this.data = data;
        }

        #endregion

        #region Declares

        private Timer timer;

        private readonly uint id;
        private readonly int x;
        private readonly int y;
        private readonly uint roomid;
        private readonly string data;

        #endregion

        #region Methods

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
                var newTree = new Tree(id, roomid, x, y, data);
                woodManager.Trees.TryAdd(id, newTree);
                timer.Dispose();
            }
            catch
            { }
        }

        #endregion
    }
}