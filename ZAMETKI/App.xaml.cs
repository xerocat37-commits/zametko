using ZAMETKI.Services;

namespace ZAMETKI
{
    public partial class App : Application
    {
        private readonly AuthService _auth;
        private readonly AppShell _shell;

        public App(AuthService auth, AppShell shell)
        {
            InitializeComponent();
            _auth = auth;
            _shell = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await Task.Yield();
            try
            {
                var ok = await _auth.RestoreAsync();
                if (ok)
                    await Shell.Current.GoToAsync("//main");
            }
            catch
            {
            }
        }
    }
}
