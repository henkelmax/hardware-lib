namespace HardwareLib
{
    public class ResourceData
    {
        public float CpuTotalUsage { get; set; }
        public float CpuTemperature { get; set; }
        public float CpuFanSpeed { get; set; }
        public float[] CpuCoreUsages { get; set; }
        public long TotalMemory { get; set; }
        public long UsedMemory { get; set; }
        public float GpuUsage { get; set; }
        public float GpuTemperature { get; set; }
        public long GpuTotalMemory { get; set; }
        public long GpuUsedMemory { get; set; }
        public int GpuFanSpeed { get; set; }
        public int GpuFanSpeedPercentage { get; set; }
        public float NetworkSent { get; set; }
        public float NetworkReceived { get; set; }
    }
}