using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

//IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres",
//        builder.CreateResourceBuilder(new ParameterResource("username", _ => "postgres")),
//        builder.CreateResourceBuilder(new ParameterResource("password", _ => "Password12!")))
//    .WithHealthCheck()
//    .WithDataVolume()
//    .WithPgAdmin();

//IResourceBuilder<PostgresDatabaseResource> backEndDb = postgres.AddDatabase("sample", "sample");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

IResourceBuilder<ProjectResource> backEnd = builder.AddProject<Sample_BackEnd>("SampleBackEnd")
    //.WithReference(backEndDb)
    .WithReference(rabbitMq)
    //.WaitFor(postgres)
    .WaitFor(rabbitMq);

IResourceBuilder<ProjectResource> api = builder.AddProject<Sample_Api>("SampleApi")
    //.WithReference(backEndDb)
    .WithReference(rabbitMq)
    //.WaitFor(postgres)
    .WaitFor(rabbitMq);

builder.Build().Run();