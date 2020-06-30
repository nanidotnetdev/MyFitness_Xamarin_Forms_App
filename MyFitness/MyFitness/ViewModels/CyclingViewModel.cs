using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MyFitness.Utilities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
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
            get { return _trackingEnabled; }
            set { SetProperty(ref _trackingEnabled, value); }
        }

        private string _trackingButtonLabel;

        public string TrackingButtonLabel { get { return _trackingButtonLabel; } 
            set
            {
                SetProperty(ref _trackingButtonLabel, value);
            }
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
                var hasPermission = await XamUtils.CheckPermissions(Permission.Location);
                if (!hasPermission)
                    return;

                if (TrackingEnabled)
                {
                    TrackingEnabled = false;
                    TrackingButtonLabel = "Start Tracking";
                    _locator.PositionChanged -= CrossGeolocator_Current_PositionChanged;
                }
                else
                {
                    //var position = await locator.GetPositionAsync();
                    var position = new Position(44.986656, -93.258133);

                    if (position == null)
                    {
                        TrackingEnabled = false;
                        TrackingButtonLabel = "Start Tracking";
                        return;
                    }

                    TrackingEnabled = true;
                    TrackingButtonLabel = "Stop Tracking";

                    Positions.Add(position);

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
                //count++;
                //LabelCount.Text = $"{count} updates";
                //labelGPSTrack.Text = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
                //    position.Timestamp, position.Latitude, position.Longitude,
                //    position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);
            });
        }
    }
}