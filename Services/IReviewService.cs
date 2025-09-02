using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
    public interface IReviewService
    {
      
        PagedResult<ReviewDTO> GetAllReviews(int pageNumber, int pageSize, int pid);
        Review? GetReviewById(int rid);
        IEnumerable<ReviewDTO> GetReviewByProductId(int pid);
        void AddReview(int uid, int pid, ReviewDTO reviewDTO);
        void UpdateReview(int rid, ReviewDTO reviewDTO);
        void DeleteReview(int rid);
    }
}