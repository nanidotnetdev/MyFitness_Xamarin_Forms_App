using MyFitness.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyFitness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CyclingMapPage : ContentPage
    {
        public CyclingMapPage()
        {
            InitializeComponent();

            BindingContext = new CyclingMapViewModel();
        }
    }
}