namespace AquirisMiniRedisApi.Domain
{
    public class MiniRedisData
    {
        public string Key { get; set; }
        public int? Score { get; set; }
        public string Value { get; set; }
        public double? Time { get; set; }

        public MiniRedisData(string key, int? score, string value, double? time)
        {
            this.Key = key;
            this.Score = score;
            this.Value = value;
            this.Time = time;
        }
        public MiniRedisData((string key, int? score, string value, double? time) val)
        {
            Key = val.key;
            Score = val.score;
            Value = val.value;
            Time = val.time;
        }
    }
}