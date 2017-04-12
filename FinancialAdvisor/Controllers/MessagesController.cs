using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using FinancialAdvisor.Helpers;
using FinancialAdvisor;
using System.Globalization;

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
                await TranslationHandler.DetectAndSetUserLanguageCode(activity);
                if (activity.Text.ToLower() == Resources.Resource.HiString.ToLower())
                {
                    await WelcomeMessage(activity);
                }
                else
                if (activity.Text.ToLower() == Resources.Resource.HelpString.ToLower())
                {
                    await HelpMessage(activity);
                }
                else
                {
                    var userLanguageCode = StateHelper.GetUserLanguageCode(activity);
                    if (userLanguageCode != "en")
                        activity.Text = await TranslationHandler.DoTranslation(activity.Text, userLanguageCode, "en");
                    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                }
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
            var text = string.Concat(string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringFirstLine, message.From.Name),
                Environment.NewLine, string.Format(CultureInfo.CurrentCulture, Resources.Resource.WelcomeStringSecondLine));
            var reply = message.CreateReply(text);
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        //private async Task DisplayLanguage(Activity message)
        //{
        //    var language = StateHelper.GetUserLanguageCode(message);
        //    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
        //    await connector.Conversations.ReplyToActivityAsync(message.CreateReply(language));
        //}

        private async Task HelpMessage(Activity message)
        {
            var text = string.Concat(string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageFirstLine), "\n\n" ,
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageSecondLine), "\n\n" ,
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageThirdLine) , "\n\n" ,
                     string.Format(CultureInfo.CurrentCulture, Resources.Resource.UsageFourthLine));
            var reply = message.CreateReply(text);

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
                await TranslationHandler.DetectAndSetUserLanguageCode(message);
                IConversationUpdateActivity conversationupdate = message;

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    if (conversationupdate.MembersAdded.Any())
                    {
                        var reply = message.CreateReply();
                        foreach (var newMember in conversationupdate.MembersAdded)
                        {
                            if (newMember.Id != message.Recipient.Id)
                            {
                                await WelcomeMessage(message);
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
                await TranslationHandler.DetectAndSetUserLanguageCode(message);
                if (message.Action == "add" && (message.ChannelId == "skype" || message.ChannelId == "facebook"))
                {
                    await WelcomeMessage(message);
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