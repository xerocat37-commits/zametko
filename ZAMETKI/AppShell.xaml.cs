using ZAMETKI.Api.Contracts;
using ZAMETKI.Pages;
using ZAMETKI.Services;

namespace ZAMETKI
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _auth;

        public AppShell(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;

            Routing.RegisterRoute("roleSelection", typeof(RoleSelectionPage));
            Routing.RegisterRoute("registerStudent", typeof(RegisterStudentPage));
            Routing.RegisterRoute("registerTeacher", typeof(RegisterTeacherPage));
            Routing.RegisterRoute("groupDetails", typeof(GroupDetailsPage));
            Routing.RegisterRoute("groupNoteEditor", typeof(GroupNoteEditorPage));

            _auth.AuthStateChanged += (_, _) => ApplyRoleVisibility();
            ApplyRoleVisibility();
        }

        private void ApplyRoleVisibility()
        {
            var role = _auth.CurrentUser?.Role;
            MyGroupsShellItem.IsVisible = role == UserRoles.Teacher;
            HeadBroadcastShellItem.IsVisible = role == UserRoles.Head;
            FlyoutBehavior = _auth.IsAuthenticated ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled;
        }

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            await _auth.LogoutAsync();
            await GoToAsync("//login");
        }
    }
}
