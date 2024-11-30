using System.Net.NetworkInformation;

namespace SupportHub.Helpers 
{
    public static class PingHelper
    {
        public static bool PingHost(string hostUri, int timeout = 2000)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(hostUri, timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
