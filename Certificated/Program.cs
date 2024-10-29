// y el Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                        },
                    new List<string>()
                  }
                });

    //NOTA: Este "V1" debe ser igual al que ponemos más adelante en "c.SwaggerEndpoint($"/swagger/V1/swagger.json", "V1.0");"

    c.SwaggerDoc("V1", new OpenApiInfo
    {
        //Estos datos los recogemos del appSettings
        Version = Utils.Configuration["Swagger:Document:Info:Version"],
        Title = Utils.Configuration["Swagger:Document:Info:Title"],
        Contact = new OpenApiContact()
        {
            Email = "",
            Name = "Name",
            Url = new Uri("http://www.WEB.es")
        },
        Description = Utils.Configuration["Swagger:Document:Info:Description"]
    });
    c.ExampleFilters();

    //[ALR] Dejaremos que se genere un error para resolverlo
    //c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(type => type.FullName);
    //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{xmlFilename}");

    //Usando esto y marcando la opcion de que genere el xml del proyecto que querremos nos mostrara la informacion del esquema que tengamos definido
    List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
    xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));

    //[FCP] Sustituye el nº de Version en la URL
    c.OperationFilter<RemoveVersionParameterFilter>();
    c.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();

    c.EnableAnnotations();
    //c.AddServer(new OpenApiServer()
    //{
    //    Url = "http://localhost:2222/api/v1"
    //});

    // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
    // Use [ValidateModelState] on Actions to actually validate it in C# as well!
    c.OperationFilter<GeneratePathParamsValidationFilter>();
    c.DocumentFilter<JsonPatchDocumentFilter>();

    c.OperationFilter<AcceptedLanguageHeader>();
    //Remove irrelevent schemas
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
builder.Services.AddSwaggerGen();


var runningAsService = Utils.isRunningAsService(args);
if (runningAsService)
{
    Log.Information($"Starting {AppDomain.CurrentDomain.FriendlyName} web host as windows service");
    builder.Host.ConfigureServices((context, services) =>
    {
        services.Configure<KestrelServerOptions>(context.Configuration.GetSection("Kestrel"));
    })
    .UseWindowsService();
}
else
{
    Log.Information($"Starting {AppDomain.CurrentDomain.FriendlyName}");
}
#endregion Initializing Services

#region Initializing Middlewares

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRequestLocalization(options =>
{
    //[ALR] If DefaultRequestCulture  is not invariant, we can have problems between different machines
    options.DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture);
    options.ApplyCurrentCultureToResponseHeaders = true;
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr")
        // Agrega más culturas según tus necesidades
    };
    options.SupportedUICultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr")
        // Agrega más culturas según tus necesidades
    };
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});
