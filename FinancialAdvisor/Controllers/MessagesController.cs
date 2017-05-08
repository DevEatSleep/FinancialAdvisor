using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using FinancialAdvisor.Helpers;
using System.Globalization;
using System.Resources;
using System.Collections.Generic;
using System.Threading;
using FinancialAdvisor.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Diagnostics;
using System.Reflection;

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
                var UiLanguage =  StateHelper.GetUserUiLanguage(activity);
                var neutralLanguage = 
                    Assembly.GetExecutingAssembly().GetCustomAttribute<NeutralResourcesLanguageAttribute>().CultureName.Substring(0, 2);
                if (UiLanguage == null)
                {
                    UiLanguage = neutralLanguage;
                    await StateHelper.SetUserUiLanguageAsync(activity, UiLanguage);
                }                    
                if (UiLanguage != neutralLanguage)
                {
                    activity.Text = await TranslationHelper.DoTranslation(activity.Text, UiLanguage, neutralLanguage);
                }
                await Conversation.SendAsync(activity, () => new RootDialog());               
            }
            else
            {
                await HandleSystemMessageAsync(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
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

                IConversationUpdateActivity conversationupdate = message;

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (conversationupdate.MembersAdded.Any())
                    {
                        foreach (var newMember in conversationupdate.MembersAdded)
                        {
                            if (newMember.Id != message.Recipient.Id)
                            {
                                await Messages.WelcomeMessageAsync(message);
                            }
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                // For Skype and Messenger ?
                //if (message.Action == "add" && (message.ChannelId == "skype" || message.ChannelId == "facebook"))
                //{
                //    Messages.WelcomeMessage(message, message.From.Name);
                //}
                //if (message.Action == "add")
                //{
                //    Messages.WelcomeMessage(message, message.From.Name);
                //}
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