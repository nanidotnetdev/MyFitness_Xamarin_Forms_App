//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MyFitness.Models;
//using Newtonsoft.Json;
//using Xamarin.Forms.GoogleMaps;

//namespace MyFitness.Repository
//{
//    public class RouteRepository
//    {
//        public IEnumerable<Polyline> GetAllPolyLines()
//        {
//            try
//            {
//                var items = GetItemsAsync().GetAwaiter().GetResult();
//                var routes = items.Select(p => p.Value);
//                var polyLines = routes.Select(JsonConvert.DeserializeObject<Polyline>);
//                return polyLines;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }

//            return new List<Polyline>();
//        }

//        public Task<List<Route>> GetItemsAsync()
//        {
//            return MyFitnessDatabase.Database.Table<Route>().ToListAsync();
//        }

//        public Task<Route> GetItemAsync(int id)
//        {
//            return MyFitnessDatabase.Database.Table<Route>().Where(i => i.Id == id).FirstOrDefaultAsync();
//        }

//        public Task<int> SaveItemAsync(Route item)
//        {
//            if (item.Id != 0)
//            {
//                return MyFitnessDatabase.Database.UpdateAsync(item);
//            }
//            else
//            {
//                return MyFitnessDatabase.Database.InsertAsync(item);
//            }
//        }

//        public Task<int> DeleteItemAsync(Route item)
//        {
//            return MyFitnessDatabase.Database.DeleteAsync(item);
//        }

//        public async void SaveRoute(Polyline polyLine)
//        {
//            var route = new Route{
//                Value = JsonConvert.SerializeObject(polyLine)
//            };

//            await SaveItemAsync(route);
//        }
//    }
//}
