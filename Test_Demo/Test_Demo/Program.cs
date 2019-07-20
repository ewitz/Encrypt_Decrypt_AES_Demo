using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Test_Demo.Models;

namespace Test_Demo
{
    public class Program
    {
        static public IConfiguration configuration { get; set; }
        static public string decryptedString { get; } = Task.Run(MethodToMakeInitialAPICall).Result;

        static IEnumerable<KeyValuePair<string, string>> Descrypted_Strings { get; } =
        new Dictionary<string, string>()
        {
            ["Decrypted_String"] = decryptedString,
            // add as many other key value pairs as you'd like
        };
        public static async Task<string> MethodToMakeInitialAPICall()
        {
            using ( HttpClient httpClient = new HttpClient())
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json").Build();
                Uri baseAddress = new Uri(config["EncryptDecryptService"]);
                httpClient.BaseAddress = baseAddress;
                EncryptDecryptModel model = new EncryptDecryptModel();
                model.cipherText = config["Encrypted_String"];
                string jsonInput = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(jsonInput, System.Text.Encoding.UTF8, "application/json");
                string requestEndPoint = "api/Security/decrypt";
                HttpResponseMessage httpResponse = await httpClient.PostAsync(requestEndPoint, content).ConfigureAwait(true);
                string response = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
                EncryptDecryptModel encryptDecryptModel = JsonConvert.DeserializeObject<EncryptDecryptModel>(response);
                return encryptDecryptModel.plainText;
            }
        }
        public static void Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(Descrypted_Strings);
            configuration = configurationBuilder.Build();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseIISIntegration()
            .ConfigureAppConfiguration((WebHostBuilderContext, ConfigurationBuildy) =>
            {
                IHostingEnvironment env = WebHostBuilderContext.HostingEnvironment;
                ConfigurationBuildy.AddConfiguration(configuration);
            })
            .UseStartup<Startup>();
    }
}
