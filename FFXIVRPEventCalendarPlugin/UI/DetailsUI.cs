//-----------------------------------------------------------------------
// <copyright file="DetailsUI.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;

    using Dalamud.Game.ClientState;
    using Dalamud.Game.Gui;
    using Dalamud.Interface;
    using Dalamud.Interface.Components;
    using Dalamud.IoC;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;

    using ImGuiNET;

    /// <summary>
    /// The user interface for displaying event details.
    /// </summary>
    public class DetailsUI : IDisposable
    {
        private readonly Configuration configuration;
        private bool disposedValue;
        private bool visible = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsUI"/> class.
        /// </summary>
        /// <param name="configuration">The Dalamud configuration.</param>
        public DetailsUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DetailsUI"/> class.
        /// </summary>
        ~DetailsUI()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Gets or sets the roleplay event to currently display.
        /// </summary>
        /// <remarks>
        /// Setting to null will hide the window.
        /// </remarks>
        public RPEvent? RPEvent { get; set; }

        /// <summary>
        /// Gets or sets the list of event categories.
        /// </summary>
        public List<EventCategoryInfo>? EventCategories { get; set; }

        /// <summary>
        /// Gets or sets the list of ESRBRatings.
        /// </summary>
        public List<ESRBRatingInfo>? ESRBRatings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the PluginUI is visible.
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        /// <summary>
        /// Draw the details pane.
        /// </summary>
        public void Draw()
        {
            if (!this.visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(320, 600), ImGuiCond.Appearing);
            ImGui.SetNextWindowSizeConstraints(new Vector2(320, 600), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Event Details", ref this.visible, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (this.RPEvent == null)
                {
                    ImGui.Text("No event details found.");
                }
                else
                {
                    ImGui.Text(this.RPEvent.EventName);
                    ImGui.Spacing();

                    if (ImGui.BeginChild("###eventDetails", new Vector2(-1, -1), true))
                    {
                        if (!string.IsNullOrWhiteSpace(this.RPEvent.EventURL))
                        {
                            ImGui.Text("Website:");
                            ImGui.Separator();
                            ImGui.Text(this.RPEvent.EventURL);
                            ImGui.SameLine();
                            if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe))
                            {
                                ImGuiUtilities.OpenBrowser(this.RPEvent.EventURL);
                            }

                            ImGui.Spacing();
                        }

                        ImGui.Text("When:");
                        ImGui.Separator();
                        ImGui.Text($"{this.RPEvent.LocalStartTime.ToShortDateString()} {this.RPEvent.LocalStartTime.ToShortTimeString()} - {this.RPEvent.LocalEndTime.ToShortTimeString()}");
                        ImGui.Spacing();

                        if (!string.IsNullOrWhiteSpace(this.RPEvent.Location))
                        {
                            ImGui.Text("Location:");
                            ImGui.Separator();

                            string serverLine = string.Empty;
                            if (!string.IsNullOrWhiteSpace(this.RPEvent.Datacenter))
                            {
                                serverLine = $"{this.RPEvent.Datacenter} - ";
                            }

                            if (!string.IsNullOrWhiteSpace(this.RPEvent.Server))
                            {
                                serverLine += $"{this.RPEvent.Server}: ";
                            }

                            float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                            ImGui.Text($"{serverLine}{this.RPEvent.Location}");
                            ImGui.PopTextWrapPos();
                            ImGui.Spacing();
                        }

                        if (!string.IsNullOrWhiteSpace(this.RPEvent.ESRBRating))
                        {
                            ImGui.Text("ESRB Rating:");
                            ImGui.Separator();
                            ImGui.Text(this.RPEvent.ESRBRating);

                            if (this.ESRBRatings != null)
                            {
                                ESRBRatingInfo? esrbRatingInfo = this.ESRBRatings
                                    .Where(x => x.RatingName == this.RPEvent.ESRBRating)
                                    .FirstOrDefault();

                                if (esrbRatingInfo != null && esrbRatingInfo.Description != null)
                                {
                                    ImGui.SameLine();
                                    ImGuiUtilities.BuildToolTip(esrbRatingInfo.Description);
                                }
                            }

                            ImGui.Spacing();
                        }

                        if (!string.IsNullOrWhiteSpace(this.RPEvent.ShortDescription))
                        {
                            ImGui.Text("Description:");
                            ImGui.Separator();

                            float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                            ImGui.Text(this.RPEvent.ShortDescription);
                            ImGui.PopTextWrapPos();

                            ImGui.Spacing();
                        }

                        if (!string.IsNullOrWhiteSpace(this.RPEvent.EventCategory))
                        {
                            ImGui.Text("Category:");
                            ImGui.Separator();

                            ImGui.Text($"{this.RPEvent.EventCategory}");

                            if (this.EventCategories != null)
                            {
                                EventCategoryInfo? eventCategoryInfo = this.EventCategories
                                    .Where(x => x.CategoryName == this.RPEvent.EventCategory)
                                    .FirstOrDefault();

                                if (eventCategoryInfo != null && eventCategoryInfo.Description != null)
                                {
                                    ImGui.SameLine();
                                    ImGuiUtilities.BuildToolTip(eventCategoryInfo.Description);
                                }
                            }

                            ImGui.Spacing();
                        }

                        if (!string.IsNullOrWhiteSpace(this.RPEvent.Contacts))
                        {
                            ImGui.Text("Contacts:");
                            ImGui.Separator();

                            float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                            ImGui.Text(this.RPEvent.Contacts);
                            ImGui.PopTextWrapPos();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispose of the <see cref="PluginUI"/> class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the <see cref="DetailsUI"/> class.
        /// </summary>
        /// <param name="disposing">A value indicating whether the Dispose call has been called directly instead of from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state(managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }
    }
}
