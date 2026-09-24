using NetArchTest.Rules;
using Shouldly;

namespace Framework.Architecture.Tests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_does_not_reference_other_layers()
    {
        var result = Types.InAssembly(typeof(global::Framework.Domain.Authorization.Permissions).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Framework.Application", "Framework.Infrastructure", "Framework.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_does_not_reference_infrastructure_or_web()
    {
        var result = Types.InAssembly(typeof(global::Framework.Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Framework.Infrastructure", "Framework.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_does_not_reference_web()
    {
        var result = Types.InAssembly(typeof(global::Framework.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Framework.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(Environment.NewLine, result.FailingTypeNames ?? []));
    }
}
