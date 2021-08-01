using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicBot.Common.Cards
{
    public class MainOptionsCard
    {
        public static async Task ToShow(DialogContext stepContext, CancellationToken cancellationtoken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel(), cancellationtoken);
        }

        private static Activity CreateCarousel()
        {
            var cardCitasMedicas = new HeroCard
            {
                Title = "Citas Medicas",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu01.JPG") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title="Crear Cita Médica",Value="Crear Cita Médica",Type=ActionTypes.ImBack},
                    new CardAction(){Title="Ver mi cita",Value="Ver mi cita",Type=ActionTypes.ImBack},
                }
            };

            var cardInformacionContacto = new HeroCard
            {
                Title = "Informacion de Contacto",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/meno02.JPG") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title="Centro de Contacto",Value="Centro de Contacto",Type=ActionTypes.ImBack},
                    new CardAction(){Title="Sitio web",Value="ENLACE",Type=ActionTypes.OpenUrl},
                }
            };

            var cardSiguenosRedes = new HeroCard
            {
                Title = "Siguenos en las Redes",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu03.JPG") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title="Facebook",Value="ENLACE",Type=ActionTypes.OpenUrl},
                    new CardAction(){Title="Instagram",Value="ENLACE",Type=ActionTypes.OpenUrl},
                    new CardAction(){Title="Twitter",Value="ENLACE",Type=ActionTypes.OpenUrl},
                }
            };

            var cardCalificacion = new HeroCard
            {
                Title = "Calificacion",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://clinicbotstorage.blob.core.windows.net/images/menu04.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title="Calificar Bot",Value="Calificar Bot",Type=ActionTypes.ImBack},

                }
            };

            var optionsAttachments = new List<Attachment>()
            {
                cardCitasMedicas.ToAttachment(),
                cardInformacionContacto.ToAttachment(),
                cardSiguenosRedes.ToAttachment(),
                cardCalificacion.ToAttachment(),

            };
            var reply = MessageFactory.Attachment(optionsAttachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}

