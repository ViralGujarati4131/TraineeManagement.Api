using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Reviews.DTOs;
using TraineeManagementApi.Reviews.Models;
using TraineeManagementApi.Reviews.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Reviews.Service;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    private readonly ILogger<ReviewService> _logger;

    public ReviewService(AppDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private ReviewResponseDto MapToResponseDto(Review review)
    {
        return new ReviewResponseDto
        (
            review.Id,
            review.SubmissionId,
            review.MentorId,
            review.Feedback,
            review.Score,
            review.ReviewStatus,
            review.ReviewedDate
        );
    }

    private async Task<Review> FetchReviewByIdInternalAsync(int id)
    {
        Review? review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} was not found", id);

            throw new NotFoundException("Review");
        }
        return review;
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

        return MapToResponseDto(review);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsAsync()
    {
        _logger.LogDebug("Fetching all reviews from the database");

        IEnumerable<ReviewResponseDto> reviews = await _context.Reviews
                                                             .Select(r => new 
                                                                ReviewResponseDto
                                                                (
                                                                    r.Id,
                                                                    r.SubmissionId,
                                                                    r.MentorId,
                                                                    r.Feedback,
                                                                    r.Score,
                                                                    r.ReviewStatus,
                                                                    r.ReviewedDate
                                                                )
                                                            ).ToListAsync();
        return reviews;
    }

    public async Task<ReviewResponseDto> GetReviewByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving review with ID: {ReviewId}", id);

        ReviewResponseDto? review = await _context.Reviews  
                                                .Where(r => r.Id == id)
                                                .Select(r => new 
                                                    ReviewResponseDto
                                                    (
                                                        r.Id,
                                                        r.SubmissionId,
                                                        r.MentorId,
                                                        r.Feedback,
                                                        r.Score,
                                                        r.ReviewStatus,
                                                        r.ReviewedDate
                                                    )
                                                ).FirstOrDefaultAsync();
        if (review == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} was not found", id);
            
            throw new NotFoundException("Review");
        }                            
        return review;
    }
}