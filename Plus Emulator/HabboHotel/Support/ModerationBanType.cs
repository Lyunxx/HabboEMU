namespace Plus.HabboHotel.Support
{
    /// <summary>
    /// Enum ModerationBanType
    /// </summary>
    public enum ModerationBanType
    {
        IP,
        MACHINE,
        USERNAME
    }

    /// <summary>
    /// The ban type unility.
    /// </summary>
    public static class BanTypeUtility
    {
        public static ModerationBanType GetModerationBanType(string Type)
        {
            switch (Type)
            {
                default:
                case "user":
                    return ModerationBanType.USERNAME;
                case "ip":
                    return ModerationBanType.IP;
                case "machine":
                    return ModerationBanType.MACHINE;
            }
        }

        public static string FromModerationBanType(ModerationBanType Type)
        {
            switch (Type)
            {
                default:
                case ModerationBanType.USERNAME:
                    return "user";
                case ModerationBanType.IP:
                    return "ip";
                case ModerationBanType.MACHINE:
                    return "machine";
            }
        }
    }
}