using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MoggleFunctions
{
    public static class Functions
    {
        [FunctionName("GetGame")]
        public static async Task<IActionResult> GetGame(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            [CosmosDB(
                databaseName: "Moggle",
                collectionName: "Game",
                ConnectionStringSetting = "DBConnectionString",
                SqlQuery = "Select * from c Where c.GameId={id} ORDER BY c._ts DESC"
            )]
            IEnumerable<Game> games,
            ILogger log,
            string id
            )
        {
            if (games == null)
            {
                log.LogInformation("No Games found from Moggle Games");
                return new NotFoundResult();
            }

            log.LogInformation("Games fetched from Moggle Games");
            return new OkObjectResult(games);
        }



        //[FunctionName("Function1")]
        //public static async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}
    }
}
