using MyFitness.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyFitness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CyclingPage : ContentPage
    {
        public CyclingPage()
        {
            InitializeComponent();

            BindingContext = new CyclingViewModel();
        }
    }
}