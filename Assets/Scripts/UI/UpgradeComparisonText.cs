using System;

public static class UpgradeComparisonText
{
    private const string Separator = " -> ";

    public static string HighlightNextValue(string preview)
    {
        if (!preview.Contains('\n'))
            return HighlightLine(preview);

        string[] lines = preview.Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = HighlightLine(lines[i]);

        return string.Join("\n", lines);
    }

    private static string HighlightLine(string preview)
    {
        int separator = preview.IndexOf(Separator, StringComparison.Ordinal);
        if (separator < 0)
            return preview;

        int nextValueStart = separator + Separator.Length;
        return preview.Insert(nextValueStart, "<color=#BBF879>") + "</color>";
    }
}
