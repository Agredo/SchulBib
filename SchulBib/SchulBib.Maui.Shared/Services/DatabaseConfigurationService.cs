using IPreferences = AgredoApplication.MVVM.Services.Abstractions.Storage.IPreferences;
using SchulBib.Data;
using SchulBib.Models.Entities.Enums;

namespace SchulBib.Maui.Shared.Services;

public class DatabaseConfigurationService
{
    private const string DB_TYPE_SETTING_KEY = "DatabaseType";
    private const string CONNECTION_STRING_KEY = "ConnectionString";

    private DatabaseType DatabaseType { get; set; } = DatabaseType.SQLite; // Default to SQLite

    public IServiceProvider ServiceProvider { get; private set; }
    public IPreferences Preferences { get; private set; }
    public string connectionString { get; private set; }
    public SchulBibDbContext DBContext { get; private set; }

    public DatabaseConfigurationService(IServiceProvider serviceProvider, IServiceCollection services, IPreferences preferences)
    {
        this.ServiceProvider = serviceProvider;
        this.Preferences = preferences;

        var containsDatabaseType = Preferences.ContainsKey(DB_TYPE_SETTING_KEY, DB_TYPE_SETTING_KEY);
        var containsConnectionString = Preferences.ContainsKey(CONNECTION_STRING_KEY, CONNECTION_STRING_KEY);

        if (containsDatabaseType && containsConnectionString)
        {
            var dbTypeString = Preferences.Get(DB_TYPE_SETTING_KEY, nameof(DatabaseType.SQLite), DB_TYPE_SETTING_KEY);
            var type = DatabaseType.SQLite;
            DatabaseType.TryParse(dbTypeString, out type);
            this.DatabaseType = type;

            if (this.DatabaseType == DatabaseType.SQLite)
            {
                this.connectionString = Preferences.Get(CONNECTION_STRING_KEY, string.Empty, CONNECTION_STRING_KEY);
                
            }
            else if (this.DatabaseType == DatabaseType.PostgreSQL)
            {
                this.connectionString = Preferences.Get(CONNECTION_STRING_KEY, string.Empty, CONNECTION_STRING_KEY);
            }
        }
        else
        {
            switch(DatabaseType)
                {
                case DatabaseType.SQLite:
                    this.connectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "SchulBib.sqlite")}";
                    Preferences.Set(DB_TYPE_SETTING_KEY, nameof(DatabaseType.SQLite), DB_TYPE_SETTING_KEY);
                    Preferences.Set(CONNECTION_STRING_KEY, this.connectionString, CONNECTION_STRING_KEY);
                    break;
                case DatabaseType.PostgreSQL:
                    this.connectionString = "Host=localhost;Database=schulbib;Username=postgres;Password=yourpassword";
                    Preferences.Set(DB_TYPE_SETTING_KEY, nameof(DatabaseType.PostgreSQL), DB_TYPE_SETTING_KEY);
                    Preferences.Set(CONNECTION_STRING_KEY, this.connectionString, CONNECTION_STRING_KEY);
                    break;
            }
        }
    }

    public async Task InitializeDatabaseContext()
    {
        using var scope = ServiceProvider.CreateScope();
        DBContext = scope.ServiceProvider.GetRequiredService<SchulBibDbContext>();

    }

    public async Task SetDatabaseTypeAsync(DatabaseType databaseType, string connectionString)
    {
        this.DatabaseType = databaseType;
        this.connectionString = connectionString;
        Preferences.Set(DB_TYPE_SETTING_KEY, nameof(this.DatabaseType), DB_TYPE_SETTING_KEY);
        Preferences.Set(CONNECTION_STRING_KEY, this.connectionString, CONNECTION_STRING_KEY);


    }
}
