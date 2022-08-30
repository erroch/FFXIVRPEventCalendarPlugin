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
    using System.Text;
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
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The primary plugin UI.
    /// </summary>
    public class PluginUI : IDisposable
    {
        private const float FooterSize = 35;

        private const string ComboEventRangeTitle = "Event Range: ";
        private const string TextBoxSearchTitle = "Search: ";

        private readonly EventsService eventsService;
        private readonly Configuration configuration;
        private readonly SettingsUI settingsUI;
        private readonly DebugUI debugUI;
        private bool visible = false;
        private bool isLoading = false;
        private bool disposedValue;
        private byte[] textBuffer = new byte[64];

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

        /// <summary>
        /// Gets or sets value of chars array for InputText.
        /// </summary>
        private byte[] TextBuffer
        {
            get { return this.textBuffer; }
            set { this.textBuffer = value; }
        }

        private List<EventCategoryInfo>? EventCategories { get; set; }

        private List<ESRBRatingInfo>? ESRBRatings { get; set; }

        private RPEvent? SelectedRPEvent { get; set; }

        /// <summary>
        /// Returns given event list filtered by searchFilter string.
        /// </summary>
        /// <param name="events"> Given time frame from configuration.</param>
        /// <param name="searchFilter"> Given search filter.</param>
        /// <returns> The given event list filtered by event name.</returns>
        public static List<RPEvent>? SearchEvents(List<RPEvent>? events, string searchFilter)
        {
            List<RPEvent>? rPEvents = new ();

            if (events == null)
            {
                return rPEvents;
            }

            foreach (RPEvent e in events!)
            {
                if (e.EventName!.ToLower().IndexOf(searchFilter.ToLower()) != -1)
                {
                    rPEvents.Add(e);
                }
            }

            return rPEvents;
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

                    if (this.debugUI != null)
                    {
                        this.debugUI.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        private void BuildWidgets()
        {
            // Build Event Range Combo
            Vector2 vector2 = ImGuiUtilities.CalcWidgetChildFrameVector2(ImGui.CalcTextSize(EventTimeframe.NextHours.GetDescription()).X, ImGui.CalcTextSize(ComboEventRangeTitle).X);

            if (ImGui.BeginChildFrame(1, vector2, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground))
            {
                this.BuildEventRangeCombo(this.configuration.EventTimeframe);
            }

            ImGui.EndChildFrame();
            ImGui.SameLine();

            // Build Search Input Textbox
            vector2 = ImGuiUtilities.CalcWidgetChildFrameVector2(ImGui.GetWindowWidth() / 4, ImGui.CalcTextSize(ComboEventRangeTitle).X);

            if (ImGui.BeginChildFrame(2, vector2, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground))
            {
                this.BuildEventSearchTextBox();
            }

            ImGui.EndChildFrame();
        }

        private void BuildEventRangeCombo(EventTimeframe selectedTimeFrame)
        {
            IEnumerable<EventTimeframe> eventTimeFrames = System.Enum.GetValues(typeof(EventTimeframe)).Cast<EventTimeframe>();
            ImGui.Text(ComboEventRangeTitle);
            ImGui.SameLine();

            // has to be non empty or the entire thing won't show.
            if (ImGui.BeginCombo(" ", selectedTimeFrame.GetDescription()))
            {
                foreach (EventTimeframe timeFrame in eventTimeFrames)
                {
                    if (ImGui.Selectable(timeFrame.GetDescription(), this.configuration.EventTimeframe == timeFrame))
                    {
                        this.configuration.EventTimeframe = timeFrame;
                        this.eventsService.FilterEvents();
                        this.configuration.Save();
                    }
                }

                ImGui.EndCombo();
            }
        }

        private void BuildEventSearchTextBox()
        {
            uint buf = 64;

            ImGui.Text(TextBoxSearchTitle);
            ImGui.SameLine();
            ImGui.InputText(" ", this.TextBuffer, buf);
        }

        private void DrawMainWindow()
        {
            if (!this.Visible)
            {
                return;
            }

            const float detailsWidth = 320f;

            this.eventsService.RefreshEvents();
            ImGui.SetNextWindowSize(new Vector2(900, 600), ImGuiCond.Appearing);
            ImGui.SetNextWindowSizeConstraints(new Vector2(720, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("FFXIV RP Event Calendar", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.BeginChild("##mainwindow", new Vector2(ImGui.GetContentRegionAvail().X, -1f * FooterSize * ImGuiHelpers.GlobalScale), false, ImGuiWindowFlags.NoScrollbar))
                {
                    if (ImGui.BeginTable("##mainSizingTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
                    {
                        ImGui.TableSetupColumn("##eventListColumn", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("##detailListColumn", ImGuiTableColumnFlags.WidthStretch, detailsWidth);
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();

                        this.BuildEventListColumn();

                        ImGui.TableNextColumn();

                        this.BuildDetailsColumn();
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();

                ImGui.Separator();
                ImGui.Text($"Event list last udpated on: {this.eventsService.LastRefreshLocalTime:g}");
                ImGui.SameLine();
                if (ImGui.Button("Refresh"))
                {
                    this.eventsService.RefreshEvents(forceRefresh: true);
                }
            }

            ImGui.End();
        }

        private void BuildEventListColumn()
        {
            if (ImGui.BeginChild("##EventListPane", new Vector2(-1, -1), true))
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

                ImGui.Text($"Local Time Zone: {timeZoneInfo.DisplayName} Current Offset: {timeZoneInfo.GetUtcOffset(DateTime.UtcNow)}");

                ImGui.Spacing();

                if (this.configuration.TimeZoneInfo != TimeZoneInfo.Local)
                {
                    ImGui.Text($"Overriding with Configured Time Zone: {this.configuration.TimeZoneInfo.DisplayName}");
                }

                ImGui.Separator();

                this.BuildOptions();

                ImGui.Text("Event Lists");

                if (ImGui.BeginTabBar("Roleplay Events"))
                {
                    if (ImGui.BeginTabItem("Server"))
                    {
                        ImGui.Text("Current Server Events");
                        List<RPEvent>? serverEvents = this.eventsService.ServerEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            serverEvents = SearchEvents(serverEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(serverEvents, "##ServerEvents");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Data Center"))
                    {
                        ImGui.Text("Current Data Center Events");
                        List<RPEvent>? serverEvents = this.eventsService.DatacenterEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            serverEvents = SearchEvents(serverEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(serverEvents, "##DatacenterEvents");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Region"))
                    {
                        ImGui.Text("Current Region Events");
                        List<RPEvent>? serverEvents = this.eventsService.RegionEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            serverEvents = SearchEvents(serverEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(serverEvents, "##RegionEvents");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Test"))
                    {
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }

            ImGui.EndChild();
        }

        private void BuildDetailsColumn()
        {
            if (this.SelectedRPEvent == null)
            {
                ImGui.Text("Select an Event to display details.");
            }
            else
            {
                ImGui.Text(this.SelectedRPEvent.EventName);
                ImGui.Spacing();

                if (ImGui.BeginChild("###eventDetails", new Vector2(-1, -1), true))
                {
                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.EventURL))
                    {
                        ImGui.Text("Website:");
                        ImGui.Separator();
                        ImGui.Text(this.SelectedRPEvent.EventURL);
                        ImGui.SameLine();
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe))
                        {
                            ImGuiUtilities.OpenBrowser(this.SelectedRPEvent.EventURL);
                        }

                        ImGui.Spacing();
                    }

                    ImGui.Text("When:");
                    ImGui.Separator();
                    ImGui.Text($"{this.SelectedRPEvent.LocalStartTime.ToShortDateString()} {this.SelectedRPEvent.LocalStartTime.ToShortTimeString()} - {this.SelectedRPEvent.LocalEndTime.ToShortTimeString()}");
                    ImGui.Spacing();

                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.Location))
                    {
                        ImGui.Text("Location:");
                        ImGui.Separator();

                        string serverLine = string.Empty;
                        if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.Datacenter))
                        {
                            serverLine = $"{this.SelectedRPEvent.Datacenter} - ";
                        }

                        if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.Server))
                        {
                            serverLine += $"{this.SelectedRPEvent.Server}: ";
                        }

                        float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                        ImGui.Text($"{serverLine}{this.SelectedRPEvent.Location}");
                        ImGui.PopTextWrapPos();
                        ImGui.Spacing();
                    }

                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.ESRBRating))
                    {
                        ImGui.Text("ESRB Rating:");
                        ImGui.Separator();
                        ImGui.Text(this.SelectedRPEvent.ESRBRating);

                        if (this.ESRBRatings != null)
                        {
                            ESRBRatingInfo? esrbRatingInfo = this.ESRBRatings
                                .Where(x => x.RatingName == this.SelectedRPEvent.ESRBRating)
                                .FirstOrDefault();

                            if (esrbRatingInfo != null && esrbRatingInfo.Description != null)
                            {
                                ImGui.SameLine();
                                ImGuiUtilities.BuildToolTip(esrbRatingInfo.Description);
                            }
                        }

                        ImGui.Spacing();
                    }

                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.ShortDescription))
                    {
                        ImGui.Text("Description:");
                        ImGui.Separator();

                        float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                        ImGui.Text(this.SelectedRPEvent.ShortDescription);
                        ImGui.PopTextWrapPos();

                        ImGui.Spacing();
                    }

                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.EventCategory))
                    {
                        ImGui.Text("Category:");
                        ImGui.Separator();

                        ImGui.Text($"{this.SelectedRPEvent.EventCategory}");

                        if (this.EventCategories != null)
                        {
                            EventCategoryInfo? eventCategoryInfo = this.EventCategories
                                .Where(x => x.CategoryName == this.SelectedRPEvent.EventCategory)
                                .FirstOrDefault();

                            if (eventCategoryInfo != null && eventCategoryInfo.Description != null)
                            {
                                ImGui.SameLine();
                                ImGuiUtilities.BuildToolTip(eventCategoryInfo.Description);
                            }
                        }

                        ImGui.Spacing();
                    }

                    if (!string.IsNullOrWhiteSpace(this.SelectedRPEvent.Contacts))
                    {
                        ImGui.Text("Contacts:");
                        ImGui.Separator();

                        float wrapWidth = ImGui.GetWindowContentRegionMax().X;
                        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + wrapWidth - 10);
                        ImGui.Text(this.SelectedRPEvent.Contacts);
                        ImGui.PopTextWrapPos();
                    }
                }

                ImGui.EndChild();
            }
        }

        private void BuildEventTable(List<RPEvent>? eventList, string tableId)
        {
            this.BuildWidgets();

            if (eventList == null || eventList.Count == 0)
            {
                ImGui.Text("No events found.");
                return;
            }

            Vector2 outerSize = new (-1, -1);
            if (ImGui.BeginTable(
                tableId,
                9,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Hideable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY,
                outerSize))
            {
                ImGui.TableSetupColumn("Server");
                ImGui.TableSetupColumn("Datacenter", ImGuiTableColumnFlags.DefaultHide);
                ImGui.TableSetupColumn("Start Time");
                ImGui.TableSetupColumn("End Time", ImGuiTableColumnFlags.DefaultHide);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Location");
                ImGui.TableSetupColumn("URL");
                ImGui.TableSetupColumn("Rating", ImGuiTableColumnFlags.DefaultHide);
                ImGui.TableSetupColumn("Category");
                ImGui.TableHeadersRow();

                int rowNumber = 0;
                foreach (RPEvent myEvent in eventList)
                {
                    bool rowSelected = myEvent == this.SelectedRPEvent;

                    string rowId = $"eventRow_{rowNumber}";
                    ImGui.PushID(rowId);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGui.Selectable(myEvent.Server, rowSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        this.SelectedRPEvent = myEvent;
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.Datacenter);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{myEvent.LocalStartTime:g}");
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.LocalEndTime.ToShortTimeString());
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.EventName);
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.Location);
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.EventURL);
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.ESRBRating);
                    ImGui.TableNextColumn();
                    ImGui.Text(myEvent.EventCategory);
                    ImGui.PopID();

                    rowNumber++;
                }

                ImGui.EndTable();
            }
        }

        private void BuildOptions()
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
                    if (this.configuration.Ratings.Count == 0)
                    {
                        this.configuration.Ratings = this.ESRBRatings
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
                                bool check = this.configuration.Ratings.Contains(rating.RatingName);
                                if (ImGui.Checkbox(rating.RatingName, ref check))
                                {
                                    if (check)
                                    {
                                        if (!this.configuration.Ratings.Contains(rating.RatingName))
                                        {
                                            this.configuration.Ratings.Add(rating.RatingName);
                                        }
                                    }
                                    else
                                    {
                                        if (this.configuration.Ratings.Contains(rating.RatingName))
                                        {
                                            this.configuration.Ratings.Remove(rating.RatingName);
                                        }
                                    }

                                    this.configuration.Save();
                                    this.eventsService.FilterEvents();
                                }

                                if (rating.Description != null)
                                {
                                    ImGui.SameLine();
                                    ImGuiUtilities.BuildToolTip(rating.Description);
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
                    if (this.configuration.Categories == null)
                    {
                        this.configuration.Categories = this.EventCategories
                            .Where(x => x.CategoryName != null)
                            .Select(x => x.CategoryName ?? string.Empty)
                            .ToList();
                    }

                    int itemCount = 0;

                    foreach (EventCategoryInfo category in this.EventCategories)
                    {
                        if (category.CategoryName != null)
                        {
                            bool check = this.configuration.Categories.Contains(category.CategoryName);
                            if (ImGui.Checkbox(category.CategoryName, ref check))
                            {
                                if (check)
                                {
                                    if (!this.configuration.Categories.Contains(category.CategoryName))
                                    {
                                        this.configuration.Categories.Add(category.CategoryName);
                                    }
                                }
                                else
                                {
                                    if (this.configuration.Categories.Contains(category.CategoryName))
                                    {
                                        this.configuration.Categories.Remove(category.CategoryName);
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
            }
        }

        private void LoadConfigureSettings()
        {
            if (!this.isLoading)
            {
                this.isLoading = true;

                Task<List<ESRBRatingInfo>>.Run(async () => await CalendarService.ESRBRatings(this.configuration)
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

                Task.Run(async () => await CalendarService.EventCategories(this.configuration)
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
