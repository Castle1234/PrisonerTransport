using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace PrisonerTransport
{
    public class Main : BaseScript
    {
        private static int gTimer;
        public Main()
        {
            API.RegisterCommand("ptvan", new Action(SummonPTVan), false);

            Tick += OnTick;
        }

        private async Task Ontick()
        {
            if(API.GetGameTimer() -gTimer >= 1000)
            {
                gTimer = API.GetGameTimer();

                API.EnableDispatchService(1, false);

                PTVan.Loop();
            }
        }

        private void SummonPTVan()
        {
            PTVan.Summon();
        }
    }
}
