using AquirisMiniRedisApi.Utils;

namespace AquirisMiniRedisApi.Dtos
{
    public class CommandDto
    {
        //se der tempo crio 1 para cada, e faço uma fila para cada tipo de request
        public Command Command { get; set; } 
        public string Key { get; set; }
        public string Value { get; set; }
        public int Score { get; set; }
        public double Time { get; set; }
    }
}