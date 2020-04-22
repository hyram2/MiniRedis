using System;
using System.Linq;
using AquirisMiniRedisApi.Application.Interface;
using AquirisMiniRedisApi.Dtos;
using AquirisMiniRedisApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace AquirisMiniRedisApi.Controllers
{
    [ApiController]
    [Route("")]
    public class MiniRedisController : ControllerBase
    {
        private readonly IDbApplication _application;

        public MiniRedisController(IDbApplication application)
        {
            //dependency injection
            _application = application;
        }
        
        /// <summary>
        ///This function is used to redirect the user's requested actions
        /// Examples to use actions:
        /// GET 
        /// /?cmd=Get key returns the value inside the key
        /// /?cmd=Get key "value" returns Ok if the base contains this key element 
        /// SET
        /// /?cmd=Set key "value" returns Ok if set or add this new value
        /// /?cmd=Set key "value" ex number returns Ok if set or add this new value with a lifetime in seconds
        /// DEL
        /// /?cmd=Del key returns the number of deleted values in this key
        /// /?cmd=Del key key2 key3 key4 returns the number of deleted values in those key
        /// /?cmd=Del key "value" returns Ok if delete any key value with this pattern or Null if don't.
        /// DBSIZE
        /// /?cmd=Dbsize returns the number of elements in the base.
        /// INCR
        /// /?cmd=Incr key returns Ok if have any value to increase, otherwise returns an error.
        /// ZADD
        /// /?cmd=ZAdd key score "value" returns Ok if update or add this new value
        /// ZCARD
        /// /?cmd=ZCard key returns the count of elements with this key
        /// ZRANGE
        /// /?cmd=ZRange key start end return all values in the key with they scores
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get(string cmd = "")
        {
            string[] keyValueCommand;
            if (cmd != string.Empty)
            {
                keyValueCommand = cmd.Split(" ");
            }
            else
                return Ok("Server on");

            var parseSucceed = Enum.TryParse<Command>((keyValueCommand[0]??"").ToUpper(),out var command);
            if(!parseSucceed)
                return Ok(_application.Get(cmd).Response);
            
            var keyValues = keyValueCommand.Skip(1).ToArray();

            return command switch
            {
                Command.SET => CmdSet(keyValues),
                Command.GET => CmdGet(keyValues),
                Command.DEL => CmdDelete(keyValues),
                Command.DBSIZE => Ok(_application.DbSize().Response),
                Command.INCR => CmdIncrease(keyValues),
                Command.ZADD => CmdZAdd(keyValues),
                Command.ZCARD => CmdZCard(keyValues),
                Command.ZRANK => CmdZRank(keyValues),
                Command.ZRANGE => CmdZRange(keyValues),
                _ => NotFound()
            };
        }

        #region  --Cmd Commands--
        private ActionResult CmdSet(string[] keyValues)
        {
            
            if (keyValues == null || keyValues?.Length < 2)
                return BadRequest(
                    $"you must need to declare a key and value with quotes(ex:{'"'}your-value{'"'}) after key in the set command");

            ResponseDto response;
            if (!IsAQuote(keyValues[1] ?? ""))
                return BadRequest(
                    $"you must need to declare a value with quotes(ex:{'"'}your-value{'"'}) after key in set command");

            if (keyValues.Length <= 2)
            {
                response = _application.Set(keyValues[0], QuoteRemover(keyValues[1]));
                return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
            }

            if (keyValues[2].ToLower() != "ex")
            {
                response = _application.Set(keyValues[0], QuoteRemover(keyValues[1]));
                return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
            }

            if (keyValues.Length <=3)
                return BadRequest("you must need to declare a number of seconds after ex command");
            
            if(!double.TryParse((keyValues[3] ?? ""), out var time))
                return BadRequest("you must need to declare a REAL number of seconds after ex command"); 
                
            response = _application.Set(keyValues[0],QuoteRemover(keyValues[1]),time);
            return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
        }
        private ActionResult CmdGet(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length==0)
                return BadRequest(
                    $"you must need to declare a key in the get command");
            var response = _application.Get(keyValues[0], keyValues.Length<=1?null:QuoteRemover(keyValues[1]));
            return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
        }
        private ActionResult CmdDelete(string[] keyValues)
        {
            if (keyValues == null) 
                return BadRequest(
                    $"you must need to declare a key in the del command");
            var containsValue = keyValues.Any(x => x.StartsWith('"') && x.EndsWith('"'));
            ResponseDto response;
            if (!containsValue)
            {
                response = _application.Delete(keyValues.Where(x=>x.ToLower() != "del").ToArray());
                return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
            }
            if (keyValues.Length > 2)
            {
                return BadRequest("delete with values accept only one request per del");
            }
            if (keyValues[0].Contains('"'))
            {
                return BadRequest("you need to put your value after a key");
            }
            if (!IsAQuote(keyValues[1]))
                return BadRequest("you must need to declare a value after key in del command");
            
            response = _application.Delete(keyValues[0], QuoteRemover(keyValues[1]));
            return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
            
        }
        private ActionResult CmdIncrease(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length < 1)
                return BadRequest("you must need to declare who is the key to increase"); 
            
            var response = _application.Increase(keyValues[0]);
            return response.Status == StatusCall.Error ? StatusCode(400,response.Response) : Ok(response.Response);
        }
        private ActionResult CmdZAdd(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length < 3)
            {
                return BadRequest("you must need insert all values to add in ZAdd command");
            }

            if (!int.TryParse((keyValues[1] ?? ""), out var score))
            {
                return BadRequest("you must need insert a integer score");
            }

            var response = _application.ZAdd(keyValues[0], score, QuoteRemover(keyValues[2]));
            return response.Status == StatusCall.Error ? StatusCode(400,response.Response) : Ok(response.Response);
        }
        private ActionResult CmdZCard(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length < 1)
                return BadRequest("you must need to declare who is the key to ZCard"); 
            
            var response = _application.ZCard(keyValues[0]);
            return response.Status == StatusCall.Error ? StatusCode(400,response.Response) : Ok(response.Response);
        }
        private ActionResult CmdZRank(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length < 2)
                return BadRequest("you must need insert all values in ZRank command"); 

            var response = _application.ZRank(keyValues[0], QuoteRemover(keyValues[1]));
            return response.Status == StatusCall.Error ? StatusCode(400,response.Response) : Ok(response.Response);
        }
        private ActionResult CmdZRange(string[] keyValues)
        {
            if (keyValues == null || keyValues.Length < 3)
            {
                return BadRequest("you must need insert all values to add in ZRange command");
            }

            if (!int.TryParse((keyValues[1] ?? ""), out var start))
            {
                return BadRequest("you must need insert a integer start value");
            }
            if (!int.TryParse((keyValues[2] ?? ""), out var end))
            {
                return BadRequest("you must need insert a integer end value");
            }
            var response = _application.ZRange(keyValues[0],start,end);
            return response.Status == StatusCall.Error ? Problem(response.Response) : Ok(response.Response);
        }
        #endregion
        
        private static bool IsAQuote(string value) => value.Contains('"');

        private static string? QuoteRemover(string? value) => value?.Replace('"'.ToString(), string.Empty);
    }
}
