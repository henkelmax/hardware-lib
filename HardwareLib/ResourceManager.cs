using System;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace HardwareLib
{
    public class ResourceManager
    {
        private PerformanceCounter totalCPU;
        private PerformanceCounter[] cpuCores;
        private PerformanceCounter memoryPercentage;
        private PerformanceCounter memoryAvailable;
        private PerformanceCounter networkSent;
        private PerformanceCounter networkReceived;
        private Computer computer;
        private IHardware gpu;
        private IHardware cpu;
        private ISensor gpuLoad;
        private ISensor gpuTemp;
        private ISensor gpuFan;
        private ISensor cpuTemp;
        private int coreCount;
        private long totalMemory;

        public ResourceManager() : this(NetworkDevices.GetNetDevices().Length <= 0
            ? null
            : NetworkDevices.GetNetDevices()[0])
        {
        }

        public ResourceManager(string networkDeviceName)
        {
            totalCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            coreCount = Environment.ProcessorCount;
            cpuCores = new PerformanceCounter[coreCount];
            for (int i = 0; i < coreCount; i++)
            {
                cpuCores[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }

            memoryPercentage = new PerformanceCounter("Memory", "% Committed Bytes In Use", "Memory Used");
            memoryAvailable = new PerformanceCounter("Memory", "Available Bytes", true);

            var search = new ObjectQuery("SELECT * FROM Win32_OperatingSystem ");

            var searcher = new ManagementObjectSearcher(search);
            var results = searcher.Get();

            foreach (var result in results)
            {
                totalMemory = (long) Convert.ToDouble(result["TotalVisibleMemorySize"]) * 1024;
            }

            computer = new Computer()
            {
                GPUEnabled = true,
                CPUEnabled = true
            };
            computer.Open();

            foreach (var dev in computer.Hardware)
            {
                if (dev.HardwareType.Equals(HardwareType.GpuAti) || dev.HardwareType.Equals(HardwareType.GpuNvidia))
                {
                    gpu = dev;
                    foreach (var sens in dev.Sensors)
                    {
                        switch (sens.SensorType)
                        {
                            case SensorType.Load when sens.Name.Contains("Core"):
                                gpuLoad = sens;
                                break;
                            case SensorType.Temperature:
                                gpuTemp = sens;
                                break;
                            case SensorType.Fan:
                                gpuFan = sens;
                                break;
                        }
                    }
                }
                else if (dev.HardwareType.Equals(HardwareType.CPU))
                {
                    cpu = dev;
                    foreach (var sens in dev.Sensors)
                    {
                        if (sens.SensorType.Equals(SensorType.Temperature))
                        {
                            if (sens.Name.Contains("Package"))
                            {
                                cpuTemp = sens;
                            }
                        }
                    }
                }
            }

            if (networkDeviceName != null)
            {
                SetNetDevice(networkDeviceName);
            }
        }

        public void Close()
        {
            computer.Close();
            totalCPU.Close();
            foreach (var core in cpuCores)
            {
                core.Close();
            }

            memoryPercentage.Close();
            memoryAvailable.Close();
        }

        public float GetTotalCpuUsage()
        {
            return totalCPU.NextValue();
        }

        public float[] GetCoreUsages()
        {
            var data = new float[coreCount];
            for (int i = 0; i < coreCount; i++)
            {
                data[i] = cpuCores[i].NextValue();
            }

            return data;
        }

        public float GetMemoryUsage()
        {
            return memoryPercentage.NextValue();
        }

        public long GetMemoryAvailable()
        {
            return Convert.ToInt64(memoryAvailable.NextValue());
        }

        public long GetMemoryUsed()
        {
            return totalMemory - GetMemoryAvailable();
        }

        public long GetTotalMemory()
        {
            return totalMemory;
        }

        public float GetGpuUsage()
        {
            if (gpu == null || gpuLoad == null)
            {
                return 0;
            }

            gpu.Update();
            if (gpuLoad.Value != null)
            {
                return (float) gpuLoad.Value;
            }

            return 0;
        }

        public int GetGpuFanSpeed()
        {
            if (gpu == null || gpuFan == null)
            {
                return 0;
            }

            gpu.Update();
            if (gpuFan.Value != null)
            {
                return (int) gpuFan.Value;
            }

            return 0;
        }

        public float GetGpuTemperature()
        {
            if (gpu == null || gpuTemp == null)
            {
                return 0;
            }

            gpu.Update();
            if (gpuTemp.Value != null)
            {
                return (float) gpuTemp.Value;
            }

            return 0;
        }

        public float GetCpuTemperature()
        {
            if (cpu == null || cpuTemp == null)
            {
                return 0;
            }

            cpu.Update();
            if (cpuTemp.Value != null)
            {
                return (float) cpuTemp.Value;
            }

            return 0;
        }

        public int GetCpuFanSpeed()
        {
            return 0;
        }

        public float GetNetworkBytesSent()
        {
            return networkSent.NextValue();
        }

        public float GetNetworkBytesReceived()
        {
            return networkReceived.NextValue();
        }

        public async Task<object> GetResourceData(object obj)
        {
            if (obj != null && obj is string)
            {
                SetNetDevice((string) obj);
            }

            return new ResourceData
            {
                CpuTotalUsage = GetTotalCpuUsage(),
                CpuTemperature = GetCpuTemperature(),
                CpuFanSpeed = GetCpuFanSpeed(),
                CpuCoreUsages = GetCoreUsages(),
                TotalMemory = GetTotalMemory(),
                UsedMemory = GetMemoryUsed(),
                GpuUsage = GetGpuUsage(),
                GpuTemperature = GetGpuTemperature(),
                GpuFanSpeed = GetGpuFanSpeed(),
                NetworkSent = GetNetworkBytesSent(),
                NetworkReceived = GetNetworkBytesReceived()
            };
        }

        public async Task<object> Test(object obj)
        {
            return obj;
        }

        public async Task<object> GetCoreCount(object obj)
        {
            return coreCount;
        }

        public async Task<object> SetNetworkDevice(object obj)
        {
            SetNetDevice((string) obj);
            return null;
        }

        public void SetNetDevice(string name)
        {
            networkSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);
            networkReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
        }
    }
}