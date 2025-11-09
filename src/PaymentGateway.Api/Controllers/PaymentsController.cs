using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentService _payService;

    public PaymentsController(IPaymentService payService)
    {
        _payService = payService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse>> GetPaymentAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid payment ID.");
        }

        var previousPaymentRequest = await _payService.GetPaymentAsync(id);
        if (previousPaymentRequest.IsSuccess)
        {
            return Ok(previousPaymentRequest.Data);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        if (!PaymentRequestCommand.TryCreate(request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.Currency,
                request.Amount,
                request.Cvv,
                out PaymentRequestCommand payRequest,
                out var errors))
        {
            return BadRequest(errors);
        }

        var processPayResult = await _payService.ProcessPaymentAsync(payRequest);
        if (processPayResult.IsFailure)
        {
            return StatusCode((int)processPayResult.Error!.Code, processPayResult.Error.Message);
        }

        return Ok(new PostPaymentResponse
        {
            Id = processPayResult.Data!.Id,
            Amount = processPayResult.Data.Amount,
            Currency = processPayResult.Data.Currency,
            CardNumberLastFour = processPayResult.Data.CardNumberLastFour,
            ExpiryMonth = processPayResult.Data.ExpiryMonth,
            ExpiryYear = processPayResult.Data.ExpiryYear,
            Status = processPayResult.Data.Status
        });
    }
}