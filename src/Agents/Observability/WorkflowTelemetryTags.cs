using System.Diagnostics;

namespace Application.Observability;

public static class WorkflowTelemetryTags
{
    private const string Prefix = "workflow";

    public const string Node = Prefix + ".node";
    public const string ArtifactKey = Prefix + ".artifact_key";

    private const string InputPreview = Prefix + ".input.preview";
    public const string InputNodePreview = Prefix + ".input.node.preview";
    public const string OutputNodePreview = Prefix + ".output.node.preview";
    private const string InputLength = Prefix + ".input.length";
    private const string InputTruncated = Prefix + ".input.truncated";

    private const string OutputPreview = Prefix + ".output.preview";
    private const string OutputLength = Prefix + ".output.length";
    private const string OutputTruncated = Prefix + ".output.truncated";

    private const int DefaultPreviewLength = 2000;

    public static void SetInputPreview(Activity? activity, string? value, int maxPreviewLength = DefaultPreviewLength)
    {
        if (activity == null) return;

        if (value == null)
        {
            activity.SetTag(InputPreview, string.Empty);
            activity.SetTag(InputLength, 0);
            activity.SetTag(InputTruncated, false);
            return;
        }

        activity.SetTag(InputLength, value.Length);

        var truncated = value.Length > maxPreviewLength;

        var preview = truncated ? value.Substring(0, maxPreviewLength) : value;

        activity.SetTag(InputPreview, preview);
        activity.SetTag(InputTruncated, truncated);
    }

    public static void Preview(Activity? activity, string tag, string value, int maxPreviewLength = DefaultPreviewLength)
    {
        if (activity == null) return;

        activity.SetTag(InputLength, value.Length);

        var truncated = value.Length > maxPreviewLength;

        var preview = truncated ? value.Substring(0, maxPreviewLength) : value;

        if (truncated)
        {
            preview += " -[Truncated]";
        }

        activity.SetTag(tag, preview);
        
    }

    public static void SetOutputPreview(Activity? activity, string? value, int maxPreviewLength = DefaultPreviewLength)
    {
        if (activity == null) return;

        if (value == null)
        {
            activity.SetTag(OutputPreview, string.Empty);
            activity.SetTag(OutputLength, 0);
            activity.SetTag(OutputTruncated, false);
            return;
        }

        activity.SetTag(OutputLength, value.Length);

        var truncated = value.Length > maxPreviewLength;

        var preview = truncated ? value.Substring(0, maxPreviewLength) : value;

        activity.SetTag(OutputPreview, preview);
        activity.SetTag(OutputTruncated, truncated);
    }
}