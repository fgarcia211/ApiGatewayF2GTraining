using ApiGatewayF2GTraining.Data;
using ApiGatewayF2GTraining.Helpers;
using ApiGatewayF2GTraining.Repositories;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ApiGatewayF2GTraining;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        string connectionString = HelperSecretManager.GetSecretAsync("MySqlF2G").Result;

        services.AddDbContext<F2GDataBaseContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        services.AddTransient<IRepositoryF2GTraining, RepositoryF2GTraining>();

        services.AddSingleton<HelperOAuthToken>();
        HelperOAuthToken helper = new HelperOAuthToken(Configuration);
        services.AddAuthentication(helper.GetAuthenticationOptions()).AddJwtBearer(helper.GetJwtOptions());
   
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(document => {

            document.Title = "Api OAuth F2G Training";
            document.Description = "Api de F2G Training";

            document.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Copia y pega el Token en el campo 'Value:' así: Bearer {Token JWT}."
            });

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        services.AddCors(p => p.AddPolicy("corsenabled", builder =>
        {
            builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
        }));

        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.InjectStylesheet("/css/bootstrap.css");
            options.InjectStylesheet("/css/material3x.css");
            options.SwaggerEndpoint(
                url: "swagger/v1/swagger.json", name: "Api v1");
            options.RoutePrefix = "";
            options.DocExpansion(DocExpansion.None);
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("corsenabled");
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}