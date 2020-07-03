using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyFitness.Models;
using MyFitness.Repository;
using MyFitness.Utilities;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Position = Xamarin.Forms.GoogleMaps.Position;
using GeoPosition = Plugin.Geolocator.Abstractions.Position;
using Map = Xamarin.Forms.GoogleMaps.Map;

namespace MyFitness.ViewModels
{
    public class CyclingMapViewModel: BaseViewModel
    {
        public Command TrackingCommand { get; set; }

        private bool _trackingEnabled = false;

        public bool TrackingEnabled
        {
            get => _trackingEnabled;
            set => SetProperty(ref _trackingEnabled, value);
        }

        private string _trackingButtonLabel;

        public string TrackingButtonLabel
        {
            get => _trackingButtonLabel;
            set => SetProperty(ref _trackingButtonLabel, value);
        }

        private IGeolocator _locator;

        private ObservableCollection<GeoPosition> _positions = new ObservableCollection<GeoPosition>();

        private ObservableCollection<GeoPosition> Positions
        {
            get => _positions;
            set => SetProperty(ref _positions, value);
        }

        public Map MapView { get; }

        private MyFitnessDatabase repository;

        public CyclingMapViewModel()
        {
            Title = "Cycling Map";

            TrackingCommand = new Command(async () => await ExecuteTrackingCommand());

            _locator = CrossGeolocator.Current;
            _locator.DesiredAccuracy = 5;

            TrackingButtonLabel = "Start Tracking";

            repository = new MyFitnessDatabase();
            MapView = new Map();

            //get routes
            var routes = repository.GetAllPolyLines();
            MapView.Polylines.AddRange(routes);

            GoToCurrentLocation();
        }

        private async void GoToCurrentLocation()
        {
            var position = await GetCurrentLocation();

            var pin = new Pin
            {
                Type = PinType.SearchResult,
                Position = new Position(position.Latitude, position.Longitude),
                Label = string.Format("{0}, {1}", position.Latitude, position.Longitude)
            };

            MapView.Pins.Add(pin);

            MapView.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude), Distance.FromKilometers(1)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteTrackingCommand()
        {
            try
            {
                var hasPermission = await XamUtils.CheckPermissions<LocationWhenInUsePermission>();
                if (!hasPermission)
                    return;

                var position = await GetCurrentLocation();

                if (TrackingEnabled || position == null)
                {
                    TrackingEnabled = false;
                    TrackingButtonLabel = "Start Tracking";

                    await _locator.StopListeningAsync();

                    _locator.PositionChanged -= CrossGeolocator_Current_PositionChanged;

                    SaveLastPath();
                }
                else
                {
                    TrackingEnabled = true;
                    TrackingButtonLabel = "Stop Tracking";

                    //new path
                    Positions.Add(position);

                    await _locator.StartListeningAsync(TimeSpan.FromSeconds(5), 5);

                    _locator.PositionChanged += CrossGeolocator_Current_PositionChanged;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.InnerException);
            }
        }

        /// <summary>
        /// save last path
        /// </summary>
        private void SaveLastPath()
        {
            if (Positions.Count > 5)
            {
                List<Position> geoPositions = Positions
                    .Select(p => new Position(p.Latitude, p.Longitude)).ToList();

                Random rnd = new Random();

                Polyline route = new Polyline
                {
                    StrokeColor = Color.FromRgba(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256)),
                    StrokeWidth = 8
                };

                route.Positions.AddRange(geoPositions);

                MapView.Polylines.Add(route);

                Route viewModel = new Route
                {
                    Value = JsonConvert.SerializeObject(route)
                };

                repository.SaveItemAsync(viewModel);
            }

            Positions.Clear();
        }

        /// <summary>
        /// Calculate Route Distance.
        /// not a driving directions
        /// </summary>
        /// <returns></returns>
        private double CalculateDistance()
        {
            var start = Positions.OrderBy(p => p.Timestamp).First();
            var destination = Positions.OrderBy(p => p.Timestamp).Last();

            var distance = Location.CalculateDistance(start.Latitude, start.Longitude,
                destination.Latitude, destination.Longitude, DistanceUnits.Miles);

            return distance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<GeoPosition> GetCurrentLocation()
        {
            var position = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10));

            return position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CrossGeolocator_Current_PositionChanged(object sender, PositionEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var position = e.Position;
                Positions.Add(position);
            });
        }
    }
}