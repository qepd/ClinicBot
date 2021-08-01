﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClinicBot.Common.Models.User;
using ClinicBot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicBot
{
    public class ClinicBot<T> : ActivityHandler where T : Dialog
    {
        //Declaramos variables para los estados del usuario y la conversacion
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly IDataBaseService _databaseService;

        //Creamos un constructor con los mismos valores
        public ClinicBot(UserState userState, ConversationState conversationState, T dialog, IDataBaseService databaseService)
        {
            //hacemos inyeccion de dependencias
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _databaseService = databaseService;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hola PAPUS"), cancellationToken);
                }
            }
        }
        //Aqui implementamos un metodo para capturar las actividades tanto del bot como del usuario
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            //Para grabar los cambios del usuario
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        //Aqui creamos un metodo para capturar todas las actividades del usuario

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await SaveUser(turnContext);
            await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
                );

        }
        private async Task SaveUser(ITurnContext<IMessageActivity> turnContext)
        {
            var userModel = new UserModel();
            userModel.id = turnContext.Activity.From.Id;
            userModel.userNameChannel = turnContext.Activity.From.Name;
            userModel.channel = turnContext.Activity.ChannelId;
            userModel.registerDate = DateTime.Now.Date;

            var user = await _databaseService.User.FirstOrDefaultAsync(x => x.id == turnContext.Activity.From.Id);

            if (user == null)
            {
                await _databaseService.User.AddAsync(userModel);
                await _databaseService.SaveAsync();
            }

        }

    }
}

