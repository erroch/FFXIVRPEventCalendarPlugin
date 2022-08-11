using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVRPCalendar.Models;
using FFXIVRPCalendar.Services;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ConfigurationProperties configuration = new ConfigurationProperties();
            try
            {
                Task.Run(async () => await EventService.GetToday(configuration)
                    .ContinueWith(
                        t =>
                        {
                            if (t.IsFaulted)
                            {
                                Console.WriteLine($"Error retrieving events.");
                            }
                            else
                            {
                                List<RPEvent> events = t.Result;
                                Console.WriteLine($"Number of events parsed: {events.Count}");
                            }
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.ReadLine();
        }
    }
}
