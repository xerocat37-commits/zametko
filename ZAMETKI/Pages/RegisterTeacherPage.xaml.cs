using ZAMETKI.ViewModels;

namespace ZAMETKI.Pages;

public partial class RegisterTeacherPage : ContentPage
{
    public RegisterTeacherPage(RegisterTeacherViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
