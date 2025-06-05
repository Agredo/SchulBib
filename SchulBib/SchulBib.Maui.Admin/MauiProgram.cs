using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SchulBib.Data;
using SchulBib.Data.Repository;
using SchulBib.Data.Repository.Abstractions;
using FileSystem = AgredoApplication.MVVM.Services.Maui.IO.FileSystem;
using IFileSystem = AgredoApplication.MVVM.Services.Abstractions.IO.IFileSystem;
using IPreferences = AgredoApplication.MVVM.Services.Abstractions.Storage.IPreferences;
using Preferences = AgredoApplication.MVVM.Services.Maui.Storage.Preferences;

namespace SchulBib.Maui.Admin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            
            ConfigureServices(builder.Services);


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register the SchulBibDbContext with SQLite database configuration
            services.AddDbContext<SchulBibDbContext>();
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register other services as needed

            services.AddSingleton<IPreferences, Preferences>();
            services.AddSingleton<IFileSystem, FileSystem>();
        }
    }
}
