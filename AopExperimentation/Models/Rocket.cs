using System;
using System.Threading.Tasks;

namespace AopExperimentation.Models
{
    public class Rocket : IRocket
    {
        public string Model { get; set; } = string.Empty;

        public ISettings Settings { get; } = SpyLogger<ISettings>.Create(new Settings());

        private event EventHandler? _onCrash;

        public event EventHandler OnLanded
        {
            add => _onCrash += value;
            remove => _onCrash -= value;
        }

        public int CalculateSum(int numberA, int numberB) => numberA + numberB;

        public void DoSomething(string str){ }

        public async Task<string> TakeOff()
        {
            await Task.Delay(TimeSpan.FromSeconds(Settings.CountdownDelay));
            return "Rocket taking off!";
        }

        public void ThrowException()
        {
            throw new NotImplementedException();
        }

        public void DoNotProfile() { }
    }
}