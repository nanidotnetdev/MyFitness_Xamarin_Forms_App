using System;
using System.Collections.Generic;
using MyFitness.Utilities;
using MyFitness.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace MyFitness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CyclingMapPage : ContentPage
    {
        private CyclingMapViewModel viewModel;
        public CyclingMapPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new CyclingMapViewModel();

            //BindMap();
        }

        //private async void BindMap()
        //{
        //    gmap.Polylines.AddRange(viewModel.RoutesCovered);

        //    GoToCurrentLocation();
        //}

        //private async void GoToCurrentLocation()
        //{
        //    var position = await viewModel.GetCurrentLocation();

        //    var pin = new Pin
        //    {
        //        Type = PinType.SearchResult,
        //        Position = new Position(position.Latitude, position.Longitude),
        //        Label = string.Format("{0}, {1}", position.Latitude, position.Longitude)
        //    };

        //    gmap.Pins.Add(pin);

        //    gmap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude), Distance.FromKilometers(1)));
        //}
    }
}