using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.xUnitV3;
using FluentValidation;
using Mediator;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Library.ArchitectureTests;

// The rules select by interface, so a module that stops declaring handlers or messages fails them
// rather than passing on an empty selection.
public sealed class HandlerRuleTests
{
    [Fact]
    public void Handlers_ArePublicAndSealed() =>
        Handlers()
            .Should().BePublic()
            .AndShould().BeSealed()
            .Check(Solution.Architecture);

    [Fact]
    public void CommandsAndQueriesWithALimit_HaveAValidator() =>
        Messages()
            .Should().FollowCustomCondition(
                message => !NeedsValidator(message) || HasValidator(message),
                "have a {Name}Validator in the same assembly",
                "has no validator")
            .Check(Solution.Architecture);

    private static GivenClassesConjunction Handlers() =>
        Classes().That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or().ImplementInterface(typeof(IQueryHandler<,>));

    private static GivenClassesConjunction Messages() =>
        Classes().That()
            .ImplementInterface(typeof(ICommand<>))
            .Or().ImplementInterface(typeof(IQuery<>));

    private static bool NeedsValidator(Class message) =>
        message.ImplementsInterface(typeof(ICommand<>).FullName!)
        || message.GetPropertyMembers().Any(property => property.Name == "Limit");

    private static bool HasValidator(Class message) =>
        Solution.Architecture.Classes.Any(candidate =>
            candidate.Assembly.Equals(message.Assembly)
            && candidate.Name == message.Name + "Validator"
            && candidate.ImplementsInterface(typeof(IValidator<>).FullName!));
}
