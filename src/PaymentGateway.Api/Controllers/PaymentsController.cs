using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest paymentRequest)
    {
        var res = await _payService.ProcessPaymentAsync(paymentRequest);
        return new OkObjectResult(res);
    }
}