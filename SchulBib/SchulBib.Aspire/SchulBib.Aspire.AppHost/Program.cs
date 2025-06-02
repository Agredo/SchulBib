var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SchulBib_Backend>("schulbib-backend");

builder.Build().Run();
