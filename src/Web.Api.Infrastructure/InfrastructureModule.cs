using Autofac;
using Module = Autofac.Module;

namespace Web.Api.Infrastructure
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<GroupRepository>().As<IGroupRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<JwtFactory>().As<IJwtFactory>().SingleInstance().FindConstructorsWith(new InternalConstructorFinder());
            //builder.RegisterType<JwtTokenHandler>().As<IJwtTokenHandler>().SingleInstance().FindConstructorsWith(new InternalConstructorFinder());
            //builder.RegisterType<TokenFactory>().As<ITokenFactory>().SingleInstance();
            //builder.RegisterType<JwtTokenValidator>().As<IJwtTokenValidator>().SingleInstance().FindConstructorsWith(new InternalConstructorFinder());
            //builder.RegisterType<Logger>().As<ILogger>().SingleInstance();
        }
    }
}
