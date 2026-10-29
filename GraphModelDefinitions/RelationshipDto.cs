using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DotNetSolutionAnalyzer.GraphModelDefinitions
{
    public class RelationshipDto
    {
        [JsonPropertyName("SourceId")]
        public string SourceId { get; set; } = string.Empty;

        [JsonPropertyName("TargetId")]
        public string TargetId { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;
    }
}
