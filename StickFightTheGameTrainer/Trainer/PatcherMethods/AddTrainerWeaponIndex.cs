using dnlib.DotNet;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>         
    /// Add a public int field to keep track of the current weapon selection index
    /// </summary>
    internal static class AddTrainerWeaponIndex
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            if(InjectionHelpers.AddField(targetModule, "Fighting", "TrainerWeaponIndex", targetModule.CorLibTypes.Int32, FieldAttributes.Public) == null)
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
