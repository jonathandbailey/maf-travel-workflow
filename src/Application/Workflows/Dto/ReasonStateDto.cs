namespace Application.Workflows.Dto;

using System.Collections.Generic;

public class ReasonState
{
    public string Thought { get; set; } = string.Empty;

    public string NextAction { get; set; } = string.Empty;

    public Dictionary<string, string> Parameters { get; set; } = new();
}




