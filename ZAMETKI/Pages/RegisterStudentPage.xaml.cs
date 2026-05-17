using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class RegisterStudentPage : ContentPage
{
    public RegisterStudentPage(RegisterStudentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
