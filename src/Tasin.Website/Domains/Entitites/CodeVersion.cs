﻿using System.ComponentModel.DataAnnotations;

namespace Tasin.Website.Domains.Entitites
{
    public class CodeVersion
    {
        [Key]
        public int ID { get; set; }
        public required string Prefix { get; set; }
        public int VersionIndex { get; set; }
        public string? Type { get; set; }
        public string? Note { get; set; }
    }
}
