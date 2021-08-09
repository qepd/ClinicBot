using ClinicBot.Common.Models.BotStateModel;
using ClinicBot.Common.Models.MedicalAppointment;
using ClinicBot.Common.Models.User;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Dialogs.CreateAppointment
{
    public class CreateAppointmentDialog : ComponentDialog
    {
        private readonly IDataBaseService _databaseService;
        public static UserModel newUserModel = new UserModel();
        public static MedicalAppointmentModel medicalAppointmentModel = new MedicalAppointmentModel();

        private readonly IStatePropertyAccessor<BotStateModel> _userState;

        public CreateAppointmentDialog(IDataBaseService databaseService, UserState userState)
        {
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            _databaseService = databaseService;
            var waterfallStep = new WaterfallStep[]
        {
                SetPhone,
                SetFullName,
                SetEmail,
                SetDate,
                SetTime,
                Confirmation,
                FinalProcess

        };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> SetPhone(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {



            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tú numero de telefono") },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetFullName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userPhone = stepContext.Context.Activity.Text;
            newUserModel.phone = userPhone;

            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu nombre completo :") },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetEmail(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var fullNameUser = stepContext.Context.Activity.Text;
            newUserModel.fullName = fullNameUser;
            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text("Ahora ingresa tu correo : ") },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetDate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userEmail = stepContext.Context.Activity.Text;
            newUserModel.email = userEmail;

            string text = $"Ahora necesito la fecha de la cita médica con el siguiente formato :" + $"{Environment.NewLine}dd/mm/yyyy";

            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = MessageFactory.Text(text) },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> SetTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var medicalDate = stepContext.Context.Activity.Text;
            medicalAppointmentModel.date = DateTime.ParseExact(medicalDate, "dd/MM/yyyy", null);

            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = CreateButtonsTime() },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> Confirmation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var medicalTime = stepContext.Context.Activity.Text;
            medicalAppointmentModel.time = int.Parse(medicalTime);

            return await stepContext.PromptAsync(
            nameof(TextPrompt),
            new PromptOptions { Prompt = CreateButtonConfirmation() },
            cancellationToken
            );
        }

        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userConfirmation = stepContext.Context.Activity.Text;

            if (userConfirmation.ToLower().Equals("si"))
            {
                //Grabamos en la base de datos.
                string userId = stepContext.Context.Activity.From.Id;
                var userModel = await _databaseService.User.FirstOrDefaultAsync(x => x.id == userId);

                //Actualizamos al usuario
                userModel.phone = newUserModel.phone;
                userModel.fullName = newUserModel.fullName;
                userModel.email = newUserModel.email;

                _databaseService.User.Update(userModel);
                await _databaseService.SaveAsync();

                //Guardar la Cita Medica
                medicalAppointmentModel.id = Guid.NewGuid().ToString();
                medicalAppointmentModel.idUser = userId;
                await _databaseService.MedicalAppointment.AddAsync(medicalAppointmentModel);
                await _databaseService.SaveAsync();
                await stepContext.Context.SendActivityAsync("tu cita se realizo con exito", cancellationToken: cancellationToken);

                //Mostrar la informacion de toda la cita medica
                string summaryMedical = $"Para: {userModel.fullName}" +
                    $"{Environment.NewLine}📞 Telefono: {userModel.phone}" +
                    $"{Environment.NewLine}✉ Email: {userModel.email}" +
                    $"{Environment.NewLine}📅 Fecha: {medicalAppointmentModel.date}" +
                    $"{Environment.NewLine}⏰ Hora: {medicalAppointmentModel.time}";

                await stepContext.Context.SendActivityAsync(summaryMedical, cancellationToken: cancellationToken);
                await Task.Delay(1000);
                await stepContext.Context.SendActivityAsync("En que mas puedo ayudarte?", cancellationToken: cancellationToken);
                medicalAppointmentModel = new MedicalAppointmentModel();

            }
            else
            {
                await stepContext.Context.SendActivityAsync("No hay problema será la proxima", cancellationToken: cancellationToken);
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }

        private Activity CreateButtonConfirmation()
        {
            var reply = MessageFactory.Text("Confirmas la creación de esta cita Medica?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>
                {
                    new CardAction(){Title="Si",Value="Si",Type=ActionTypes.ImBack },
                    new CardAction(){Title="No",Value="No",Type=ActionTypes.ImBack },
                }
            };
            return reply as Activity;
        }

        private Activity CreateButtonsTime()
        {
            var reply = MessageFactory.Text("Ahora selecciona la hora");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>
                {
                    new CardAction(){Title="9",Value="9",Type=ActionTypes.ImBack},
                    new CardAction(){Title="10",Value="10",Type=ActionTypes.ImBack},
                    new CardAction(){Title="11",Value="11",Type=ActionTypes.ImBack},
                    new CardAction(){Title="12",Value="12",Type=ActionTypes.ImBack},
                    new CardAction(){Title="13",Value="13",Type=ActionTypes.ImBack},
                    new CardAction(){Title="14",Value="14",Type=ActionTypes.ImBack},
                    new CardAction(){Title="15",Value="15",Type=ActionTypes.ImBack},
                    new CardAction(){Title="16",Value="16",Type=ActionTypes.ImBack},
                    new CardAction(){Title="17",Value="17",Type=ActionTypes.ImBack},
                    new CardAction(){Title="18",Value="18",Type=ActionTypes.ImBack},
                }
            };
            return reply as Activity;
        }
    }
}

