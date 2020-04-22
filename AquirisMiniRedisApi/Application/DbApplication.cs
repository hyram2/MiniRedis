using System;
using AquirisMiniRedisApi.Application.Interface;
using AquirisMiniRedisApi.Domain.Interface;
using AquirisMiniRedisApi.Dtos;
using AquirisMiniRedisApi.Utils;

namespace AquirisMiniRedisApi.Application
{
    public class DbApplication : IDbApplication
    {
        private IDbSimulation _db;

        public DbApplication(IDbSimulation db)
        {
            _db = db;
        }
        
        public ResponseDto Set(string key, string value, double? time = null)
        {
            if (time == null) return new ResponseDto(_db.Set(key, value, null, null));
            //if have a time to deal, gets the time relative to expire in Milliseconds.
            var now = DateTime.Now;
            double? tempTime = now.AddSeconds((int)time).Ticks;
            return new ResponseDto(_db.Set(key, value, null, tempTime));
        }
        public ResponseDto DbSize() => new ResponseDto
            {
                Response = _db.DbSize().ToString(),
                Status = StatusCall.Success
            };
        public ResponseDto ZCard(string key) => new ResponseDto
        {
            Response = _db.Cardinal(key).ToString(),
            Status = StatusCall.Success
        };
        public ResponseDto Get(string key, string? value = null) => new ResponseDto(_db.Get(key, value));
        public ResponseDto Delete(string key, string value) => new ResponseDto(_db.Remove(key, value));
        public ResponseDto Delete(string[] key) => new ResponseDto(_db.Remove(key));
        public ResponseDto Increase(string key) => new ResponseDto(_db.Increase(key));
        public ResponseDto ZAdd(string key, int score, string value) => new ResponseDto(_db.Add(key,value,score));
        public ResponseDto ZRank(string key, string value) => new ResponseDto(_db.GetRank(key,value));
        public ResponseDto ZRange(string key, int start, int end) => new ResponseDto(_db.GetRange(key,start,end));

    }
}