using System;

namespace System.Threading
{
    // Note: Only visible members have documentation!
    // By MagicMayhem

    /// <summary>
    /// A special timer class which allows you to invoke a method every tick
    /// </summary>
    public class FlexTimer
    {
        Timer ThreadingTimer;
        Action<object> OnElapseMethod;
        Action<object> OnTickMethod;

        object ObjectToReturn;
        long TimeUntilElapse;
        long TimeToDeduct;
        bool InfiniteLoop;

        /// <summary>
        /// The time remaining until the timer elapses
        /// </summary>
        public long TimeRemaining;

        /// <summary>
        /// A special timer class which allows you to invoke a method every tick
        /// </summary>
        /// <param name="OnElapseMethod">The method to call when the timer has finished</param>
        /// <param name="OnTickMethod">The method to call when the timer has ticked</param>
        /// <param name="ObjectToReturn">The object which is returned to both methods (OnElapse, OnTick)</param>
        /// <param name="TimeUntilElapse">The time until the timer finishes</param>
        /// <param name="TimeToDeduct">The time to deduct (also the time when every tick occurs)</param>
        /// <param name="InfiniteLoop">If the timer should never end</param>
        public FlexTimer(Action<object> OnElapseMethod, Action<object> OnTickMethod, object ObjectToReturn,
                         long TimeUntilElapse, long TimeToDeduct, bool InfiniteLoop = false)
        {
            this.OnElapseMethod = OnElapseMethod;
            this.OnTickMethod = OnTickMethod;

            this.ObjectToReturn = ObjectToReturn;
            this.TimeUntilElapse = TimeUntilElapse;
            this.TimeToDeduct = TimeToDeduct;
            this.InfiniteLoop = InfiniteLoop;
            this.TimeRemaining = TimeUntilElapse;

            StartFlexTimer();
        }

        void StartFlexTimer()
        {
            ThreadingTimer = new Timer(FlexTimerTicked, null, TimeToDeduct, TimeToDeduct);
        }

        void FlexTimerTicked(object Object)
        {
            try
            {
                DeductTime();

                if (IsTimerFinished() && !InfiniteLoop)
                {
                    TimerFinishedEvent();
                }
                else
                {
                    OnTickMethod.BeginInvoke(ObjectToReturn, null, null);
                    ThreadingTimer.Change(TimeToDeduct, TimeToDeduct);
                }
            }
            catch { }
        }

        void DeductTime()
        {
            TimeRemaining -= TimeToDeduct;
            TimeUntilElapse -= TimeToDeduct;
        }

        bool IsTimerFinished()
        {
            if (TimeUntilElapse > 0)
                return false;
            else
                return true;
        }

        void TimerFinishedEvent()
        {
            OnElapseMethod.BeginInvoke(ObjectToReturn, null, null);
            StopFlexTimer();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void StopFlexTimer()
        {
            if (ThreadingTimer == null)
                return;

            try
            {
                ThreadingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                ThreadingTimer.Dispose();
                ThreadingTimer = null;
            }
            catch { }
        }
    }
}
