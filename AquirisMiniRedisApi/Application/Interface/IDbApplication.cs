using AquirisMiniRedisApi.Dtos;

namespace AquirisMiniRedisApi.Application.Interface
{
    public interface IDbApplication
    {
        ResponseDto Set(string key, string value, double? time = null);
        ResponseDto DbSize();
        ResponseDto ZCard(string key);
        ResponseDto Get(string key, string? value = null);
        ResponseDto Delete(string key, string value);
        ResponseDto Delete(string[] key);
        ResponseDto Increase(string key);
        ResponseDto ZAdd(string key, int score, string value);
        ResponseDto ZRank(string key, string value);
        ResponseDto ZRange(string key, int start, int end);
    }
}