﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.XSS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolrNet;
using UploadImage;
using Withyun.Infrastructure.Data;
using Withyun.Infrastructure.Services;
using Withyun.Infrastructure.Utility;

namespace Withyun.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration,ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public LoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BlogContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly("Withyun.Web")));

            Logger.logger = LoggerFactory.CreateLogger<Logger>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<UploadImageUtility>();
            services.AddSingleton<HtmlSanitizer>();

            services.AddScoped<AccountService>();
            services.AddScoped<BlogService>();
            services.AddScoped<CollectionService>();
            services.AddScoped<FollowService>();
            services.AddScoped<ImageUrlService>();
            services.AddScoped<LinkInvalidService>();
            services.AddScoped<LinkService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<RecommentService>();
            services.AddScoped<ReportService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<VoteDownService>();
            services.AddScoped<VoteUpService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSolrNet(Configuration["SolrUrl"]);
            services.Configure<UploadProfile>(Configuration.GetSection("UploadProfile"));
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
