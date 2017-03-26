using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System;

namespace FinancialAdvisor
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private List<string> _usersList = new List<string>();
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            //var response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;
        }

        private Activity HandleSystemMessage(Activity message)
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
                if (message.MembersAdded.Count > _usersList.Count)
                {
                    if (!_usersList.Contains(message.From.Id))
                    {
                        _usersList.Add(message.From.Id);
                        ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));
                        Activity reply = message.CreateReply(string.Format("Welcome {0}, I'm Bob, your financial advisor." +
                            Environment.NewLine + "type 'Help' to know what I can do for you", message.From.Name));
                        connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
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