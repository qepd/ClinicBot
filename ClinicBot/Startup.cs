// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.14.0

using ClinicBot.Data;
using ClinicBot.Dialogs;
using ClinicBot.Infraestructure.Luis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClinicBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Agregamos AzureBlobStorage
            var storage = new AzureBlobStorage(
                Configuration.GetSection("StorageConnectionString").Value,
                Configuration.GetSection("StorageContainer").Value
                );


            //Inicializamos una variable para el estado del usuario e implementamos con Singleton
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            //Inicializamos una variable para el estado de la converación
            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);
            
            
            //Agregamos la compatibilidad con MVC 
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);

            //habilitamos para conectarnos a cosmodb
            services.AddDbContext<DataBaseService>(options =>
            options.UseCosmos("https://clinicbot-cosmos-db1.documents.azure.com:443/","w99kvGaIEH2AczRm7F5Khi2gtUF1pUgCotmsEDCuSGiQoHBs6vkIQcWVftKuTBoqYHrkdHSXqcy6agsbOVt1Fg==", "botdb"));

            services.AddScoped<IDataBaseService, DataBaseService>();


            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //Asociamos la Interfaz que hemos creado para poder invocarla desde cualquier metodo que querramos
            //utilizar
            services.AddSingleton<ILuisServices, LuisService>();
            services.AddTransient<RootDialogs>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, ClinicBot<RootDialogs>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
