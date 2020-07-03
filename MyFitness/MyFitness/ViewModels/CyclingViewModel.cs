using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MyFitness.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Xamarin.Forms;

namespace MyFitness.ViewModels
{
    public class CyclingViewModel: BaseViewModel
    {
        public ObservableCollection<Position> Positions { get; set; }

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

        public CyclingViewModel()
        {
            Title = "Cycling";
            Positions = new ObservableCollection<Position>();

            TrackingCommand = new Command(async  ()=> await ExecuteTrackingCommand());

            _locator = CrossGeolocator.Current;
            _locator.DesiredAccuracy = 5;

            TrackingButtonLabel = "Start Tracking";
        }

        private async Task ExecuteTrackingCommand()
        {
            try
            {
                var hasPermission = await XamUtils.CheckPermissions<LocationWhenInUsePermission>();
                if (!hasPermission)
                    return;

                if (TrackingEnabled)
                {
                    TrackingEnabled = false;
                    TrackingButtonLabel = "Start Tracking";

                    await _locator.StopListeningAsync();

                    _locator.PositionChanged -= CrossGeolocator_Current_PositionChanged;
                }
                else
                {
                    var position = await _locator.GetPositionAsync();

                    if (position == null)
                    {
                        TrackingEnabled = false;
                        TrackingButtonLabel = "Start Tracking";
                        return;
                    }

                    TrackingEnabled = true;
                    TrackingButtonLabel = "Stop Tracking";

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