using System.Text.Json.Serialization;

namespace RobotCleaner.RobotCleaner
{
    public class InputData
    {
        [JsonPropertyName("map")]
        public string[][] Map { get; set; }
        [JsonPropertyName("start")]
        public Position Start { get; set; }
        [JsonPropertyName("commands")]
        public List<string> Commands { get; set; }
        [JsonPropertyName("battery")]
        public int Battery { get; set; }
    }
}
