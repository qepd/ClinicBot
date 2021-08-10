using ClinicBot.Common.Cards;
using ClinicBot.Data;
using ClinicBot.Dialogs.CreateAppointment;
using ClinicBot.Dialogs.Qualification;
using ClinicBot.Dialogs.SendGridEmail;
using ClinicBot.Infraestructure.Luis;
using ClinicBot.Infraestructure.QnAMakerAI;
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
        private readonly ILuisServices _luisService;
        private readonly IDataBaseService _databaseService;
        private readonly ISendGridEmailService _sendGridEmailService;
        private readonly IQnAMakerAIService _qnAMakerAIService;

        public RootDialogs(ILuisServices luisService, IDataBaseService databaseService, UserState userState,
            ISendGridEmailService sendGridEmailService , IQnAMakerAIService qnAMakerAIService)
        {
            _qnAMakerAIService = qnAMakerAIService;
            _sendGridEmailService = sendGridEmailService;
            _databaseService = databaseService;
            _luisService = luisService;
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            AddDialog(new QualificationDialog(_databaseService));
            AddDialog(new CreateAppointmentDialog(_databaseService, userState,_sendGridEmailService,_luisService));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
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
                case "CrearCita":
                    return await IntentCrearCita(stepContext, luisResult, cancellationToken);
                case "VerCita":
                    await IntentVerCita(stepContext, luisResult, cancellationToken);
                    break;
                case "None":
                    await IntentNone(stepContext, luisResult, cancellationToken);
                    break;
                default:
                    break;
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        
        private async Task<DialogTurnResult> IntentCrearCita(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(CreateAppointmentDialog), cancellationToken: cancellationToken);
        }
        #region  IntentLuis  

        private async Task IntentVerCita(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Un momento por favor....", cancellationToken: cancellationToken);
            await Task.Delay(1000);
            string idUser = stepContext.Context.Activity.From.Id;
            var medicalData = _databaseService.MedicalAppointment.Where(x => x.idUser == idUser).ToList();
            if (medicalData.Count > 0)
            {
                var pending = medicalData.Where(p => p.date >= DateTime.Now.Date).ToList();
                if (pending.Count > 0)
                {
                    await stepContext.Context.SendActivityAsync("Estas son tus citas pendientes", cancellationToken: cancellationToken);
                    foreach (var item in pending)
                    {
                        await Task.Delay(1000);
                        if (item.date == DateTime.Now.Date && item.time < DateTime.Now.Hour)
                            continue;

                        string summaryMedical = $"Fecha: {item.date.ToShortDateString()}" +
                           $"{Environment.NewLine}   Hora: {item.time}";


                        await stepContext.Context.SendActivityAsync(summaryMedical, cancellationToken: cancellationToken);
                    }
                }
                else
                    await stepContext.Context.SendActivityAsync("Lo siento pero no tienes citas pendientes", cancellationToken: cancellationToken);
            }
            else

                await stepContext.Context.SendActivityAsync("Lo siento pero no tienes citas pendientes", cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntentCalificar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QualificationDialog), cancellationToken: cancellationToken);
        }
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
            var resultQnA = await _qnAMakerAIService._qnaMakerResult.GetAnswersAsync(stepContext.Context);
            var score = resultQnA.FirstOrDefault()?.Score;
            string response = resultQnA.FirstOrDefault()?.Answer;
            if (score >= 0.5)
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
        
                }
                else
                  {
                await stepContext.Context.SendActivityAsync("No entiendo lo que me dices.", cancellationToken: cancellationToken);
                await Task.Delay(1000);
                await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                }
         }


        #endregion



        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
