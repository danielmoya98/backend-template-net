using BackendTemplate.Api.Controllers;
using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NetArchTest.Rules;
using Xunit;

namespace BackendTemplate.UnitTests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "BackendTemplate.Domain";
    private const string ApplicationNamespace = "BackendTemplate.Application";
    private const string InfrastructureNamespace = "BackendTemplate.Infrastructure";
    private const string ApiNamespace = "BackendTemplate.Api";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOn_OtherProjects()
    {
        var assembly = typeof(BaseEntity).Assembly;

        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace
        };

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_Infrastructure_Or_Api()
    {
        var assembly = typeof(IApplicationDbContext).Assembly;

        var forbiddenProjects = new[]
        {
            InfrastructureNamespace,
            ApiNamespace
        };

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOn_Api()
    {
        var assembly = typeof(BackendTemplate.Infrastructure.Persistence.ApplicationDbContext).Assembly;

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Have_NameEndingWith_Handler()
    {
        var assembly = typeof(IApplicationDbContext).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Validators_Should_Have_NameEndingWith_Validator()
    {
        var assembly = typeof(IApplicationDbContext).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_Should_Inherit_From_ApiControllerBase()
    {
        var assembly = typeof(ApiControllerBase).Assembly;

        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("ApiControllerBase")
            .Should()
            .Inherit(typeof(ApiControllerBase))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
