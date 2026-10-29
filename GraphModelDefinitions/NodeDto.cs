using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotNetSolutionAnalyzer.GraphModelDefinitions
{
    public class NodeDto
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("Label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
