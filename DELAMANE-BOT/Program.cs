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
        //TODO, add checksums, add putbotalias
        async static Task Main(string[] args)
        {

            PutBotRequest delamane = JsonConvert.DeserializeObject<PutBotRequest>(File.ReadAllText("delamane.json"));
            delamane.CreateVersion = true;
            var lex = new AmazonLexModelBuildingServiceClient(args[0], args[1], RegionEndpoint.EUCentral1);
            try
            {
  
                delamane.Checksum = (await lex.GetBotAsync(new GetBotRequest() { 
                    Name = delamane.Name,
                    VersionOrAlias = (await lex.GetBotVersionsAsync(new GetBotVersionsRequest() { Name = delamane.Name, MaxResults = 1})).Bots[0].Version
                })).Checksum;
            }
            catch {}
            string[] files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Intents", "*.json", SearchOption.AllDirectories);
            Console.WriteLine($"Number of Intents : {files.Length}");
            foreach (string file in files)
            {
                var intent = JsonConvert.DeserializeObject<PutIntentRequest>(File.ReadAllText(file));
                try
                {
                    intent.Checksum = (await lex.GetIntentAsync(new GetIntentRequest() { 
                        Name = intent.Name,
                        Version = (await lex.GetIntentVersionsAsync(new GetIntentVersionsRequest() { Name = intent.Name, MaxResults = 1 })).Intents[0].Version
                    })).Checksum;
                }
                catch {}
                intent.CreateVersion = true;
                var res = await lex.PutIntentAsync(intent);
                delamane.Intents.Add(new Intent() { IntentName = intent.Name, IntentVersion =  res.Version});
            }
            var _res = await lex.PutBotAsync(delamane);
            //await lex.CreateBotVersionAsync(new CreateBotVersionRequest() { Name = delamane.Name });
            //foreach (Intent intent in delamane.Intents)
                // lex.CreateIntentVersionAsync(new CreateIntentVersionRequest() { Name = intent.IntentName });
        }
    }
}
