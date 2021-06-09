﻿using System;
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
            var lex = new AmazonLexModelBuildingServiceClient(args[0], args[1], RegionEndpoint.EUCentral1);
            string[] files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Intents", "*.json", SearchOption.AllDirectories);
            Console.WriteLine($"Number of Intents : {files.Length}");
            foreach (string file in files)
            {
                var intent = JsonConvert.DeserializeObject<PutIntentRequest>(File.ReadAllText(file));
                var res = await lex.PutIntentAsync(intent);
                delamane.Intents.Add(new Intent() { IntentName = intent.Name, IntentVersion =  res.Version});
            }
            await lex.PutBotAsync(delamane);
            await lex.CreateBotVersionAsync(new CreateBotVersionRequest() { Name = delamane.Name });
            foreach (Intent intent in delamane.Intents)
                await lex.CreateIntentVersionAsync(new CreateIntentVersionRequest() { Name = intent.IntentName });
        }
    }
}
