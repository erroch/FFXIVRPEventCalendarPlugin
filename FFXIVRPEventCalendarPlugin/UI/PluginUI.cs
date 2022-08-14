//-----------------------------------------------------------------------
// <copyright file="PluginUI.cs" company="FFXIV RP Event Calendar">
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
    using Dalamud.IoC;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;

    using ImGuiNET;

    /// <summary>
    /// The primary plugin UI.
    /// </summary>
    public class PluginUI : IDisposable
    {
        private const float OptionsSize = 122;
        private const float HeaderSize = 200;
        private const float FooterSize = 35;

        private readonly EventsService eventsService;
        private readonly Configuration configuration;
        private readonly SettingsUI settingsUI;
        private readonly DebugUI debugUI;
        private bool visible = false;
        private bool isLoading = false;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginUI"/> class.
        /// </summary>
        /// <param name="configuration">The Dalamud configuration.</param>
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
            this.LoadConfigureSettings();
            this.settingsUI = new SettingsUI(configuration);
            this.debugUI = new DebugUI();
            this.eventsService = new EventsService(configuration);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PluginUI"/> class.
        /// </summary>
        ~PluginUI()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// Gets the name of the plugin UI window.
        /// </summary>
        public static string Name => "FFXIV RP Event Calendar";

        /// <summary>
        /// Gets or sets a value indicating whether the PluginUI is visible.
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the SettingsUI is visible.
        /// </summary>
        public bool SettingsVisible
        {
            get
            {
                if (this.settingsUI != null)
                {
                    return this.settingsUI.Visible;
                }

                return false;
            }

            set
            {
                if (this.settingsUI != null)
                {
                    this.settingsUI.Visible = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DebugUI is visible.
        /// </summary>
        public bool DebugVisible
        {
            get
            {
                if (this.debugUI != null)
                {
                    return this.debugUI.Visible;
                }

                return false;
            }

            set
            {
                if (this.debugUI != null)
                {
                    this.debugUI.Visible = value;
                }
            }
        }

        private List<EventCategoryInfo>? EventCategories { get; set; }

        private List<ESRBRatingInfo>? ESRBRatings { get; set; }

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
        /// Draw the PluginUI.
        /// </summary>
        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.
            this.DrawMainWindow();
            this.settingsUI.Draw();
            this.debugUI.Draw();
        }

        /// <summary>
        /// Dispose of the <see cref="PluginUI"/> class.
        /// </summary>
        /// <param name="disposing">A value indicating whether the Dispose call has been called directly instead of from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.settingsUI != null)
                    {
                        this.settingsUI.Dispose();
                    }

                    if (this.eventsService != null)
                    {
                        this.eventsService.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        private void BuildEventRangeCombo(EventTimeframe selectedTimeFrame)
        {
            const string comboTitle = "Event Range: ";

            float width = (ImGui.CalcTextSize(EventTimeframe.NextHours.GetDescription()).X + ImGui.CalcTextSize(comboTitle).X) * 1.3f;
            float height = ImGui.GetTextLineHeightWithSpacing() * 1.3f;
            Vector2 vector2 = new (width, height);

            IEnumerable<EventTimeframe> eventTimeFrames = System.Enum.GetValues(typeof(EventTimeframe))
                .Cast<EventTimeframe>();

            if (ImGui.BeginChildFrame(1, vector2, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground))
            {
                ImGui.Text(comboTitle);
                ImGui.SameLine();

                // has to be non empty or the entire thing won't show.
                if (ImGui.BeginCombo(" ", selectedTimeFrame.GetDescription()))
                {
                    foreach (EventTimeframe timeFrame in eventTimeFrames)
                    {
                        if (ImGui.Selectable(timeFrame.GetDescription(), this.configuration.ConfigurationProperties.EventTimeframe == timeFrame))
                        {
                            this.configuration.ConfigurationProperties.EventTimeframe = timeFrame;
                            this.eventsService.FilterEvents();
                            this.configuration.Save();
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.EndChildFrame();
            }
        }

        private void BuildEventTable(List<RPEvent>? eventList, string tableId, bool optionsOpen)
        {
            this.BuildEventRangeCombo(this.configuration.ConfigurationProperties.EventTimeframe);

            if (eventList == null || eventList.Count == 0)
            {
                ImGui.Text("No events found.");
                return;
            }

            float tableSize = ImGui.GetWindowHeight() - HeaderSize - FooterSize;

            if (optionsOpen)
            {
                tableSize -= OptionsSize;
            }

            Vector2 outerSize = new (0, tableSize);
            if (ImGui.BeginTable(
                tableId,
                6,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY,
                outerSize))
            {
                ImGui.TableSetupColumn("Server");
                ImGui.TableSetupColumn("Start Time");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Location");
                ImGui.TableSetupColumn("URL");
                ImGui.TableSetupColumn("Category");
                ImGui.TableHeadersRow();
                foreach (RPEvent myEvent in eventList)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.Server}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.LocalStartTime:g}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.EventName}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.Location}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.EventURL}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.EventCategory}");
                }

                ImGui.EndTable();
            }
        }

        private void DrawMainWindow()
        {
            if (!this.Visible)
            {
                return;
            }

            this.eventsService.RefreshEvents();
            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("FFXIV RP Event Calendar", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

                ImGui.Text($"Local Time Zone: {timeZoneInfo.DisplayName} Current Offset: {timeZoneInfo.GetUtcOffset(DateTime.UtcNow)}");

                ImGui.Spacing();

                if (this.configuration.ConfigurationProperties.TimeZoneInfo != TimeZoneInfo.Local)
                {
                    ImGui.Text($"Overriding with Configured Time Zone: {this.configuration.ConfigurationProperties.TimeZoneInfo.DisplayName}");
                }

                ImGui.Separator();

                bool optionsOpen = this.BuildOptions();

                ImGui.Text("Event Lists");

                if (ImGui.BeginTabBar("Roleplay Events"))
                {
                    if (ImGui.BeginTabItem("Server"))
                    {
                        ImGui.Text("Current Server Events");
                        List<RPEvent>? serverEvents = this.eventsService.ServerEvents;
                        this.BuildEventTable(serverEvents, "##ServerEvents", optionsOpen);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Data Center"))
                    {
                        ImGui.Text("Current Data Center Events");
                        List<RPEvent>? serverEvents = this.eventsService.DatacenterEvents;
                        this.BuildEventTable(serverEvents, "##DatacenterEvents", optionsOpen);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Region"))
                    {
                        ImGui.Text("Current Region Events");
                        List<RPEvent>? serverEvents = this.eventsService.RegionEvents;
                        this.BuildEventTable(serverEvents, "##RegionEvents", optionsOpen);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Test"))
                    {
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }

            ImGui.Separator();
            ImGui.Text($"Event list last udpated on: {this.eventsService.LastRefreshLocalTime:g}");
            ImGui.SameLine();
            if (ImGui.Button("Refresh"))
            {
                this.eventsService.RefreshEvents(forceRefresh: true);
            }

            ImGui.End();
        }

        private bool BuildOptions()
        {
            if (ImGui.CollapsingHeader("Options"))
            {
                ImGui.Text("ESRB RATINGS");
                if (this.ESRBRatings == null)
                {
                    ImGui.Text("ESRBRatings Loading...");
                }
                else
                {
                    if (this.configuration.ConfigurationProperties.Ratings.Count == 0)
                    {
                        this.configuration.ConfigurationProperties.Ratings = this.ESRBRatings
                            .Where(x => x.RatingName != null)
                            .Select(x => x.RatingName ?? string.Empty)
                            .ToList();
                    }

                    foreach (ESRBRatingInfo rating in this.ESRBRatings)
                    {
                        if (!rating.RequiresAgeValidation)
                        {
                            if (rating.RatingName != null)
                            {
                                bool check = this.configuration.ConfigurationProperties.Ratings.Contains(rating.RatingName);
                                if (ImGui.Checkbox(rating.RatingName, ref check))
                                {
                                    if (check)
                                    {
                                        if (!this.configuration.ConfigurationProperties.Ratings.Contains(rating.RatingName))
                                        {
                                            this.configuration.ConfigurationProperties.Ratings.Add(rating.RatingName);
                                        }
                                    }
                                    else
                                    {
                                        if (this.configuration.ConfigurationProperties.Ratings.Contains(rating.RatingName))
                                        {
                                            this.configuration.ConfigurationProperties.Ratings.Remove(rating.RatingName);
                                        }
                                    }

                                    this.configuration.Save();
                                    this.eventsService.FilterEvents();
                                }
                            }

                            ImGui.SameLine();
                        }
                    }
                }

                ImGui.NewLine();
                ImGui.Separator();
                ImGui.Text("CATEGROIES");
                if (this.EventCategories == null)
                {
                    ImGui.Text("Categories are loading...");
                }
                else
                {
                    if (this.configuration.ConfigurationProperties.Categories == null)
                    {
                        this.configuration.ConfigurationProperties.Categories = this.EventCategories
                            .Where(x => x.CategoryName != null)
                            .Select(x => x.CategoryName ?? string.Empty)
                            .ToList();
                    }

                    int itemCount = 0;

                    foreach (EventCategoryInfo category in this.EventCategories)
                    {
                        if (category.CategoryName != null)
                        {
                            bool check = this.configuration.ConfigurationProperties.Categories.Contains(category.CategoryName);
                            if (ImGui.Checkbox(category.CategoryName, ref check))
                            {
                                if (check)
                                {
                                    if (!this.configuration.ConfigurationProperties.Categories.Contains(category.CategoryName))
                                    {
                                        this.configuration.ConfigurationProperties.Categories.Add(category.CategoryName);
                                    }
                                }
                                else
                                {
                                    if (this.configuration.ConfigurationProperties.Categories.Contains(category.CategoryName))
                                    {
                                        this.configuration.ConfigurationProperties.Categories.Remove(category.CategoryName);
                                    }
                                }

                                this.configuration.Save();
                                this.eventsService.FilterEvents();
                            }

                            if (category.Description != null)
                            {
                                ImGui.SameLine();
                                ImGuiUtilities.BuildToolTip(category.Description);
                            }

                            if (itemCount >= 5)
                            {
                                itemCount = 0;
                            }
                            else
                            {
                                itemCount++;
                                ImGui.SameLine();
                            }
                        }
                    }
                }

                ImGui.Separator();

                return true;
            }

            return false;
        }

        private void LoadConfigureSettings()
        {
            if (!this.isLoading)
            {
                this.isLoading = true;

                Task<List<ESRBRatingInfo>>.Run(async () => await CalendarService.ESRBRatings(this.configuration.ConfigurationProperties)
                    .ContinueWith(t =>
                       {
                           if (t.IsFaulted)
                           {
                               AggregateException? ex = t.Exception;
                               if (ex != null)
                               {
                                   Plugin.ChatGui.PrintError($"Error getting Event Ratings: {ex.Message}");
                               }
                               else
                               {
                                   Plugin.ChatGui.PrintError($"Error getting Event Ratings");
                               }

                               this.ESRBRatings = new List<ESRBRatingInfo>();
                           }
                           else
                           {
                               this.ESRBRatings = t.Result;
                           }
                       }));

                Task.Run(async () => await CalendarService.EventCategories(this.configuration.ConfigurationProperties)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            AggregateException? ex = t.Exception;
                            if (ex != null)
                            {
                                Plugin.ChatGui.PrintError($"Error getting Event Categories: {ex.Message}");
                            }
                            else
                            {
                                Plugin.ChatGui.PrintError($"Error getting Event Categories");
                            }
                        }
                        else
                        {
                            this.EventCategories = t.Result;
                        }
                    }));
            }
        }
    }
}
