using StickFightTheGameTrainer.Common;
using StructureMap;

namespace StickFightTheGameTrainer.DependencyInjection.Registries
{
    public class TrainerRegistry : Registry
    {
        public TrainerRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            ForSingletonOf<ILogger>().Use<Logger>();
            For<ErrorReportingForm>().Use<ErrorReportingForm>();
        }
    }
}
