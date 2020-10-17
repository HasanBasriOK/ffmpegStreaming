using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace streamtest
{

    public class Startup
    {
        static string url = @"video url";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    WebRequest request = WebRequest.Create(url);
                    var response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    context.Response.ContentType = "audio/mp3";
                    context.Response.StatusCode = 200;

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = false,
                            Arguments = "-i - -f mp3 -",
                            FileName = @"ffmpeg.exe path"
                        },
                        EnableRaisingEvents = true,
                    };
                    process.Start();
                    var inputTask = responseStream.CopyToAsync(process.StandardInput.BaseStream).ContinueWith(c => process.StandardInput.Close());
                    var outputTask = process.StandardOutput.BaseStream.CopyToAsync(context.Response.Body);
                    Task.WaitAll(inputTask, outputTask);
                    process.WaitForExit();
                });
            });
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
