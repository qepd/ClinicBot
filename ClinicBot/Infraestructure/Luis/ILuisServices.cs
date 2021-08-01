using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Infraestructure.Luis
{
    public interface ILuisServices
    {
        LuisRecognizer _luisRecognizer { get; set; }
    }
}
