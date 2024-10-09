[GenerateSerializer]
[Alias("StreamItem")]
public record StreamItem
{
    [Id(0)]
    public required string FullUrl { get; set; }
}