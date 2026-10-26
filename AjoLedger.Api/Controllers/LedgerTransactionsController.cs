
using AjoLedger.Api.DTOs;
using AjoLedger.Api.Models;
using AjoLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AjoLedger.Api.Controllers;

[ApiController]
[Route("api/savings-groups/{groupId:guid}/ledger-transactions")]
public class LedgerTransactionsController : ControllerBase
{
    private readonly ILedgerTransactionService _service;

    public LedgerTransactionsController(
        ILedgerTransactionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid groupId,
        RecordLedgerTransactionRequest request)
    {
        var transaction = new LedgerTransaction
        {
            SavingsGroupId = groupId,
            Reference = request.Reference,
            TransactionType = request.TransactionType,
            Description = request.Description,
            IdempotencyKey = request.IdempotencyKey,
            Entries = request.Entries.Select(entry => new LedgerEntry
            {
                LedgerAccountId = entry.LedgerAccountId,
                Debit = entry.Debit,
                Credit = entry.Credit
            }).ToList()
        };

        try
        {
            var result = await _service.Create(transaction);

            return result.Status switch
            {
                LedgerTransactionCreateStatus.Created =>
                    CreatedAtAction(
                        nameof(GetById),
                        new
                        {
                            groupId,
                            transactionId = result.Transaction.Id
                        },
                        result.Transaction),

                LedgerTransactionCreateStatus.AlreadyExists =>
                    Ok(result.Transaction),

                LedgerTransactionCreateStatus
                    .IdempotencyKeyReusedWithDifferentPayload =>
                    Conflict(new
                    {
                        message = "This idempotency key was already used with different transaction details."
                    }),

                LedgerTransactionCreateStatus.ReferenceAlreadyExists =>
                    Conflict(new
                    {
                        message = "This transaction reference already exists in this savings group."
                    }),

                _ => StatusCode(500, new
                {
                    message = "An unexpected transaction result occurred."
                })
            };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetByGroup(Guid groupId)
    {
        var transactions = await _service.GetByGroup(groupId);
        return Ok(transactions);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<IActionResult> GetById(
        Guid groupId,
        Guid transactionId)
    {
        var transaction = await _service.GetById(
            groupId,
            transactionId);

        if (transaction is null)
        {
            return NotFound(new
            {
                message = "Ledger transaction not found."
            });
        }

        return Ok(transaction);
    }
}
