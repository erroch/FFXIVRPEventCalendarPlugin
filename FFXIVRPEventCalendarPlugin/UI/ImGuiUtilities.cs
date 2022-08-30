//-----------------------------------------------------------------------
// <copyright file="ImGuiUtilities.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.UI
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Numerics;

    using ImGuiNET;

    /// <summary>
    /// A collection of common ImGui functions.
    /// </summary>
    public static class ImGuiUtilities
    {
        /// <summary>
        /// Build a tooltip as the next item in an ImGui interface.
        /// </summary>
        /// <param name="description">The tooltip description.</param>
        public static void BuildToolTip(string description)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
                ImGui.TextUnformatted(description);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// Open a browser window to the provided URL.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public static void OpenBrowser(string url)
        {
            Uri uri = new (url);
            if (!uri.IsFile && !uri.IsUnc && uri.IsAbsoluteUri)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = uri.ToString(),
                    UseShellExecute = true,
                });
            }
        }

        /// <summary>
        /// Calculate vector of widgets.
        /// </summary>
        /// <param name="width1">The first width value.</param>
        /// <param name="width2">The second width value.</param>
        /// <returns>A new Vector2.</returns>
        public static Vector2 CalcWidgetChildFrameVector2(float width1, float width2)
        {
            float width = (width1 + width2) * 1.3f;
            float height = ImGui.GetTextLineHeightWithSpacing() * 1.3f;
            return new Vector2(width, height);
        }
    }
}
