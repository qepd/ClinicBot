using ClinicBot.Common.Models.Qualification;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Dialogs.Qualification
{
    public class QualificationDialog : ComponentDialog
    {
        private IDataBaseService _databaseService;
        public QualificationDialog(IDataBaseService databaseService)
        {
            _databaseService = databaseService;
            var waterfallSteps = new WaterfallStep[]
            {
                ToShowButtom,
                ValidateOption
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }



        private async Task<DialogTurnResult> ToShowButtom(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions
                {
                    Prompt = CreateButtonsQualifation()
                },
                cancellationToken
                );
        }

        private Activity CreateButtonsQualifation()
        {
            var reply = MessageFactory.Text("Calificame por favor");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title="1⭐",Value="1⭐",Type=ActionTypes.ImBack},
                    new CardAction(){Title="2⭐",Value="2⭐",Type=ActionTypes.ImBack},
                    new CardAction(){Title="3⭐",Value="3⭐",Type=ActionTypes.ImBack},
                    new CardAction(){Title="4⭐",Value="4⭐",Type=ActionTypes.ImBack},
                    new CardAction(){Title="5⭐",Value="5⭐",Type=ActionTypes.ImBack},
                }
            };
            return reply as Activity;
        }

        private async Task<DialogTurnResult> ValidateOption(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Context.Activity.Text;
            await stepContext.Context.SendActivityAsync($"Gracias por tu {options}", cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync("¿En que más te quedo ayudar?", cancellationToken: cancellationToken);
            await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            //Graba Calificación
            await SaveQualification(stepContext, options);
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task SaveQualification(WaterfallStepContext stepContext, string options)
        {
            var qualificationModel = new QualificationModel();
            qualificationModel.id = Guid.NewGuid().ToString();
            qualificationModel.idUser = stepContext.Context.Activity.From.Id;
            qualificationModel.Qualification = options;
            qualificationModel.registerDate = DateTime.Now.Date;

            await _databaseService.Qualification.AddAsync(qualificationModel);
            await _databaseService.SaveAsync();
        }
    }
}//Nuevo codigo

