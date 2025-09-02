using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using static E_CommerceSystem.Models.PagingDtos;

namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepo _reviewRepo;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepo reviewRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IOrderService orderService,
            IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
            _mapper = mapper;
        }

        // Paged list of reviews (DTOs) for a product
        public PagedResult<ReviewDTO> GetAllReviews(int pageNumber, int pageSize, int pid)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // repo likely returns IEnumerable<Review>; materialize and page safely
            var allForProduct = _reviewRepo.GetReviewByProductId(pid) ?? Enumerable.Empty<Review>();
            var total = allForProduct.Count();

            var pageItems = allForProduct
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtoItems = _mapper.Map<List<ReviewDTO>>(pageItems);
            return new PagedResult<ReviewDTO>(dtoItems, total, pageNumber, pageSize);
        }

        public Review? GetReviewById(int rid)
        {
            return _reviewRepo.GetReviewById(rid);
        }

        public IEnumerable<ReviewDTO> GetReviewByProductId(int pid)
        {
            var list = _reviewRepo.GetReviewByProductId(pid) ?? Enumerable.Empty<Review>();
            return _mapper.Map<List<ReviewDTO>>(list);
        }

        public void AddReview(int uid, int pid, ReviewDTO reviewDTO)
        {
            // Must have purchased the product
            var userOrders = _orderService.GetOrderByUserId(uid) ?? Enumerable.Empty<Order>();
            var purchased = false;

            foreach (var order in userOrders)
            {
                var items = _orderProductsService.GetOrdersByOrderId(order.OID) ?? new List<OrderProducts>();
                if (items.Any(op => op.PID == pid))
                {
                    purchased = true;
                    break;
                }
            }

            if (!purchased)
                throw new InvalidOperationException("You can only review products you have purchased.");

            // Only one review per user per product
            var existingReview = _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);
            if (existingReview != null)
                throw new InvalidOperationException("You have already reviewed this product.");

            // Map DTO -> entity and set contextual fields
            var review = _mapper.Map<Review>(reviewDTO);
            review.PID = pid;
            review.UID = uid;
            review.ReviewDate = DateTime.UtcNow;

            _reviewRepo.AddReview(review);

            // Update product rating for this product
            RecalculateProductRating(pid);
        }

        public void UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = _reviewRepo.GetReviewById(rid)
                         ?? throw new KeyNotFoundException($"Review with ID {rid} not found.");

            // Overlay DTO -> entity (only editable fields)
            _mapper.Map(reviewDTO, review);
            review.ReviewDate = DateTime.UtcNow;

            _reviewRepo.UpdateReview(review);

            // Recompute rating for this product (pid, not rating!)
            RecalculateProductRating(review.PID);
        }

        public void DeleteReview(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid)
                         ?? throw new KeyNotFoundException($"Review with ID {rid} not found.");

            _reviewRepo.DeleteReview(rid);

            // Update rating for that product
            RecalculateProductRating(review.PID);
        }

        private void RecalculateProductRating(int pid)
        {
            // Get only reviews for this product
            var reviews = _reviewRepo.GetReviewByProductId(pid) ?? Enumerable.Empty<Review>();

            var product = _productService.GetProductById(pid)
                          ?? throw new KeyNotFoundException($"Product with ID {pid} not found.");

            var average = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            product.OverallRating = Convert.ToDecimal(average);

            _productService.UpdateProduct(product);
        }
    }
}
