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
                    return Close(sessionAttr);
                case "DialogCodeHook":
                    return Validate(sessionAttr, lexEvent.InputTranscript, lexEvent.CurrentIntent.Slots);
                default:
                    throw new InvalidOperationException();
            }
        }

        //TODO: move this to common utils
        public LexResponse Validate(IDictionary<string, string> sessionAttr, string input, IDictionary<string, string> slots)
        {
            Console.WriteLine("----------- Validate SESSSION ATTR ---------------");

            var res = new LexResponse();
            res.SessionAttributes = sessionAttr;
            var dialogAction = new LexResponse.LexDialogAction();
            dialogAction.Slots = (slots != null)? slots : new Dictionary<string, string>();
            dialogAction.Message = new LexResponse.LexMessage();
            //TODO, does this work?
            dialogAction.Message.ContentType = "PlainText";
            if (dialogAction.Slots["confirm"] != null)
                if (dialogAction.Slots["confirm"] != "yes")
                {
                    dialogAction.Type = "ElicitIntent";
                    res.DialogAction = dialogAction;
                    dialogAction.Message.Content = sessionAttr["RemediationText"];
                    dialogAction.Slots["confirm"] = null;
                    dialogAction.Slots["RemediationIndex"] = null;
                    return res;
                }
                else
                {
                    return Close(sessionAttr);
                }
            int code = Validator.Validate(input, sessionAttr);
            if (code == -1)
            {
                Console.WriteLine("Reached the breach");
                dialogAction.Type = "ElicitIntent";
                res.DialogAction = dialogAction;
                dialogAction.Slots["RemediationIndex"] = null;
                dialogAction.Message.Content = "Invalid option selection, go fuck yourself and select something valid!";
                return res;
            }
            dialogAction.Type = "ElicitSlot";
            dialogAction.IntentName = "OptionIntent";
            if (dialogAction.Slots["RemediationIndex"] == null)
                dialogAction.Slots["RemediationIndex"] = code.ToString();
            dialogAction.SlotToElicit = "confirm";
            List<string> rems = JsonConvert.DeserializeObject<List<string>>(sessionAttr["RemediationOptions"]);
            dialogAction.Message.Content = $"You have decided to {rems[code - 1]}.";
            res.DialogAction = dialogAction;
            return res;
        }

        public LexResponse Close(IDictionary<string, string> sessionAttr)
        {
            Console.WriteLine("----------- Close SESSSION ATTR ---------------");
            Console.WriteLine(sessionAttr);
            Console.WriteLine(sessionAttr == null ? "is null" : "not null");
            var res = new LexResponse();
            res.SessionAttributes = sessionAttr;
            var dialogAction = new LexResponse.LexDialogAction();
            dialogAction.Message = new LexResponse.LexMessage();
            dialogAction.Type = "Close";
            dialogAction.FulfillmentState = "Fulfilled";
            res.DialogAction = dialogAction;
            return res;
        }
    }
}
