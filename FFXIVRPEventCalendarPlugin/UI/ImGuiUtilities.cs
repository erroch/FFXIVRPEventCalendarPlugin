//-----------------------------------------------------------------------
// <copyright file="ImGuiUtilities.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.UI
{
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
    }
}
