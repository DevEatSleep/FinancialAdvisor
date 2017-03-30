using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Tab
using FinancialAdvisor.Entity;

namespace FinancialAdvisor
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
       
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {          
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text.ToLower() == "hi")
                    await WelcomeMessage(activity);
                else
                    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
                HandleSystemMessageAsync(activity);
#pragma warning restore CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);           
        }

        private async Task WelcomeMessage(Activity message)
        {
            var reply = message.CreateReply(string.Format("Welcome {0}, I'm Bob, your financial advisor." +
                           Environment.NewLine + "type 'Help' to know what I can do for you", message.From.Name));

            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channelsc

                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    await WelcomeMessage(message);

                    //var reply = message.CreateReply(string.Format("Welcome {0}, I'm Bob, your financial advisor." +
                    //        Environment.NewLine + "type 'Help' to know what I can do for you", message.From.Name));

                    //ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    //await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                // For Skype and Messenger ?
                if (message.Action == "add")
                {
                    await WelcomeMessage(message);
                    //var reply = message.CreateReply(string.Format("Welcome {0}, I'm Bob, your financial advisor." +
                    //    Environment.NewLine + "type 'Help' to know what I can do for you", message.From.Name));

                    //ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    //await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}