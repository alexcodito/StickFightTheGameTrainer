using dnlib.DotNet;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Add a private field indicating that the module has been modified.
    /// </summary>
    internal static class AddTrainerPatchIndicator
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            var stickFightConstantsTypeDef = targetModule.Find("StickFightConstants", false);

            if (stickFightConstantsTypeDef == null)
            {
                return await Task.FromResult(1);
            }

            stickFightConstantsTypeDef.Fields.Add(new FieldDefUser("LoxaTrainerPatch", new FieldSig(targetModule.CorLibTypes.Boolean), FieldAttributes.Private));

            return await Task.FromResult(0);
        }
    }
}
