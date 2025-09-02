using System.Security.Claims;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("AddReview")]
        public IActionResult AddReview([FromQuery] int pid, [FromBody] ReviewDTO review)
        {
            if (review is null) return BadRequest("Review data is required.");

            var uid = GetUserId();
            _reviewService.AddReview(uid, pid, review);
            return Ok("Review added successfully.");
        }

        [AllowAnonymous]
        [HttpGet("GetAllReviews")]
        [ProducesResponseType(typeof(PagedResult<ReviewDTO>), 200)]
        public IActionResult GetAllReviews(
            [FromQuery] int productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("PageNumber and PageSize must be greater than 0.");

            var result = _reviewService.GetAllReviews(pageNumber, pageSize, productId);
            if (result.Total == 0) return NotFound("No reviews for this product.");
            return Ok(result);
        }

        [HttpDelete("DeleteReview/{reviewId:int}")]
        public IActionResult DeleteReview(int reviewId)
        {
            var review = _reviewService.GetReviewById(reviewId);
            if (review is null) return NotFound($"Review {reviewId} not found.");

            var uid = GetUserId();
            if (review.UID != uid) return BadRequest("You are not authorized to delete this review.");

            _reviewService.DeleteReview(reviewId);
            return Ok($"Review {reviewId} deleted successfully.");
        }

        [HttpPut("UpdateReview/{reviewId:int}")]
        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDTO reviewDTO)
        {
            if (reviewDTO is null) return BadRequest("Review data is required.");

            var review = _reviewService.GetReviewById(reviewId);
            if (review is null) return NotFound($"Review {reviewId} not found.");

            var uid = GetUserId();
            if (review.UID != uid) return BadRequest("You are not authorized to update this review.");

            _reviewService.UpdateReview(reviewId, reviewDTO);
            return Ok($"Review {reviewId} updated successfully.");
        }

        private int GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     User.FindFirst("sub")?.Value;

            if (!int.TryParse(id, out var uid))
                throw new UnauthorizedAccessException("User id not found in token.");
            return uid;
        }
    }
}
