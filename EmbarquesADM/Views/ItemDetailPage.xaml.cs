using System.ComponentModel;
using Xamarin.Forms;
using EmbarquesADM.ViewModels;

namespace EmbarquesADM.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}