using System;
using Library.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Library.Api.Tests.Integration;

public class LibraryApiFactory : WebApplicationFactory<IApiMarker>
{
    override protected void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.AddSingleton<IDbConnectionFactory>(_ => 
            new SqliteConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared"));
        });
    }

}
