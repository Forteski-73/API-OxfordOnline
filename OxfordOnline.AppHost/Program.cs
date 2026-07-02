var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OxfordOnline>("oxfordonline");

builder.Build().Run();
