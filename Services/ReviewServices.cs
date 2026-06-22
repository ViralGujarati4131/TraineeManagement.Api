using System.Collections;
using System.Data;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.RedisCaching.ServiceInterface;
using TraineeManagementApi.Reviews.DTOs;
using TraineeManagementApi.Reviews.Models;
using TraineeManagementApi.Reviews.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.Reviews.Service;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    private readonly ILogger<ReviewService> _logger;

    private readonly ICacheService _cacheService;

     private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public ReviewService(AppDbContext context, ILogger<ReviewService> logger, ICacheService cacheService)
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

        await _cacheService.RemoveAsync("Reviews:All");

        return MapToResponseDto(review);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetReviewsAsync()
    {
        _logger.LogDebug("Fetching all reviews from the database");

        string cacheKey = "Reviews:All";
        IEnumerable<ReviewResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<ReviewResponseDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning reviews from cache");
            return cached;
        }

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

        await _cacheService.SetAsync(cacheKey, reviews, CacheTtl);

        return reviews;
    }

    public async Task<ReviewResponseDto> GetReviewByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving review with ID: {ReviewId}", id);
        
        string cacheKey = $"Review:{id}";
        ReviewResponseDto? cached = await _cacheService.GetAsync<ReviewResponseDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning review from cache");
            return cached;
        }

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
        await _cacheService.SetAsync(cacheKey, dto, CacheTtl);
        
        return dto;
    }
}