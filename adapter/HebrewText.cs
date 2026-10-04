using RTLTMPro;

public static class HebrewText
{
    public static bool ContainsHebrew(string text)
    {
        if (text == null) return false;
        foreach (char c in text) if (c >= '\u0590' && c <= '\u05ff') return true;
        return false;
    }

    // TMP's RTL layout performs the final reversal, with wrapping at runtime.
    public static string Prepare(string text)
    {
        if (!ContainsHebrew(text)) return text;
        var output = new FastStringBuilder(RTLSupport.DefaultBufferSize);
        RTLSupport.FixRTL(text, output, false, true, true);
        output.Reverse();
        return output.ToString();
    }
}
