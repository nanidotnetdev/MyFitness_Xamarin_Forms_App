using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyFitness.Models;
using MyFitness.Utilities;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Forms.GoogleMaps;

namespace MyFitness.Repository
{
    public class MyFitnessDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        public static SQLiteAsyncConnection Database = lazyInitializer.Value;

        static bool initialized = false;

        public MyFitnessDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (Database.TableMappings.All(m => m.MappedType.Name != typeof(Route).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(Route)).ConfigureAwait(false);
                    initialized = true;
                }
            }
        }

        public IEnumerable<Polyline> GetAllPolyLines()
        {
            try
            {
                var items = GetItemsAsync().GetAwaiter().GetResult();
                var routes = items.Select(p => p.Value);
                var polyLines = routes.Select(JsonConvert.DeserializeObject<Polyline>);
                return polyLines;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new List<Polyline>();
        }

        public Task<List<Route>> GetItemsAsync()
        {
            return Database.Table<Route>().ToListAsync();
        }

        public Task<Route> GetItemAsync(int id)
        {
            return Database.Table<Route>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(Route item)
        {
            if (item.Id != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(Route item)
        {
            return Database.DeleteAsync(item);
        }
    }

    public static class TaskExtensions
    {
        // NOTE: Async void is intentional here. This provides a way
        // to call an async method from the constructor while
        // communicating intent to fire and forget, and allow
        // handling of exceptions
        public static async void SafeFireAndForget(this Task task,
            bool returnToCallingContext,
            Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }

            // if the provided action is not null, catch and
            // pass the thrown exception
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }
    }
}
