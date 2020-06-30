using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MyFitness.Repository;
using MyFitness.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Position = Xamarin.Forms.GoogleMaps.Position;
using GeoPosition = Plugin.Geolocator.Abstractions.Position;

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

        public Map MapView { get; private set; }

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

        private async Task ExecuteTrackingCommand()
        {
            try
            {
                var hasPermission = await XamUtils.CheckPermissions(Permission.Location);
                if (!hasPermission)
                    return;

                var position = await GetCurrentLocation();

                if (TrackingEnabled || position == null)
                {
                    TrackingEnabled = false;
                    TrackingButtonLabel = "Start Tracking";

                    _locator.PositionChanged -= CrossGeolocator_Current_PositionChanged;

                    SaveLastPath();
                }
                else
                {
                    TrackingEnabled = true;
                    TrackingButtonLabel = "Stop Tracking";

                    //new path
                    Positions.Add(position);

                    _locator.PositionChanged += CrossGeolocator_Current_PositionChanged;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.InnerException);
            }
        }

        private void SaveLastPath()
        {
            //save last path
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

            repository.SaveRoute(route);

            Positions.Clear();
        }

        public async Task<GeoPosition> GetCurrentLocation()
        {
            var position = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10));
            //var position = new GeoPosition(47.6381401, -122.1317367);

            return position;
        }

        void CrossGeolocator_Current_PositionChanged(object sender, PositionEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var position = e.Position;
                Positions.Add(position);
                //count++;
                //LabelCount.Text = $"{count} updates";
                //labelGPSTrack.Text = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
                //    position.Timestamp, position.Latitude, position.Longitude,
                //    position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);
            });
        }
    }
}