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

    using Dalamud.Interface;
    using Dalamud.Interface.Components;
    using Dalamud.Interface.Utility;

    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Models;
    using FFXIVRPCalendarPlugin.Services;

    using ImGuiNET;

    /// <summary>
    /// The primary plugin UI.
    /// </summary>
    public class PluginUI : IDisposable
    {
        private const float FooterSize = 35;

        private const string ComboEventRangeTitle = "Event Range: ";
        private const string TextBoxSearchTitle = "Search: ";
        private const string TimeZoneSelectionTitle = "Timezone: ";

        private readonly EventsService eventsService;
        private readonly Configuration configuration;
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
            this.LoadConfiguration();
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
        private byte[] TextBuffer { get; set; } = new byte[64];

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

        private void DrawMainWindow()
        {
            if (!this.Visible)
            {
                return;
            }

            const float detailsWidth = 320f;
            const float eventListWidth = 900f - detailsWidth;

            this.eventsService.RefreshEvents();
            ImGui.SetNextWindowSize(new Vector2(900, 600), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(720, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("FFXIV RP Event Calendar", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.BeginChild("##mainwindow", new Vector2(ImGui.GetContentRegionAvail().X, -1f * FooterSize * ImGuiHelpers.GlobalScale), false, ImGuiWindowFlags.NoScrollbar))
                {
                    if (ImGui.BeginTable("##mainSizingTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
                    {
                        ImGui.TableSetupColumn("##eventListColumn", ImGuiTableColumnFlags.WidthStretch, eventListWidth);
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

                string myUrl = "https://xiv.fyi/new";
                ImGui.SameLine();
                ImGui.Text($"Add a new Event");
                ImGui.SameLine();
                ImGui.PushID(1);
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe))
                {
                    ImGuiUtilities.OpenBrowser(myUrl);
                }

                ImGui.PopID();

                string reportUrl = "https://xiv.fyi/report";
                ImGui.SameLine();
                ImGui.Text($"Report an Inaccurate Event");
                ImGui.SameLine();
                ImGui.PushID(2);
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe))
                {
                    ImGuiUtilities.OpenBrowser(reportUrl);
                }

                ImGui.PopID();
            }

            ImGui.End();
        }

        private void BuildEventListColumn()
        {
            if (ImGui.BeginChild("##EventListPane", new Vector2(-1, -1), true))
            {
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
                        List<RPEvent>? dataCenterEvents = this.eventsService.DatacenterEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            dataCenterEvents = SearchEvents(dataCenterEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(dataCenterEvents, "##DatacenterEvents");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Region"))
                    {
                        ImGui.Text("Current Region Events");
                        List<RPEvent>? regionEvents = this.eventsService.RegionEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            regionEvents = SearchEvents(regionEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(regionEvents, "##RegionEvents");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("All"))
                    {
                        ImGui.Text("All FFXIV RP Events");
                        List<RPEvent>? allEvents = this.eventsService.FilteredEvents;
                        if (Encoding.Default.GetString(this.TextBuffer) != string.Empty)
                        {
                            allEvents = SearchEvents(allEvents, Encoding.Default.GetString(this.TextBuffer));
                        }

                        this.BuildEventTable(allEvents, "##AllEvents");
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

        private unsafe void BuildEventTable(List<RPEvent>? eventList, string tableId)
        {
            this.BuildEventFilters();

            if (eventList == null || eventList.Count == 0)
            {
                ImGui.Text("No events found.");
                return;
            }

            Vector2 outerSize = new (-1, -1);
            if (ImGui.BeginTable(
                tableId,
                9,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Hideable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable,
                outerSize))
            {
                ImGui.TableSetupColumn("Server", ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("Datacenter", ImGuiTableColumnFlags.DefaultHide | ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("Start Time", ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("End Time", ImGuiTableColumnFlags.DefaultHide);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("URL", ImGuiTableColumnFlags.NoSort);
                ImGui.TableSetupColumn("Rating", ImGuiTableColumnFlags.DefaultHide | ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableHeadersRow();

                ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
                if (sortSpecs.NativePtr != null && sortSpecs.SpecsDirty)
                {
                    switch (sortSpecs.Specs.ColumnIndex)
                    {
                        case 0:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.Server, b.Server)
                                : string.Compare(b.Server, a.Server));

                            break;
                        case 1:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.Datacenter, b.Datacenter)
                                : string.Compare(b.Datacenter, a.Datacenter));

                            break;
                        case 2:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? ImGuiUtilities.SortTime(a.StartTimeUTC, b.StartTimeUTC)
                                : ImGuiUtilities.SortTime(b.StartTimeUTC, a.StartTimeUTC));

                            break;
                        case 3:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? ImGuiUtilities.SortTime(a.EndTimeUTC, b.EndTimeUTC)
                                : ImGuiUtilities.SortTime(b.EndTimeUTC, a.EndTimeUTC));

                            break;
                        case 4:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.EventName, b.EventName)
                                : string.Compare(b.EventName, a.EventName));

                            break;
                        case 5:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.Location, b.Location)
                                : string.Compare(b.Location, a.Location));

                            break;
                        case 6:
                            break;
                        case 7:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.ESRBRating, b.ESRBRating)
                                : string.Compare(b.ESRBRating, a.ESRBRating));

                            break;
                        case 8:
                            eventList.Sort((a, b) => sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending
                                ? string.Compare(a.EventCategory, b.EventCategory)
                                : string.Compare(b.EventCategory, a.EventCategory));

                            break;
                        default:
                            break;
                    }
                }

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
                this.BuildTimeZoneCombo();
                ImGui.Separator();
                ImGui.BeginTable("esrb_misc_option_split", 2, ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoBordersInBody);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                this.BuildESRBOptions();
                ImGui.TableNextColumn();
                this.BuildMiscOptions();
                ImGui.EndTable();
                ImGui.Separator();
                this.BuildCategoryOptions();
                ImGui.Separator();
                ImGui.Separator();
            }
        }

        private void BuildMiscOptions()
        {
            ImGui.Text("Misc Options:");
            bool oneTimeOnly = this.configuration.OneTimeEventsOnly;
            if (ImGui.Checkbox("One Time Events Only", ref oneTimeOnly))
            {
                if (oneTimeOnly != this.configuration.OneTimeEventsOnly)
                {
                    this.configuration.OneTimeEventsOnly = oneTimeOnly;
                    this.configuration.Save();
                    this.eventsService.FilterEvents();
                }
            }

            ImGui.SameLine();
            ImGuiUtilities.BuildToolTip("Show only events with no recurrence schedule.  This may also show events with special one time schedule changes that are out of the ordinary.");

            // TODO: Re-add show real gil filter when the API actually supports it.
            // ImGui.SameLine();
            // bool showRealGilEvents = this.configuration.ShowRealGilEvents;
            // if (ImGui.Checkbox("Show Real Gil Events", ref showRealGilEvents))
            // {
            //     if (showRealGilEvents != this.configuration.ShowRealGilEvents)
            //     {
            //         this.configuration.ShowRealGilEvents = showRealGilEvents;
            //         this.configuration.Save();
            //         this.eventsService.FilterEvents();
            //     }
            // }
            // ImGui.SameLine();
            // ImGuiUtilities.BuildToolTip("Show events that use real or out-of-character gil in any fashion including menu items, services, raffles, giveaways, etc.");
        }

        private void BuildESRBOptions()
        {
            ImGui.Text("ESRB Retings:");
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
        }

        private void BuildCategoryOptions()
        {
            ImGui.Text("Categories:");
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

                ImGuiStylePtr stylePtr = ImGui.GetStyle();

                float windowWidth = ImGui.GetWindowWidth();
                float itemSpacing = stylePtr.ItemSpacing.X;
                float paddingSize = 0;
                float currentWidth = 0;

                bool first = true;

                foreach (EventCategoryInfo category in this.EventCategories)
                {
                    if (category.CategoryName != null)
                    {
                        bool check = this.configuration.Categories.Contains(category.CategoryName);
                        float elementWidth = ImGui.CalcTextSize(category.CategoryName).X + paddingSize;

                        if (!first)
                        {
                            if (currentWidth + elementWidth >= windowWidth)
                            {
                                currentWidth = elementWidth;
                            }
                            else
                            {
                                ImGui.SameLine();
                                currentWidth += elementWidth;
                            }
                        }

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

                        if (first)
                        {
                            currentWidth = ImGui.GetItemRectSize().X + ImGuiUtilities.GetTooltipSize() + (stylePtr.ItemSpacing.X * 2);
                            paddingSize = currentWidth - elementWidth;
                            first = false;
                        }

                        if (category.Description != null)
                        {
                            ImGui.SameLine();
                            ImGuiUtilities.BuildToolTip(category.Description);
                        }
                    }
                }
            }
        }

        private void BuildEventFilters()
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

        private void BuildTimeZoneCombo()
        {
            IEnumerable<TimeZoneInfo> timeZoneInfos = TimeZoneInfo.GetSystemTimeZones();
            ImGui.Text(TimeZoneSelectionTitle);
            ImGui.SameLine();

            if (ImGui.BeginCombo(" ", this.configuration.TimeZoneInfo.DisplayName))
            {
                foreach (TimeZoneInfo timeZoneInfo in timeZoneInfos)
                {
                    if (ImGui.Selectable(
                        timeZoneInfo.DisplayName,
                        this.configuration.TimeZoneInfo == timeZoneInfo))
                    {
                        this.configuration.TimeZoneInfo = timeZoneInfo;
                        this.eventsService.FilterEvents();
                        this.configuration.Save();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            if (ImGui.Button("Set to Local Timezone"))
            {
                this.configuration.TimeZoneInfo = TimeZoneInfo.Local;
                this.eventsService.FilterEvents();
                this.configuration.Save();
            }
        }

        private void BuildEventSearchTextBox()
        {
            uint buf = 64;

            ImGui.Text(TextBoxSearchTitle);
            ImGui.SameLine();
            ImGui.InputText(" ", this.TextBuffer, buf);
        }

        private void LoadConfiguration()
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
