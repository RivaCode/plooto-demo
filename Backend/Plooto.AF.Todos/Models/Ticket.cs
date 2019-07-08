using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Plooto.AF.Todos.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Ticket
    {
        [Key]
        public string Id { get; set; }

        [IsSearchable, Analyzer(AnalyzerName.AsString.EnLucene)]
        public string Description { get; set; }

        [IsFilterable, IsSortable]
        public bool? Completed { get; set; }

        [IsSearchable, IsFilterable, IsFacetable]
        public string[] Tags { get; set; }

        [IsSortable, IsFilterable]
        public DateTime? Created { get; set; }

        [IsSortable, IsFilterable]
        public DateTime? LastUpdate { get; set; }
    }

}
