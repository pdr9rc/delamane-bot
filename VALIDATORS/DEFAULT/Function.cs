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
            var topicArn = lexEvent.SessionAttributes.Where(attr => attr.Key == "TopicArn");
            var sessionAttr = lexEvent.SessionAttributes;
            if (topicArn.Count() <= 0)
            {
                return FullfillIntent(new Dictionary<string, string>());
            }
            return ElicitIntent(sessionAttr ?? new Dictionary<string, string>());
        }

        //TODO: move this to common utils
        public LexResponse ElicitIntent(IDictionary<string, string> sessionAttr)
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttr,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "ElicitIntent",
                    Message =
                    {
                        ContentType = "PlainText",
                        Content = "Some Prompt to trigger other intent"
                    }
                }
            };
        }


        public LexResponse FullfillIntent(IDictionary<string, string> sessionAttr)
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttr,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "Close",
                }
            };
        }
    }
}
