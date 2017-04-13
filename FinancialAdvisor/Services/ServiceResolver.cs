using Autofac;

namespace FinancialAdvisor.Services
{
    public static class ServiceResolver
    {
        public static IContainer Container;

        public static T Get<T>()
        {
            using (var scope = Container.BeginLifetimeScope())
                return scope.Resolve<T>();
        }
       
        public static T GetWithParameters<T>(NamedParameter namedParameter)
        {
            using (var scope = Container.BeginLifetimeScope())
                return scope.Resolve<T>(namedParameter);
        }
    }
}