using System;

namespace Plus.HabboHotel.Support
{
    /// <summary>
    /// Struct ModerationBan
    /// </summary>
    public class ModerationBan
    {
        public string Value;
        public double Expire;
        public string Reason;
        public ModerationBanType Type;

        /// <summary>
        /// The moderation ban constructor.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Value"></param>
        /// <param name="Reason"></param>
        /// <param name="Expire"></param>
        public ModerationBan(ModerationBanType Type, string Value, string Reason, double Expire)
        {
            this.Type = Type;
            this.Value = Value;
            this.Reason = Reason;
            this.Expire = Expire;
        }

        /// <summary>
        /// Check if timestamp for ban is expired.
        /// </summary>
        public bool Expired
        {
            get
            {
                if (Plus.GetUnixTimeStamp() >= Expire)
                    return true;
                return false;
            }
        }
    }
}