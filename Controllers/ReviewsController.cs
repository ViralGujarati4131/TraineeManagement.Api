using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.Reviews.DTOs;
using TraineeManagementApi.Reviews.ServiceInterface;
using TraineeManagementApi.Utils.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Reviews.Cotroller;

[ApiController]
[Route(AppConstants.Routes.Reviews)]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview([FromBody] ReviewCreateDto createReviewDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking review service to add a new review");

        ReviewResponseDto createdReview = await _reviewService.CreateReviewAsync(createReviewDto);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeCreated,
            AppConstants.ApiResponse.MsgCreated,
            createdReview
        );
    }

    [HttpGet]
    public async Task<ActionResult> GetReviews()
    {
        _logger.LogDebug("Invoking review service to fetch all reviews");

        IEnumerable<ReviewResponseDto> reviews = await _reviewService.GetReviewsAsync();

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            reviews
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetReviewById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking review service to retrieve review for ReviewId: {ReviewId}", id);

        ReviewResponseDto review = await _reviewService.GetReviewByIdAsync(id);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            review
        );
    }
}