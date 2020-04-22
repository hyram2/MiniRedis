using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using AquirisMiniRedisApi.Domain.Interface;
using AquirisMiniRedisApi.Dtos;
using AquirisMiniRedisApi.Utils;
using Microsoft.VisualBasic;

namespace AquirisMiniRedisApi.Domain
{
    public class DbSimulation : IDbSimulation
    {
        private readonly AtomicList<MiniRedisData> _atomicData; 
        private AtomicList<MiniRedisData> AtomicData {
            get
            {
                CleanExpiredData();
                return _atomicData;
            }
        }

        public DbSimulation(AtomicList<MiniRedisData> atomicData)
        {
            _atomicData = atomicData;
        }
        
        public bool DbExist()
        {
            return AtomicData != null;
        }

        public (StatusCall, string message) Add(string key, string value, int? score = null, double? time = null)
        {
            if (AtomicData.Any(x => x.Key == key && x.Value == value))
            {
                if (score == null)
                {
                    return (StatusCall.Error, "Error to set a key, you must need to use ZAdd a key with the same value with score update");
                }

                var toRemove= AtomicData.First(x => x.Key == key && x.Value == value);
                AtomicData.Remove(toRemove);
            }

            AtomicData.Add(new MiniRedisData(key,score,value,time));
            OrderData();
            return (StatusCall.Success, "Ok");
        }

        private (StatusCall, string message) Add(MiniRedisData value) => Add(value.Key, value.Value, value.Score, value.Time);
        
        public (StatusCall, string message) Set(string key, string value, int? score, double? time)
        {
            var valuesWithKey = AtomicData.Where(x => x.Key == key);
            var valueTuples = valuesWithKey.ToArray();
            var containsMore = valueTuples.Count() > 1;

            if (containsMore && score == null)
                return (StatusCall.Error,
                    "not allowed to set key when have more than one key without the score attribute");

            //if contains more than once select first with score equals the score inserted, than remove data
            var oldValue = containsMore?valueTuples.FirstOrDefault(x => x.Score == score):valueTuples.FirstOrDefault();
            if(oldValue != null)
                AtomicData.Remove(oldValue);
            //add new value data
            return Add(key, value, score, oldValue?.Time??time);
        }

        public (StatusCall, string message) Get(string key, string? value = null, int? score = null)
         {
            var query = AtomicData.Where(x => x.Key == key);
            var queryCount = query.Count();
            if (queryCount == 0)
                return (StatusCall.Success, "null");
            if (score != null)
                query = query.Where(x => x.Score == score);
            
            //if value inserted exists and contains someone equals the key value pair in the base return OK otherwise return NotExist
            if(value != null) return query.Any(x => x.Value == value) ? (StatusCall.Success, "Ok") : (StatusCall.Success, "Not exist");
            //if value is not inserted, verify if have some more values in that key, than return those elements. 
            return queryCount>1 ? (StatusCall.Success, query.Select(x => x.Score+":"+x.Value).Aggregate("", (current, val) => current + (val + "\n"))) : (StatusCall.Success, query.Select(x => x.Value).First());
        }
        
        public (StatusCall, string message) GetRange(string key, int start, int end)
        {
            var query = AtomicData.Where(x => x.Key == key).OrderBy(x=>x.Score).ToList();
            var queryCount = query.Count;

            var tempStart = start;
            var tempEnd = end;

            //in case of misunderstands, likewise -120918 in a list with 4 elements we make the startTime equals 0.
            if (tempStart + queryCount < 0) tempStart = 0;
            else 
            // verifying if the start or end value are negatives,
            // if they was true, the temporary value is set up to the required indexof.   
            if (tempStart < 0) tempStart += queryCount;
            if (tempEnd < 0) tempEnd += queryCount;
            //skip 'x' elements in the list;
            var tempSkip = query.Skip(tempStart);
            //capture all 'x' elements +1, because take action does not work with zero-base format.
            var tempTake = query.Take(tempEnd + 1);
            //if the start value was negative, use the union action, otherwise uses the intersect action
            var result = start < 0 ? tempSkip.Union(tempTake) : tempSkip.Intersect(tempTake);
            //return the score and the values in one string 
            var response = result.Select(x => $"{x.Score}:{x.Value}").Aggregate("", (current, val) => current + (val + "\n"));
            return (StatusCall.Success,response);
        }

        public (StatusCall, string message) Remove(string[] keys)
        {
            var values = AtomicData.Where(x => keys.Contains(x.Key));
            var valueTuples = values as MiniRedisData[] ?? values.ToArray();
            foreach (var valueTuple in valueTuples)
            {
             AtomicData.Remove(valueTuple);
            }

            return (StatusCall.Success, valueTuples.Count().ToString());
        }

        public (StatusCall, string message) Remove(string key, string value)
        {
            var values = AtomicData.Where(x => x.Key == key && x.Value == value);
            var valueTuples = values as MiniRedisData[] ?? values.ToArray();
            foreach (var valueTuple in valueTuples)
            {
                AtomicData.Remove(valueTuple);
            }
            return (StatusCall.Success, valueTuples.Length<1?"null":"Ok");
        }

        public int DbSize() => AtomicData.Count();
        
        public int Cardinal(string key) => AtomicData.Count(x => x.Key == key);

        public (StatusCall, string message) Increase(string key)
        {
            var values = AtomicData.Where(x => x.Key == key);
            var valueTuples = values as MiniRedisData[] ?? values.ToArray();
          
            foreach (var valueTuple in valueTuples)
            {
                var tuple = valueTuple;
                var success = int.TryParse(tuple.Value, out var result);
                if (!success)
                {
                    continue;
                }
                result++;
                tuple.Value = result.ToString();
                AtomicData.Remove(valueTuple);
                Add(tuple);
            }
            
            return valueTuples.Length>0?(StatusCall.Success, "Ok"):(StatusCall.Error, "not found any value with this key");
        }

        public (StatusCall, string message) GetRank(string key, string value)
        {
            var collection = AtomicData.Where(x => x.Key == key).OrderBy(x => x.Score).ThenBy(x => x.Value);
            if (!collection.Any(x=>x.Value == value)) return (StatusCall.Success, "null");
            var count = collection.TakeWhile(x => x.Value != value).Count();
            return (StatusCall.Success, count.ToString());
        }
        private void CleanExpiredData()
        {
            var actualTime = DateTime.Now.Ticks;
            var expiredData = _atomicData.Where(x => x.Time != null)?.Where(x => x.Time < actualTime);
            _atomicData.RemoveAll(expiredData);
        }

        private void OrderData()
        {
            var temp = AtomicData.OrderBy(x => x.Key).ThenBy(x => x.Score).ThenBy(x=>x.Value);
            AtomicData.Replace(temp);
        }
    }
}