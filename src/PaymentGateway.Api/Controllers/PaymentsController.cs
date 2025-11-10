using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

/// <summary>
/// Controller responsible for handling payment-related API requests, including processing new payments and retrieving past payment details.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    #region Fields

    /// <summary>
    /// The payment service used to handle the business logic for payment operations.
    /// </summary>
    private readonly IPaymentService _payService;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="payService">The payment service injected by the dependency injection container.</param>
    public PaymentsController(IPaymentService payService)
    {
        _payService = payService;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Retrieves the details of a previously processed payment by its ID.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the payment to retrieve.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the <see cref="PostPaymentResponse"/> if found (HTTP 200 Ok),
    /// or a 404 Not Found if no payment exists for the given ID, or 400 Bad Request for an invalid ID.
    /// </returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse>> GetPaymentAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid payment ID.");
        }

        var previousPaymentRequest = await _payService.GetPaymentAsync(id);
        if (previousPaymentRequest.IsFailure)
        {
            return NotFound();
        }

        return Ok(new GetPaymentResponse
        (
            previousPaymentRequest.Data!.Id,
            previousPaymentRequest.Data.Status,
            previousPaymentRequest.Data.CardNumberLastFour,
            previousPaymentRequest.Data.ExpiryMonth,
            previousPaymentRequest.Data.ExpiryYear,
            previousPaymentRequest.Data.Currency,
            previousPaymentRequest.Data.Amount
        ));
    }

    /// <summary>
    /// Processes a new payment request by validating the input and delegating the payment logic to the service layer.
    /// </summary>
    /// <param name="request">The payment request details provided in the request body.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing the processed <see cref="PostPaymentResponse"/> (HTTP 200 Ok),
    /// a 400 Bad Request for validation errors, or a 500-level status for processing errors.
    /// </returns>
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
            // Use the status code from the error object if available, otherwise default to a generic error
            return StatusCode((int)processPayResult.Error!.Code!, processPayResult.Error.Message);
        }

        // Map the result data to the response model 
        return Ok(new PostPaymentResponse(
            processPayResult.Data!.Id,
            processPayResult.Data.Status,
            processPayResult.Data.CardNumberLastFour,
            processPayResult.Data.ExpiryMonth,
            processPayResult.Data.ExpiryYear,
            processPayResult.Data.Currency,
            processPayResult.Data.Amount));
    }

    #endregion
}