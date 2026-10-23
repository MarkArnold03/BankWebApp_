using Microsoft.AspNetCore.Html;

namespace BankWebApp.Helpers
{
    /// <summary>Inline 24×24 line icons, drawn with currentColor.</summary>
    public static class Icons
    {
        private static readonly Dictionary<string, string> Paths = new()
        {
            ["overview"] = "<rect x=\"3\" y=\"3\" width=\"7.5\" height=\"9\" rx=\"2\"/><rect x=\"13.5\" y=\"3\" width=\"7.5\" height=\"5\" rx=\"2\"/><rect x=\"13.5\" y=\"11\" width=\"7.5\" height=\"10\" rx=\"2\"/><rect x=\"3\" y=\"15\" width=\"7.5\" height=\"6\" rx=\"2\"/>",
            ["transfer"] = "<path d=\"M7 4 3 8l4 4\"/><path d=\"M3 8h14\"/><path d=\"m17 12 4 4-4 4\"/><path d=\"M21 16H7\"/>",
            ["card"] = "<rect x=\"2.5\" y=\"5\" width=\"19\" height=\"14\" rx=\"2.5\"/><path d=\"M2.5 10h19\"/><path d=\"M6.5 15h4\"/>",
            ["loan"] = "<path d=\"M3 10.5 12 4l9 6.5\"/><path d=\"M5 10v9h14v-9\"/><path d=\"M10 19v-5h4v5\"/>",
            ["user"] = "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21a8 8 0 0 1 16 0\"/>",
            ["chevron"] = "<path d=\"m9 5 7 7-7 7\"/>",
            ["chevron-down"] = "<path d=\"m6 9 6 6 6-6\"/>",
            ["back"] = "<path d=\"m15 5-7 7 7 7\"/>",
            ["in"] = "<path d=\"M17 7 7 17\"/><path d=\"M16 17H7V8\"/>",
            ["out"] = "<path d=\"M7 17 17 7\"/><path d=\"M8 7h9v9\"/>",
            ["cash"] = "<rect x=\"2.5\" y=\"6\" width=\"19\" height=\"12\" rx=\"2\"/><circle cx=\"12\" cy=\"12\" r=\"2.5\"/><path d=\"M6 9.5v5M18 9.5v5\"/>",
            ["pension"] = "<path d=\"M12 3v3\"/><path d=\"M5 21v-7a7 7 0 0 1 14 0v7\"/><path d=\"M3 21h18\"/>",
            ["shield"] = "<path d=\"M12 3 4.5 6v5.5c0 4.4 3.1 8.2 7.5 9.5 4.4-1.3 7.5-5.1 7.5-9.5V6L12 3Z\"/>",
            ["interest"] = "<path d=\"M19 5 5 19\"/><circle cx=\"7\" cy=\"7\" r=\"2.5\"/><circle cx=\"17\" cy=\"17\" r=\"2.5\"/>",
            ["home"] = "<path d=\"M3 11 12 4l9 7\"/><path d=\"M5 10v10h14V10\"/>",
            ["repeat"] = "<path d=\"m17 2 4 4-4 4\"/><path d=\"M3 11V9a3 3 0 0 1 3-3h15\"/><path d=\"m7 22-4-4 4-4\"/><path d=\"M21 13v2a3 3 0 0 1-3 3H3\"/>",
            ["plus"] = "<path d=\"M12 5v14M5 12h14\"/>",
            ["logout"] = "<path d=\"M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3\"/><path d=\"m10 8-4 4 4 4\"/><path d=\"M6 12h10\"/>",
            ["moon"] = "<path d=\"M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5Z\"/>",
            ["sun"] = "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4\"/>",
            ["lock"] = "<rect x=\"5\" y=\"10.5\" width=\"14\" height=\"10\" rx=\"2\"/><path d=\"M8 10.5V7a4 4 0 0 1 8 0v3.5\"/>",
            ["check"] = "<path d=\"m5 12.5 4.5 4.5L19 7.5\"/>",
            ["info"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v5\"/><path d=\"M12 7.5v.01\"/>",
            ["mail"] = "<rect x=\"3\" y=\"5\" width=\"18\" height=\"14\" rx=\"2\"/><path d=\"m3 7 9 6 9-6\"/>",
            ["bank"] = "<path d=\"M3 9.5 12 4l9 5.5\"/><path d=\"M5 10v8M9.7 10v8M14.3 10v8M19 10v8\"/><path d=\"M3 20.5h18\"/>",
            ["search"] = "<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-4-4\"/>",
            ["users"] = "<circle cx=\"9\" cy=\"8\" r=\"3.5\"/><path d=\"M2.5 20a6.5 6.5 0 0 1 13 0\"/><path d=\"M16 4.5a3.5 3.5 0 0 1 0 7\"/><path d=\"M18 14.5a6.5 6.5 0 0 1 3.5 5.5\"/>",
            ["deposit"] = "<path d=\"M12 4v12\"/><path d=\"m7 11 5 5 5-5\"/><path d=\"M4 20h16\"/>",
            ["withdraw"] = "<path d=\"M12 16V4\"/><path d=\"m7 9 5-5 5 5\"/><path d=\"M4 20h16\"/>",
            ["globe"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M3 12h18\"/><path d=\"M12 3a14 14 0 0 1 0 18a14 14 0 0 1 0-18\"/>",
            ["key"] = "<circle cx=\"8\" cy=\"15\" r=\"4\"/><path d=\"m11 12 8.5-8.5\"/><path d=\"m17 6 2.5 2.5\"/><path d=\"m14.5 8.5 2 2\"/>",
            ["accounts"] = "<rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M7 9h10M7 13h6\"/>",
            ["menu"] = "<path d=\"M4 7h16M4 12h16M4 17h16\"/>",
        };

        public static IHtmlContent Svg(string name, int size = 20, string? cssClass = null)
        {
            var body = Paths.TryGetValue(name, out var p) ? p : Paths["info"];
            var cls = cssClass == null ? "" : $" class=\"{cssClass}\"";
            return new HtmlString(
                $"<svg{cls} width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.7\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{body}</svg>");
        }
    }
}
