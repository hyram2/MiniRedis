using AquirisMiniRedisApi.Utils;

namespace AquirisMiniRedisApi.Domain.Interface
{
    public interface IDbSimulation
    {
        bool DbExist();
        (StatusCall, string message) Add(string key, string value, int? score = null, double? time = null);
        (StatusCall, string message) Set(string key, string value, int? score = null, double? time = null);
        (StatusCall, string message) Get(string key, string? value = null, int? score = null);
        (StatusCall, string message) GetRange(string key, int start, int end);
        (StatusCall, string message) Remove(string[] keys);
        (StatusCall, string message) Remove(string key, string value);
        (StatusCall, string message) Increase(string key);
        (StatusCall, string message) GetRank(string key, string value);
        int DbSize();
        int Cardinal(string key);
    }
}
