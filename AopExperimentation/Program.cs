using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AopExperimentation.Models;

namespace AopExperimentation
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var config = new SpyLoggerConfiguration
            {
                MethodNamesToExclude = new List<string> {"DoNotProfile"},
                UseJsonSerializer = true,
                CustomMessage = "[Rocket number 2]"
            };
            var decoratedRocket = SpyLogger<IRocket>.Create(new Rocket(), config);

            //Events
            decoratedRocket.OnLanded += SendSuccessMessage;
            decoratedRocket.OnLanded -= SendSuccessMessage;

            // Method
            decoratedRocket.CalculateSum(1, 2);
            decoratedRocket.DoSomething("something");

            // Properties
            var model = decoratedRocket.Model;
            decoratedRocket.Model = "League";

            //Misc
            decoratedRocket.Settings.CountdownDelay = 2;
            await decoratedRocket.TakeOff();

            //decoratedCalculator.ThrowException();
            decoratedRocket.DoNotProfile();

            Console.ReadKey();
        }

        private static void SendSuccessMessage(object? sender, EventArgs e) { }
    }
}
