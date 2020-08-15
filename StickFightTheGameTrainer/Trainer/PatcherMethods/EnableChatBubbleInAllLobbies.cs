using dnlib.DotNet;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Enable chat bubble in all lobbies (used for weapon selection - displays name of currently held weapon)
    /// </summary>
    internal static class EnableChatBubbleInAllLobbies
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            if (InjectionHelpers.ClearMethodBody(targetModule, "ChatManager", "Awake") == false)
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
