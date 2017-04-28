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
                //if (activity.Text.ToLower().StartsWith(string.Format(CultureInfo.CurrentCulture, Resources.Resource.SpeakString.ToLower()), StringComparison.CurrentCulture))
                //{
                //    if (activity.Text.Split(' ').Length > 1)
                //    {
                //        var lang = activity.Text.Split(' ');
                //        if (await CultureHelper.ChangeCultureAsync(lang[1]))
                //            NewLanguage(activity);
                //    }
                //}
                //else               
                //{
                    string language = await TranslationHelper.DoLanguageDetectionAsync(activity.Text);
                    
                    if (language != "en")
                    {
                        activity.Text = (await TranslationHelper.DoTranslation(activity.Text, language, "en")).ToLower();
                    }

                    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog(language));
                //}
            }
            else
            {
#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
                HandleSystemMessage(activity);
#pragma warning restore CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }             

        private void NewLanguage(Activity message)
        {
            var text = string.Format(CultureInfo.CurrentCulture, Resources.Resource.NewLanguageString); 
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            connector.Conversations.ReplyToActivity(message.CreateReply(string.Format(CultureInfo.CurrentCulture, 
                string.Concat(text, " ", CultureHelper.CurrentCulture.NativeName))));
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

                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    Messages.WelcomeMessage(message, message.From.Name);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                // For Skype and Messenger ?
                if (message.Action == "add" && (message.ChannelId == "skype" || message.ChannelId == "facebook"))
                {
                    Messages.WelcomeMessage(message, message.From.Name);
                }
                if (message.Action == "add")
                {
                    Messages.WelcomeMessage(message, message.From.Name);
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