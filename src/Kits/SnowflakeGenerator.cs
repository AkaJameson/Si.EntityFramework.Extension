namespace Si.EntityFramework.Extension.Kits
{
    /// <summary>
    /// 增强版雪花ID生成器
    /// </summary>
    public class IdGenerator
    {
        // 基准时间（可根据需要调整）
        private const long Twepoch = 1582136400000L;

        // 各部分位数配置
        private const int WorkerIdBits = 5;
        private const int DatacenterIdBits = 5;
        private const int SequenceBits = 12;

        // 最大值计算
        private const long MaxWorkerId = -1L ^ -1L << WorkerIdBits;
        private const long MaxDatacenterId = -1L ^ -1L << DatacenterIdBits;
        private const long MaxSequence = -1L ^ -1L << SequenceBits;

        // 位移配置
        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private readonly object _lock = new object();
        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        private readonly long _workerId;
        private readonly long _datacenterId;

        public IdGenerator(int workerId, int datacenterId)
        {
            // 参数校验
            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"Worker ID 必须在 0 到 {MaxWorkerId} 之间");

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"Datacenter ID 必须在 0 到 {MaxDatacenterId} 之间");

            _workerId = workerId;
            _datacenterId = datacenterId;
        }
        public string FetchStr() => Fetch().ToString();
        public long Fetch()
        {
            lock (_lock)
            {
                var timestamp = GetCurrentTimestamp();
                // 解决时钟回拨问题（有限等待）
                if (timestamp < _lastTimestamp)
                {
                    var offset = _lastTimestamp - timestamp;
                    if (offset <= 5) // 允许最多5ms的回拨等待
                    {
                        Thread.Sleep((int)(offset << 1));
                        timestamp = GetCurrentTimestamp();
                        if (timestamp < _lastTimestamp)
                        {
                            throw new InvalidOperationException(
                                $"检测到时钟回拨，拒绝生成ID。回拨时间：{offset}ms");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"严重时钟回拨，拒绝服务。回拨时间：{offset}ms");
                    }
                }
                // 同一毫秒内序列号递增
                if (_lastTimestamp == timestamp)
                {
                    _sequence = _sequence + 1 & MaxSequence;
                    if (_sequence == 0) // 当前毫秒序列号用尽
                    {
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else // 新的时间周期重置序列号
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return timestamp - Twepoch << TimestampLeftShift
                       | _datacenterId << DatacenterIdShift
                       | _workerId << WorkerIdShift
                       | _sequence;
            }
        }
        private long WaitNextMillis(long lastTimestamp)
        {
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                Thread.Sleep(0);
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
        private static long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}

