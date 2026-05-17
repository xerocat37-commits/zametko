using Microsoft.Extensions.Logging;
using ZAMETKI.Api;
using ZAMETKI.Pages;
using ZAMETKI.Services;
using ZAMETKI.ViewModels;

namespace ZAMETKI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(AppConfig.ApiBaseUrl)
            });
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<NotesApi>();
            builder.Services.AddSingleton<GroupsApi>();
            builder.Services.AddSingleton<GroupMembersApi>();
            builder.Services.AddSingleton<GroupNotesApi>();
            builder.Services.AddSingleton<GlobalNotesApi>();
            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterStudentViewModel>();
            builder.Services.AddTransient<RegisterTeacherViewModel>();
            builder.Services.AddTransient<PersonalNotesViewModel>();
            builder.Services.AddTransient<FeedViewModel>();
            builder.Services.AddTransient<MyGroupsViewModel>();
            builder.Services.AddTransient<GroupDetailsViewModel>();
            builder.Services.AddTransient<GroupNoteEditorViewModel>();
            builder.Services.AddTransient<HeadBroadcastViewModel>();
            builder.Services.AddTransient<NoteEditorViewModel>();
            builder.Services.AddTransient<PersonalNoteEditorViewModel>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RoleSelectionPage>();
            builder.Services.AddTransient<RegisterStudentPage>();
            builder.Services.AddTransient<RegisterTeacherPage>();
            builder.Services.AddTransient<PersonalNotesPage>();
            builder.Services.AddTransient<FeedPage>();
            builder.Services.AddTransient<MyGroupsPage>();
            builder.Services.AddTransient<HeadBroadcastPage>();
            builder.Services.AddTransient<GroupDetailsPage>();
            builder.Services.AddTransient<GroupNoteEditorPage>();
            builder.Services.AddTransient<PersonalNoteEditorPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
