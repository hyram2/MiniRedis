using AquirisMiniRedisApi.Utils;

namespace AquirisMiniRedisApi.Dtos
{
    public class ResponseDto
    {
        public StatusCall Status { get; set; }
        public string Response { get; set; }

        public ResponseDto((StatusCall status, string response) value)
        {
            Status = value.status;
            Response = value.response;
        }
        public ResponseDto()
        {
        }
    }
}