namespace Si.EntityFramework.Extension.Kits
{
    /// <summary>
    /// 高性能雪花算法实现
    /// </summary>
    public class IdGenerator
    {
        private long Twepoch;
        private const int WorkerIdBits = 5;
        private const int DatacenterIdBits = 5;
        private const int SequenceBits = 12;

        private const long MaxWorkerId = -1L ^ -1L << WorkerIdBits;
        private const long MaxDatacenterId = -1L ^ -1L << DatacenterIdBits;
        private const long SequenceMask = -1L ^ -1L << SequenceBits;

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;
        private readonly long _workerId;
        private readonly long _datacenterId;
        public IdGenerator(int workerId, int datacenterId)
        {
            try
            {
                Twepoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (workerId > MaxWorkerId || workerId < 0)
                {
                    throw new ArgumentException($"工作id不能大于最大工作id:{MaxWorkerId}或者小于0");
                }

                if (datacenterId > MaxDatacenterId || datacenterId < 0)
                {
                    throw new ArgumentException($"数据中心id不能大于最大数据中心id:{MaxDatacenterId} 或者小于0");
                }
                _workerId = workerId;
                _datacenterId = datacenterId;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("SnowFlakeIdGenerator初始化失败", ex.ToString());
            }
        }
        public string FetchStr() => Fetch().ToString();
        public long Fetch()
        {
            var timestamp = TimeGen();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException($"时间被回拨了，禁止产生id{_lastTimestamp - timestamp} milliseconds");
            }
            if (_lastTimestamp == timestamp)
            {
                long newSequence = 0;
                do
                {
                    newSequence = _sequence + 1 & SequenceMask;
                    if (newSequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                } while (Interlocked.CompareExchange(ref _sequence, newSequence, _sequence) != _sequence);
            }
            else
            {
                _sequence = 0;
            }
            _lastTimestamp = timestamp;
            return timestamp - Twepoch << TimestampLeftShift |
                   _datacenterId << DatacenterIdShift |
                   _workerId << WorkerIdShift |
                   _sequence;

        }
        private long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }
        private long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
