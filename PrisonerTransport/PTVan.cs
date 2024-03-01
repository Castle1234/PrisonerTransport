using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrisonerTransport
{
    internal class PTVan : BaseScript
    {
        private static Vehicle ptVanEntity;
        private static int ptVanBlip;
        private static Ped ptVanPed1;
        private static Ped ptVanPed2;
        private static bool eventSpawned;
        private static bool eventOnScene;

        private static Vector3 targetLocation = new Vector3();

        private static readonly Random random = new Random();

        public static List<string> ped1Messages = new List<string>
        {
            "~b~Officer 1: ~w~We have arrived at the scene.", "~b~Officer 1: ~w~What do we have here?"
        };

        public static List<string> ped2Messages = new List<string>
        {
            "~b~Officer 2: ~w~Must have done something bad to wind up here", "~b~Officer 2: ~w~Time to get you down the nick"
        };

        public static List<string> progressMessages = new List<string>
        {
            "~b~Officer 2: ~w~Lets get you in the back of the van", "~b~Officer 2: ~w~All in a days work lets get going"
        };

        public static List<string> endMessages = new List<string>
        {
            "~b~Officer 1: ~w~We are on our way to the station", "~b~Officer 1: ~w~Lets get you booked in"
        };
        public static async void Loop()
        {
            if (eventSpawned)
            {
                Ped player = Game.Player.Character;

                if (!eventOnScene && API.GetDistanceBetweenCoords(ptVanEntity.Position.X, ptVanEntity.Position.Y, ptVanEntity.Position.Z, targetLocation.X, targetLocation.Y, targetLocation.Z, true) < 10F)
                {
                    eventOnScene = true;
                    Screen.ShowNotification("The prisoner transport van has arrived at the scene.");
                    ptVanPed1.Task.ClearAllImmediately();
                    API.SetVehicleForwardSpeed(ptVanEntity.Handle, 0F);
                    API.TaskLeaveVehicle(ptVanPed1.Handle, ptVanEntity.Handle, 0);
                    API.TaskLeaveVehicle(ptVanPed2.Handle, ptVanEntity.Handle, 0);

                    //Communications will go here
                    CommonFunction.DisplayMessage(ped1Messages[random.Next(ped1Messages.Count)], 2500);
                    await BaseScript.Delay(2500);
                    CommonFunction.DisplayMessage(ped2Messages[random.Next(ped2Messages.Count)], 2500);

                    API.TaskGoToEntity(ptVanPed1.Handle, player.Handle, -1, 5F, 3F, 1073741824, 0);
                    API.TaskGoToEntity(ptVanPed2.Handle, player.Handle, -1, 5F, 3F, 1073741824, 0);

                    while (API.GetDistanceBetweenCoords(ptVanPed2.Position.X, ptVanPed2.Position.Y, ptVanPed2.Position.Z, player.Position.X, player.Position.Y, player.Position.Z, true) > 5F)
                    {
                        Debug.WriteLine("Officer pathfinding to player");
                        await BaseScript.Delay(500);
                    }
                    API.TaskStartScenarioAtPosition(ptVanPed2.Handle, "WORLD_HUMAN_COP_IDLES", ptVanPed2.Position.X, ptVanPed2.Position.Y, ptVanPed2.Position.Z, ptVanPed2.Heading, -1, false, false);
                }
            }
        }
        public async static void Summon()
        {
            Ped player = Game.Player.Character;
            Screen.ShowNotification("Summoning the prisoner transport van...");

            //PT van model spawn

            Vector3 spawnLocation = new Vector3();
            float spawnHeading = 0F;
            int unused = 0;

            API.GetNthClosestVehicleNodeWithHeading(player.Position.X, player.Position.Y, player.Position.Z, 80, ref spawnLocation, ref spawnHeading, ref unused, 9, 3.0F, 2.5F);
            await CommonFunction.LoadModel((uint)VehicleHash.PoliceT);
            ptVanEntity = await World.CreateVehicle(VehicleHash.PoliceT, spawnLocation, spawnHeading);
            ptVanEntity.Mods.LicensePlate = "PTVAN";
            ptVanEntity.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;

            //PT van blip spawn
            ptVanBlip = API.AddBlipForEntity(ptVanEntity.Handle);
            API.SetBlipColour(ptVanBlip, 38);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString("Prisoner Transport Van");
            API.EndTextCommandSetBlipName(ptVanBlip);

            //Driver
            await CommonFunction.LoadModel((uint)PedHash.Cop01SMY);
            ptVanPed1 = await World.CreatePed(PedHash.Cop01SMY, spawnLocation);
            ptVanPed1.SetIntoVehicle(ptVanEntity, VehicleSeat.Driver);
            ptVanPed1.CanBeTargetted = false;

            //Passenger
            await CommonFunction.LoadModel((uint)PedHash.Cop01SFY);
            ptVanPed2 = await World.CreatePed(PedHash.Cop01SFY, spawnLocation);
            ptVanPed2.SetIntoVehicle(ptVanEntity, VehicleSeat.Passenger);
            ptVanPed2.CanBeTargetted = false;

            //Comfigs
            float targetHeading = 0F;
            API.GetClosestVehicleNodeWithHeading(player.Position.X, player.Position.Y, player.Position.Z, ref targetLocation, ref targetHeading, 1, 3.0F, 0);
            ptVanPed1.Task.DriveTo(ptVanEntity, targetLocation, 10F, 20F, 262972);
            eventSpawned = true;
        }
    }
}
