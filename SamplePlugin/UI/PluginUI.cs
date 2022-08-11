namespace FFXIVRPCalendarPlugin.UI
{
    using ImGuiNET;
    using Dalamud.Game.Gui;
    using Dalamud.Game.ClientState;
    using Dalamud.IoC;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using FFXIVRPCalendar.Services;
    using FFXIVRPCalendar.Models;
    using FFXIVRPCalendarPlugin;
    using FFXIVRPCalendarPlugin.Services;

    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        public static string Name => "FFXIV RP Event Calendar";
        private readonly EventsService eventsService;

        private readonly Configuration configuration;
        //public List<RPEvent>? roleplayEvents;
        private List<EventCategoryInfo>? EventCategories { get; set; }
        private List<ESRBRatingInfo>? ESRBRatings { get; set; }

        // private List<RPEvent>? filteredEvents;


        public bool isLoading = false;
        // private bool isEventsLoading = false;

        private readonly SettingsUI settingsUI;
        private readonly DebugUI debugUI;
        

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

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
            set { 
                if (this.settingsUI != null)
                {
                    this.settingsUI.Visible = value;
                }
            }
        }

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

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
            this.LoadConfigureSettings();
            this.settingsUI = new SettingsUI(configuration);
            this.debugUI = new DebugUI(configuration);
            this.eventsService = new EventsService(configuration);
        }

        public void Dispose()
        {
            if (this.settingsUI != null)
            {
                this.settingsUI.Dispose();
                this.eventsService.Dispose();
            }
        }

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

        public void DrawMainWindow()
        {
            if (!this.Visible)
            {
                return;
            }

            this.eventsService.RefreshEvents();
            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("FFXIV RP Event Calendar", ref visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

                ImGui.Text($"Local Time Zone: {timeZoneInfo.DisplayName} Current Offset: {timeZoneInfo.GetUtcOffset(DateTime.UtcNow)}");

                ImGui.Spacing();

                if (configuration.ConfigurationProperties.TimeZoneInfo != TimeZoneInfo.Local)
                {
                    ImGui.Text($"Overriding with Configured Time Zone: {configuration.ConfigurationProperties.TimeZoneInfo.DisplayName}");
                }

                ImGui.Separator();

                this.BuildOptions();

                ImGui.Text("Event Lists");

                if (ImGui.BeginTabBar("Roleplay Events"))
                {
                    if (ImGui.BeginTabItem("Server"))
                    {
                        ImGui.Text("Current Server Events");
                        List<RPEvent>? serverEvents = eventsService.ServerEvents;
                        BuildEventTable(serverEvents, "##ServerEvents");
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Data Center"))
                    {
                        ImGui.Text("Current Data Center Events");
                        List<RPEvent>? serverEvents = eventsService.DatacenterEvents;
                        BuildEventTable(serverEvents, "##DatacenterEvents");
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("Region"))
                    {
                        ImGui.Text("Current Region Events");
                        List<RPEvent>? serverEvents = eventsService.RegionEvents;
                        BuildEventTable(serverEvents, "##RegionEvents");
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

        private static void BuildEventTable(List<RPEvent>? eventList, string tableId)
        {
            if (eventList == null || eventList.Count == 0)
            {
                ImGui.Text("No events found.");
                return;
            }

            Vector2 outerSize = new Vector2(0, ImGui.GetWindowHeight() - 170 - 35);
            if (ImGui.BeginTable(tableId, 6,
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.SizingStretchProp | 
                ImGuiTableFlags.BordersInnerV | 
                ImGuiTableFlags.ScrollY,
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
                    if (this.configuration.ConfigurationProperties.Ratings.Count == 0)
                    {
                        this.configuration.ConfigurationProperties.Ratings = this.ESRBRatings.Select(x => x.RatingName).ToList(); ;
                    }

                    foreach (ESRBRatingInfo rating in this.ESRBRatings)
                    {
                        if (!rating.RequiresAgeValidation)
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
                        this.configuration.ConfigurationProperties.Categories = this.EventCategories.Select(x => x.CategoryName).ToList();
                    }

                    int itemCount = 0;

                    foreach (EventCategoryInfo category in this.EventCategories)
                    {
                        bool check = configuration.ConfigurationProperties.Categories.Contains(category.CategoryName);
                        if (ImGui.Checkbox(category.CategoryName, ref check))
                        {
                            if (check)
                            {
                                if (!configuration.ConfigurationProperties.Categories.Contains(category.CategoryName))
                                {
                                    configuration.ConfigurationProperties.Categories.Add(category.CategoryName);
                                }
                            }
                            else
                            {
                                if (configuration.ConfigurationProperties.Categories.Contains(category.CategoryName))
                                {
                                    configuration.ConfigurationProperties.Categories.Remove(category.CategoryName);
                                }
                            }

                            this.configuration.Save();
                            this.eventsService.FilterEvents();
                        }

                        ImGui.SameLine();
                        BuildToolTip(category.Description);
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

                ImGui.Separator();
            }
        }
          
        private static void BuildToolTip(string description)
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

        private void LoadConfigureSettings()
        {
            if (!this.isLoading)
            {
                this.isLoading = true;

                Task.Run(async () => await ConfigurationService.ESRBRatings(this.configuration.ConfigurationProperties)
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
                       }
                    )
                );

                Task.Run(async () => await ConfigurationService.EventCategories(this.configuration.ConfigurationProperties)
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
                    }
                    )
                );
            }
        }
    }
}
