using Plus.Configuration;

namespace Plus.HabboHotel.Misc
{
    /// <summary>
    /// Class CrossdomainPolicy.
    /// </summary>
    internal static class CrossDomainPolicy
    {
        internal static byte[] XmlPolicyBytes;

        internal static void Set()
        {
            XmlPolicyBytes = Plus.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1232-30008\" />\r\n" +
                   "</cross-domain-policy>\x0");

            /*XmlPolicyBytes = Plus.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n<cross-domain-policy>\r\n<allow-access-from domain=\"*\" to-ports=\"" +
            ConfigurationData.Data["game.tcp.port"] + "\" />\r\n</cross-domain-policy>\0");*/
        }
    }
}