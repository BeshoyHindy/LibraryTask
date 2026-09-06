using Grpc.Core;
using Lending.Contracts;
using Lending.Contracts.Grpc;
using Lending.Features.BorrowBook;
using Lending.Features.GetLoan;
using Lending.Features.ReturnBook;
using Library.Shared;
using Mediator;
using LoanMessage = Lending.Contracts.Grpc.Loan;

namespace Lending.Grpc;

public sealed class LendingGrpcService(IMediator mediator) : LendingService.LendingServiceBase
{
    public override async Task<LoanMessage> BorrowBook(BorrowBookRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var borrowedOn = ValueOrThrow(ProtoDates.Required(request.BorrowedOn, "borrowedOn"));
        var command = new BorrowBookCommand(request.BookId, request.BorrowerId, borrowedOn);

        return ToMessage(ValueOrThrow(await mediator.Send(command, context.CancellationToken)));
    }

    public override async Task<LoanMessage> ReturnBook(ReturnBookRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var returnedOn = ValueOrThrow(ProtoDates.Required(request.ReturnedOn, "returnedOn"));
        var command = new ReturnBookCommand(request.LoanId, returnedOn);

        return ToMessage(ValueOrThrow(await mediator.Send(command, context.CancellationToken)));
    }

    public override async Task<LoanMessage> GetLoan(GetLoanRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var query = new GetLoanQuery(request.LoanId);

        return ToMessage(ValueOrThrow(await mediator.Send(query, context.CancellationToken)));
    }

    private static T ValueOrThrow<T>(Result<T> result)
    {
        if (result.Error is { } error)
        {
            throw error.ToRpcException();
        }

        return result.Value;
    }

    private static LoanMessage ToMessage(LoanDto loan)
    {
        var message = new LoanMessage
        {
            LoanId = loan.Id,
            BookId = loan.BookId,
            BorrowerId = loan.BorrowerId,
            BorrowedOn = ProtoDates.ToDate(loan.BorrowedOn),
        };

        if (loan.ReturnedOn is DateOnly returnedOn)
        {
            message.ReturnedOn = ProtoDates.ToDate(returnedOn);
        }

        return message;
    }
}
