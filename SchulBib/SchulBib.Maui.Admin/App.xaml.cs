using SchulBib.Data;

namespace SchulBib.Maui.Admin
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SchulBibDbContext>();
                dbContext.EnsureDatabaseCreated();
            }

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {


            return new Window(new AppShell());
        }
    }
}