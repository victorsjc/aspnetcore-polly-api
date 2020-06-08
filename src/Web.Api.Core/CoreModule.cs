using Autofac;

namespace Web.Api.Core
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<RegisterUserUseCase>().As<IRegisterUserUseCase>().InstancePerLifetimeScope();
            //builder.RegisterType<LoginUseCase>().As<ILoginUseCase>().InstancePerLifetimeScope();
            //builder.RegisterType<ExchangeRefreshTokenUseCase>().As<IExchangeRefreshTokenUseCase>().InstancePerLifetimeScope();
            //builder.RegisterType<CreateUserGroupUseCase>().As<ICreateUserGroupUseCase>().InstancePerLifetimeScope();
        }
    }
}
