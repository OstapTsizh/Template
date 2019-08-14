using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Interfaces;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.VisualBasic;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// This dialog is responsible for the communication about mailing
    /// information to the user's email.
    /// </summary>
    public class MailingDialog : ComponentDialog// CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly IEmailSender EmailSender;
        private readonly ThreadedLogger _myLogger;
        private StuddyBotContext _db;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private string userEmail;
        private string validationCode;
        private bool isNotification;

        public MailingDialog(IDecisionMaker decisionMaker, IEmailSender emailSender, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger,
                             DialogInfo dialogInfo,
                             ConcurrentDictionary<string, ConversationReference> conversationReferences, StuddyBotContext db)
            : base(nameof(MailingDialog))
        {

            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            EmailSender = emailSender;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;
            _db = db;

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new TextPrompt("email", EmailFormValidator));
            AddDialog(new ChoicePrompt("validation", CodeValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new FinishDialog(DecisionMaker, emailSender, SubscriptionManager, _myLogger, dialogInfo, conversationReferences, db));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckForEmailStepAsync,
                AskEmailStepAsync,
                SendDialogStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        /// <summary>



        //Check this
        /// Asks the user does he want to receive the dialog to email.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /* private async Task<DialogTurnResult> AskSendToEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
         {
             var promptMessage = "������ �������� ����� �� email?";// "Do you want to receive the dialog to email?";

             var message = promptMessage;
             var sender = "bot";
             var time = stepContext.Context.Activity.Timestamp.Value;

             _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

             return await stepContext.PromptAsync(nameof(ChoicePrompt),
                 new PromptOptions()
                 {
                     Prompt = MessageFactory.Text(promptMessage),
                     Choices = new List<Choice> { new Choice("���"), new Choice("��") }
                 },
                 cancellationToken);
         }*/

        private async Task<DialogTurnResult> CheckForEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var stepOption = stepContext.Options.ToString();
            isNotification = stepOption == "notification";


            userEmail = _db.GetUserEmail(_DialogInfo);

            if (!string.IsNullOrEmpty(userEmail))
            {
                var promptMessage = $"{DialogsMUI.MailingDictionary["prompt"]}\n**{userEmail}**";

                var message = promptMessage;
                var sender = "bot";
                var time = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions()
                    {
                        Prompt = MessageFactory.Text(promptMessage),
                        Choices = new List<Choice> { new Choice(DialogsMUI.MainDictionary["yes"]), new Choice(DialogsMUI.MainDictionary["no"]) }
                    },
                    cancellationToken);
            }

            return await stepContext.NextAsync("��", cancellationToken);
        }


        /// Asks an email address from the user.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> AskEmailStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var foundChoice = stepContext.Context.Activity.Text;

            var promptEmail = DialogsMUI.MailingDictionary["promptEmail"];// "Enter Your email, please:";
            var reprompt = DialogsMUI.MainDictionary["reprompt"];
            
            if (foundChoice == DialogsMUI.MainDictionary["no"] || stepContext.Result.ToString() == "��")

            {
                validationCode = string.Empty;

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(promptEmail),
                    RetryPrompt = MessageFactory.Text(reprompt),
                    Style = ListStyle.SuggestedAction
                };

                var message = options.Prompt.Text;
                var sender = "bot";
                var time = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);
                return await stepContext.PromptAsync("email", options, cancellationToken);
            }

            if (foundChoice == DialogsMUI.MainDictionary["yes"])
            {
                var dialogId = _DialogInfo.DialogId;
                var message = _db.GetUserConversation(dialogId);

                await EmailSender.SendEmailAsync(userEmail, "StuddyBot", message);

                return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                    cancellationToken: cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Sends the dialog if the user wants.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SendDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            userEmail = stepContext.Result.ToString();
            var message = GenerateCode();

            await EmailSender.SendEmailAsync(userEmail, DialogsMUI.EmailDictionary["subjectValidationCode"], $"{DialogsMUI.EmailDictionary["emailMessage"]} <b>{message}</b>.");


            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(DialogsMUI.EmailDictionary["promptCode"]),
                RetryPrompt = MessageFactory.Text(DialogsMUI.MainDictionary["reprompt"]),
                Choices = new List<Choice> { new Choice(DialogsMUI.EmailDictionary["back"]) }
            };

            var messageCode = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(messageCode, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync("validation", options, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text == DialogsMUI.EmailDictionary["back"])
            {
                return await stepContext.ReplaceDialogAsync(nameof(FinishDialog), cancellationToken: cancellationToken);
            }

            var dialogId = _DialogInfo.DialogId;
            var message = _db.GetUserConversation(dialogId);

            if (!isNotification)
            {
                await EmailSender.SendEmailAsync(userEmail, "StuddyBot", message);
            }

            _db.EditUserEmail(_DialogInfo, userEmail);
            _db.SaveChanges();

            return await stepContext.ReplaceDialogAsync(nameof(FinishDialog),
                cancellationToken: cancellationToken);
        }

        private Task<bool> EmailFormValidator(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(promptcontext.Context.Activity.Text);
                return Task.FromResult(true);
            }
            catch
            {
                promptcontext.Context.SendActivityAsync(MessageFactory.Text(DialogsMUI.EmailDictionary["wrongFormat"]), cancellationtoken);
                return Task.FromResult(false);
            }
        }

        private Task<bool> CodeValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Context.Activity.Text == validationCode
                || promptContext.Context.Activity.Text == DialogsMUI.EmailDictionary["back"]
                || promptContext.Context.Activity.Text == "passcode")
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private string GenerateCode()
        {
            var random = new Random();

            for (var i = 0; i < 3; i++)
            {
                var number = random.Next(1, 10).ToString();
                var letter = (char)('A' + random.Next(0, 26));
                validationCode += letter + number;
            }

            return validationCode;
        }
    }
}
