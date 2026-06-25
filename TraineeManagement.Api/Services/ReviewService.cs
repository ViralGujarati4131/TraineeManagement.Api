using System.Data;
using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.CacheServiceInterface;
using TraineeManagement.Api.Data.ReviewDTO;
using TraineeManagement.Api.Data.ReviewModel;
using TraineeManagement.Api.ReviewServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.ReviewService;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    private readonly ILogger<ReviewService> _logger;

    private readonly ICacheService _cacheService;

    public ReviewService(AppDbContext context, ILogger<ReviewService> logger,ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    private ReviewResponseDto MapToResponseDto(Review review)
    {
        return new ReviewResponseDto(
            review.Id,
            review.SubmissionId,
            review.MentorId,
            review.Feedback,
            review.Score,
            review.ReviewStatus,
            review.ReviewedDate
        );
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(ReviewCreateDto reviewCreateDto)
    {
        Review review = new Review
        {
            SubmissionId = reviewCreateDto.SubmissionId,
            MentorId = reviewCreateDto.MentorId,
            Feedback = reviewCreateDto.Feedback,
            Score = reviewCreateDto.score, 
            ReviewStatus = reviewCreateDto.ReviewStatus,
            ReviewedDate = reviewCreateDto.ReviewedDate
        };
        
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new review with ID {ReviewId}", review.Id);

        await _cacheService.RemoveManyAsync(CacheKey.AllReview());

        return MapToResponseDto(review);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsAsync()
    {
        _logger.LogDebug("Fetching all reviews from the database");

        IEnumerable<ReviewResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<ReviewResponseDto>>(CacheKey.AllReview());
        if (cached is not null)
            return cached;

        IEnumerable<ReviewResponseDto> reviews = await _context.Reviews
            .AsNoTracking()
            .Select(r => new ReviewResponseDto(
                r.Id,
                r.SubmissionId,
                r.MentorId,
                r.Feedback,
                r.Score,
                r.ReviewStatus,
                r.ReviewedDate
            )).ToListAsync();
        
        await _cacheService.SetAsync(CacheKey.AllReview(), reviews, TimeSpan.FromMinutes(10));

        return reviews;
    }

    public async Task<ReviewResponseDto> GetReviewByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving review with ID: {ReviewId}", id);

        ReviewResponseDto? dto = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReviewResponseDto(
                r.Id,
                r.SubmissionId,
                r.MentorId,
                r.Feedback,
                r.Score,
                r.ReviewStatus,
                r.ReviewedDate
            )).FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} was not found during target DTO projection.", id);
            throw new NotFoundException("Review");
        }
       
        return dto;
    }
}