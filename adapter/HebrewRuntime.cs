using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

public static class HebrewRuntime
{
    private sealed class State
    {
        internal bool active;
        internal bool rtl;
        internal HorizontalAlignmentOptions alignment;
        internal TMP_FontAsset font;
        internal FontStyles style;
        internal List<OTL_FeatureTag> fontFeatures;
        internal List<OTL_FeatureTag> hebrewFeatures;
        internal Vector4 margin;
    }
    private static readonly ConditionalWeakTable<TMP_Text, State> states = new ConditionalWeakTable<TMP_Text, State>();
    private static readonly Dictionary<string, TMP_FontAsset> hebrewFonts = new Dictionary<string, TMP_FontAsset>();
    private static bool logged;

    public static string Prepare(TMP_Text label, string source, ref int start, ref int length)
    {
        if (source == null) return source;
        int safeStart = Math.Max(0, Math.Min(start, source.Length));
        string text = source.Substring(safeStart, Math.Max(0, Math.Min(length, source.Length - safeStart)));
        State state = states.GetOrCreateValue(label);
        if (!HebrewText.ContainsHebrew(text))
        {
            if (state.active)
            {
                label.isRightToLeftText = state.rtl;
                label.horizontalAlignment = state.alignment;
                label.font = state.font;
                label.fontStyle = state.style;
                label.fontFeatures = state.fontFeatures;
                label.margin = state.margin;
                state.active = false;
            }
            return source;
        }
        try
        {
            if (!state.active)
            {
                state.rtl = label.isRightToLeftText;
                state.alignment = label.horizontalAlignment;
                state.font = label.font;
                state.style = label.fontStyle;
                state.fontFeatures = new List<OTL_FeatureTag>(label.fontFeatures);
                state.hebrewFeatures = new List<OTL_FeatureTag>(state.fontFeatures);
                // TMP applies Hebrew punctuation kerning with incorrect RTL
                // placement (e.g. the final resh in "שעבר." overlaps "אני").
                // Keep natural glyph advances until TMP supports these pairs.
                state.hebrewFeatures.Remove(OTL_FeatureTag.kern);
                state.margin = label.margin;
                state.active = true;
            }
            label.font = GetFont(state.font);
            label.fontFeatures = state.hebrewFeatures;
            label.fontStyle = state.style;
            if (state.font != null && state.font.name.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) >= 0)
                label.fontStyle |= FontStyles.Italic;
            label.isRightToLeftText = true;
            if (state.alignment == HorizontalAlignmentOptions.Left)
                label.horizontalAlignment = HorizontalAlignmentOptions.Right;
            // These settings labels share horizontal space with a control. Reserve
            // its area rather than allowing right-aligned Hebrew underneath it.
            Vector4 margin = state.margin;
            if (text == "סיוע בתפירה" || text == "כלי סיבוב אביזרים")
            {
                Toggle toggle = label.GetComponentInParent<Toggle>();
                float inset = label.fontSize * 2f;
                if (toggle != null && toggle.targetGraphic != null)
                {
                    var corners = new Vector3[4];
                    toggle.targetGraphic.rectTransform.GetWorldCorners(corners);
                    float left = label.rectTransform.InverseTransformPoint(corners[0]).x;
                    inset = Mathf.Max(inset, label.rectTransform.rect.xMax - left + label.fontSize * 0.4f);
                }
                margin.z = Mathf.Max(margin.z, inset);
            }
            else if (text == "עוצמת קול ראשית" || text == "עוצמת מוזיקה")
                margin.z += label.fontSize * 0.5f;
            label.margin = margin;
            string result = HebrewText.Prepare(text);
            start = 0;
            length = result.Length;
            if (!logged) { Debug.Log("Dressmaker Hebrew: RTL renderer and Hebrew font active"); logged = true; }
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError("Dressmaker Hebrew: " + ex);
            return source;
        }
    }

    private static TMP_FontAsset GetFont(TMP_FontAsset original)
    {
        string name = original == null ? "" : original.name;
        string filename = "DavidLibre-Medium.ttf";
        if (name.IndexOf("goudy", StringComparison.OrdinalIgnoreCase) >= 0)
            filename = "Bellefair-Regular.ttf";
        else if (name.IndexOf("Newsreader", StringComparison.OrdinalIgnoreCase) >= 0)
            filename = "FrankRuhlLibre-400.ttf";
        else if (name.IndexOf("Freude", StringComparison.OrdinalIgnoreCase) >= 0)
            filename = "PlaypenSansHebrew-600.ttf";
        else if (name.IndexOf("Cantora", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 name.IndexOf("Amaranth-Bold", StringComparison.OrdinalIgnoreCase) >= 0)
            filename = "Alef-Bold.ttf";
        else if (name.IndexOf("Amaranth", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 name.IndexOf("LiberationSans", StringComparison.OrdinalIgnoreCase) >= 0)
            filename = "Alef-Regular.ttf";
        TMP_FontAsset asset;
        if (!hebrewFonts.TryGetValue(filename, out asset))
        {
            // On macOS Application.dataPath is Contents, while assemblies live
            // under Contents/Resources/Data/Managed. Resolve beside this DLL.
            string path = Path.Combine(Path.GetDirectoryName(typeof(HebrewRuntime).Assembly.Location), "DressmakerHebrew-" + filename);
            if (!File.Exists(path)) throw new FileNotFoundException("Hebrew font missing", path);
            var font = new Font(path);
            UnityEngine.Object.DontDestroyOnLoad(font);
            asset = TMP_FontAsset.CreateFontAsset(font);
            asset.name = "Dressmaker Hebrew " + filename;
            UnityEngine.Object.DontDestroyOnLoad(asset);
            if (!asset.HasCharacter('א', true, true)) throw new Exception("Font has no Hebrew glyphs: " + filename);
            hebrewFonts.Add(filename, asset);
            // Bellefair lacks Hebrew gershayim (U+05F4); use the book face for it.
            if (filename == "Bellefair-Regular.ttf")
                asset.fallbackFontAssetTable = new List<TMP_FontAsset> { GetFont(null) };
            Debug.Log("Dressmaker Hebrew font: " + name + " -> " + filename);
        }
        return asset;
    }
}
