using StructureMap;
using System;
using System.Windows.Forms;
using StickFightTheGameTrainer.DependencyInjection.Registries;

namespace StickFightTheGameTrainer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = Container.For<TrainerRegistry>();
            var mainForm = container.GetInstance<MainForm>();

#if DEBUG
            var debugForm = container.GetInstance<DebugForm>();
            debugForm.Show();
#endif

            Application.Run(mainForm);
        }
    }
}
