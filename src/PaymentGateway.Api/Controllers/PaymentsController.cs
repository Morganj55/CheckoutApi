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
    private readonly IPaymentRepository _paymentsRepository;
    private readonly IPaymentService _payService;

    public PaymentsController(IPaymentRepository paymentsRepository, IPaymentService payService)
    {
        _paymentsRepository = paymentsRepository;
        _payService = payService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        return new OkObjectResult(payment);
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
            return StatusCode((int)processPayResult.Error.Code, processPayResult.Error.Message);
        }

        var res = processPayResult.Data;
        return new OkObjectResult(new PostPaymentResponse
        {
            Id = res.Id,
            Amount = res.Amount,
            Currency = res.Currency,
            CardNumberLastFour = res.CardNumberLastFour,
            ExpiryMonth = res.ExpiryMonth,
            ExpiryYear = res.ExpiryYear,
            Status = res.Status
        });
    }
}