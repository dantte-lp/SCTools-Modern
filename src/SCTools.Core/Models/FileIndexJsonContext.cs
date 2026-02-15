// Licensed to the SCTools project under the MIT license.

using System.Text.Json.Serialization;

namespace SCTools.Core.Models;

/// <summary>
/// Source-generated JSON serialization context for the file index dictionary.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, FileHashEntry>))]
internal sealed partial class FileIndexJsonContext : JsonSerializerContext;
