﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using EHospital.Medications.BusinessLogic.Contracts;
using EHospital.Medications.BusinessLogic.Services;
using EHospital.Medications.Data;
using EHospital.Medications.Model;


namespace EHospital.Medications.WebAPI
{
    public class Startup
    {
        ///* Swagger constants
        private const string VERSION = "v.1.0";
        private const string API_NAME = "eHealth.Medications.API";
        //*/

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connection = this.Configuration.GetConnectionString("EHospitalDB");
            services.AddDbContext<MedicationDbContext>(options => options.UseSqlServer(connection));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepository<Drug>, Repository<Drug>>();
            services.AddScoped<IRepository<Prescription>, Repository<Prescription>>();
            services.AddScoped<IDrugService, DrugService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            ///* Swagger Setting
            Info info = new Info
            {
                Version = VERSION,
                Title = API_NAME,
                Description = "Microservic contain business logic "
                            + "to manage medications and prescriptions to patient.",
                Contact = new Contact()
                {
                    Name = "Serhii Maksymchuk",
                    Email = "smakdealcase@gmail.com",
                    Url = "https://github.com/smoukiDev/DP148.eHealth.Medications"
                }
            };
            services.AddSwaggerGen(c => { c.SwaggerDoc(VERSION, info); });
            //*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            ///* Swagger Setting
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{VERSION}/swagger.json", API_NAME);
            });
            //*/
        }
    }
}