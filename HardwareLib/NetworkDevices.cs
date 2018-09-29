using System.Diagnostics;
using System.Threading.Tasks;

namespace HardwareLib
{
    public class NetworkDevices
    {
        public NetworkDevices()
        {
            
        }
        
        public async Task<object> GetNetworkDevices(object obj)
        {
            return GetNetDevices();
        }

        public static string[] GetNetDevices()
        {
            return new PerformanceCounterCategory("Network Interface").GetInstanceNames();
        }
    }
}