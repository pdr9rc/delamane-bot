using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.LexModelBuildingService;
using Amazon.LexModelBuildingService.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

namespace DELAMANE_BOT
{
    class Program
    {
        async static Task Main(string[] args)
        {
            PutBotRequest delamane = JsonConvert.DeserializeObject<PutBotRequest>(File.ReadAllText("delamane.json"));
            var awsCredentials = new BasicAWSCredentials(args[0], args[1]);
            var lex = new AmazonLexModelBuildingServiceClient(awsCredentials, RegionEndpoint.EUCentral1);
            string[] files = Directory.GetFiles("Intents", "*.json", SearchOption.AllDirectories);
            foreach (string file in files)
                delamane.Intents.Add(JsonConvert.DeserializeObject<Intent>(File.ReadAllText(file)));
            await lex.PutBotAsync(delamane);
            await lex.CreateBotVersionAsync(new CreateBotVersionRequest() { Name = delamane.Name });
            foreach (Intent intent in delamane.Intents)
                await lex.CreateIntentVersionAsync(new CreateIntentVersionRequest() { Name = intent.IntentName });
        }
    }
}
