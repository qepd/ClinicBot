using ClinicBot.Common.Cards;
using ClinicBot.Infraestructure.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Dialogs
{
    public class RootDialogs: ComponentDialog
    {
        private readonly ILuisServices luisservice;
        private ILuisServices _luisService;

        public RootDialogs(ILuisServices luisService)
        {
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            return await ManageIntentions(stepContext, luisResult, cancellationToken);
        }
        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            var topIntent = luisResult.GetTopScoringIntent();
            switch (topIntent.intent)
            {
                case "Saludar":
                    await IntentSaludar(stepContext, luisResult, cancellationToken);
                    break;
                case "Agradecer":
                    await IntentAgradecer(stepContext, luisResult, cancellationToken);
                    break;
                case "Despedir":
                    await IntentDespedir(stepContext, luisResult, cancellationToken);
                    break;
                case "VerOpciones":
                    await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                    break;
                case "VerCentroContacto":
                    await IntentVerCentroContacto(stepContext, luisResult, cancellationToken);
                    break;
                case "VerCalificar":
                    return await IntentCalificar(stepContext, luisResult, cancellationToken);
                case "None":
                    await IntentNone(stepContext, luisResult, cancellationToken);
                    break;
                default:
                    break;
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        #region  IntentLuis  

        private async Task IntentVerCentroContacto(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            string phoneDetail = $"Nuestros números de atención son los siguientes: {Environment.NewLine}"
                + $"📞 +51 961784838{Environment.NewLine} 📞+51 961784839";
            string addressDetail = $"Estamos ubicados en {Environment.NewLine}Calle Ica 123, Ica";

            await stepContext.Context.SendActivityAsync(phoneDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync(addressDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync("En qué más te puedo ayudar", cancellationToken: cancellationToken);
        }
        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync("Aqui tengo mis opciones.", cancellationToken: cancellationToken);
            await MainOptionsCard.ToShow(stepContext, cancellationToken);

        }

        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("hola que gusto verte", cancellationToken: cancellationToken);
        }

        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No te preocupes, me gusta ayudar", cancellationToken: cancellationToken);
        }

        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("espero verte pronto", cancellationToken: cancellationToken);
        }

        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No entiendo lo que me dices", cancellationToken: cancellationToken);
        }

        #endregion

     

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
