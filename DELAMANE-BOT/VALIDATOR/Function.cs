using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DEFAULT
{
    public class Function
    {
        private readonly string[] CONFIRM_STATE_VALUES = { "yes", "1" };
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public LexResponse FunctionHandler(LexEvent lexEvent, ILambdaContext context)
        {
            
            var sessionAttr = lexEvent.SessionAttributes ?? new Dictionary<string, string>();
            Console.WriteLine("----------- HANDLER SESSSION ATTR ---------------");
            
            switch (lexEvent.InvocationSource)
            {
                case "FulfillmentCodeHook":
                    return Close(sessionAttr, lexEvent.CurrentIntent.Slots);
               case "DialogCodeHook":
                    return Validate(sessionAttr, lexEvent.InputTranscript, lexEvent.CurrentIntent.Slots);
               default:
                    Console.WriteLine("DEFAULT HOOK ");
                    throw new InvalidOperationException();
            }
        }

        //TODO: move this to common utils
        public LexResponse Validate(IDictionary<string, string> sessionAttr, string input, IDictionary<string, string> slots)
        {
            Console.WriteLine("----------- Validate Handler ---------------");
            var res = new LexResponse();
            res.SessionAttributes = sessionAttr;
            var dialogAction = new LexResponse.LexDialogAction();
            //dialogAction.Slots = (slots != null)? slots : new Dictionary<string, string>();
            

            if (slots["confirm"] != null)
            {
                if (!CONFIRM_STATE_VALUES.Contains(slots["confirm"]))//dialogAction.Slots["confirm"] != "yes")
                {
                    Console.WriteLine("NO CONFIRM STATE");
                    dialogAction.Type = "ElicitIntent";
                    dialogAction.Message = new LexResponse.LexMessage();
                    dialogAction.Message.ContentType = "PlainText";
                    dialogAction.Message.Content = sessionAttr["RemediationText"];
                    res.DialogAction = dialogAction;
                    return res;
                }
                else
                {
                    Console.WriteLine("YES CONFIRM STATE");
                    dialogAction.Type = "Delegate";
                    dialogAction.Slots = (slots != null) ? slots : new Dictionary<string, string>();
                    res.DialogAction = dialogAction;
                    Console.WriteLine(JsonConvert.SerializeObject(res));
                    return res;
                }
            }

            int code = Validator.Validate(input, sessionAttr);

            if (code == -1)
            {
                Console.WriteLine("INVALID STATE");
                dialogAction.Type = "ElicitIntent";
                dialogAction.Message = new LexResponse.LexMessage();
                dialogAction.Message.ContentType = "PlainText";
                dialogAction.Message.Content = "Invalid option selection, go fuck yourself and select something valid!";
                res.DialogAction = dialogAction;
                return res;
            }

            Console.WriteLine("CONFIRM QUESTION STATE");

            dialogAction.Type = "ElicitSlot";
            dialogAction.IntentName = "OptionIntent";
            dialogAction.Slots = (slots != null) ? slots : new Dictionary<string, string>();
            if (dialogAction.Slots["RemediationIndex"] == null)
                dialogAction.Slots["RemediationIndex"] = code.ToString();

            dialogAction.Slots["confirm"] = null;
            dialogAction.SlotToElicit = "confirm";
            List<string> rems = JsonConvert.DeserializeObject<List<string>>(sessionAttr["RemediationOptions"]);
            dialogAction.Message = new LexResponse.LexMessage();
            dialogAction.Message.ContentType = "PlainText";
            dialogAction.Message.Content = $"You have decided to {rems[code - 1]}. Say yes, or press 1 to confirm. Say no or press 2 to retry";
            res.DialogAction = dialogAction;
            return res;
        }

        public LexResponse Close(IDictionary<string, string> sessionAttr, IDictionary<string, string> slots)
        {
            Console.WriteLine("----------- Close SESSSION ATTR ---------------");
            var res = new LexResponse();
            res.SessionAttributes = sessionAttr;
            var dialogAction = new LexResponse.LexDialogAction();
            dialogAction.Type = "Close";
            dialogAction.FulfillmentState = "Fulfilled";
            res.DialogAction = dialogAction;
            Console.WriteLine(JsonConvert.SerializeObject(res));
            return res;
        }
    }
}
  