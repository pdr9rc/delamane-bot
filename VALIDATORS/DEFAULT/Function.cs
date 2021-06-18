using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;

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
            Console.WriteLine(sessionAttr);
            Console.WriteLine(sessionAttr == null ? "is null": "not null");
            var topicArn = lexEvent.SessionAttributes.Where(attr => attr.Key == "TopicArn");
            
            if (topicArn.Count() <= 0)
            {
                return FulfillIntent(sessionAttr);
            }
            return ElicitIntent(sessionAttr);
        }

        //TODO: move this to common utils
        public LexResponse ElicitIntent(IDictionary<string, string> sessionAttr)
        {
            Console.WriteLine("----------- ElicitIntent SESSSION ATTR ---------------");
            Console.WriteLine(sessionAttr);
            Console.WriteLine(sessionAttr == null ? "is null" : "not null");
            var res = new LexResponse();
            res.SessionAttributes = sessionAttr;
            var dialogAction = new LexResponse.LexDialogAction();
            dialogAction.Message = new LexResponse.LexMessage();
            dialogAction.Type = "ElicitIntent";
            dialogAction.Message.ContentType = "PlainText";
            dialogAction.Message.Content = "Some Prompt to trigger other intent";
            res.DialogAction = dialogAction;
            return res;
        }


        public LexResponse FulfillIntent(IDictionary<string, string> sessionAttr)
        {
            Console.WriteLine("----------- FullfillIntent SESSSION ATTR ---------------");
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
