using System.Text.Json.Serialization;

namespace DotNetSolutionAnalyzer.GraphModelDefinitions
{
    // ==========================================
    // 1. Graph Model Definitions
    // ==========================================
    public class CodeGraph
    {
        [JsonPropertyName("Nodes")]
        public List<NodeDto> Nodes { get; set; } = [];

        [JsonPropertyName("Relationships")]
        public List<RelationshipDto> Relationships { get; set; } = [];
    }

}
