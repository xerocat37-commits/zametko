namespace ZAMETKI.Pages;

public partial class RoleSelectionPage : ContentPage
{
    public RoleSelectionPage()
    {
        InitializeComponent();
    }

    private async void OnStudentClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("registerStudent");

    private async void OnTeacherClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("registerTeacher");
}
