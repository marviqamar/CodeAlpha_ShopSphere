import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product';
import { ReviewService } from '../../../core/services/review';
import { CartService } from '../../../core/services/cart';
import { Auth } from '../../../core/services/auth';
import { Product } from '../../../core/models/product.model';
import { Review } from '../../../core/models/review.model';
import { DatePipe } from '@angular/common';
import { WishlistService } from '../../../core/services/wishlist';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-details',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css'
})
export class ProductDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private reviewService = inject(ReviewService);
  private cartService = inject(CartService);
  authService = inject(Auth);

  product = signal<Product | null>(null);
  reviews = signal<Review[]>([]);
  selectedImageIndex = signal(0);
  isLoading = signal(true);

  quantity = 1;
  addToCartMessage = signal('');

  newReviewRating = 5;
  newReviewComment = '';
  reviewError = signal('');
  reviewSuccess = signal('');

  ngOnInit(): void {
    const productId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadProduct(productId);
    this.loadReviews(productId);
    this.wishlistService.getMyWishlist().subscribe();
  }

  loadProduct(id: number): void {
    this.isLoading.set(true);
    this.productService.getById(id).subscribe({
      next: (data) => {
        this.product.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadReviews(productId: number): void {
    this.reviewService.getByProductId(productId).subscribe({
      next: (data) => this.reviews.set(data)
    });
  }

  selectImage(index: number): void {
    this.selectedImageIndex.set(index);
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;

    this.addToCartMessage.set('');

    this.cartService.addItem({ productId: product.id, quantity: this.quantity }).subscribe({
      next: () => {
        this.addToCartMessage.set(`Added ${this.quantity} item(s) to cart!`);
      },
      error: (err) => {
        this.addToCartMessage.set(err.error?.message || 'Could not add to cart.');
      }
    });
  }

  submitReview(): void {
    const product = this.product();
    if (!product) return;

    this.reviewError.set('');
    this.reviewSuccess.set('');

    this.reviewService.create({
      productId: product.id,
      rating: this.newReviewRating,
      comment: this.newReviewComment || undefined
    }).subscribe({
      next: () => {
        this.reviewSuccess.set('Review submitted!');
        this.newReviewComment = '';
        this.newReviewRating = 5;
        this.loadReviews(product.id); // refresh the list
      },
      error: (err) => {
        this.reviewError.set(err.error?.message || 'Could not submit review.');
      }
    });
  }

    // add to the injected services:
  private wishlistService = inject(WishlistService);

  // add a new method:
  toggleWishlist(): void {
    const product = this.product();
    if (!product) return;

    if (this.wishlistService.isInWishlist(product.id)) {
      const item = this.wishlistService.items().find(i => i.productId === product.id);
      if (item) this.wishlistService.remove(item.id).subscribe();
    } else {
      this.wishlistService.add({ productId: product.id }).subscribe();
    }
  }

  //Hover Ratings
  hoverRating = signal(0);

  setRating(value: number): void {
    this.newReviewRating = value;
  }

  setHoverRating(value: number): void {
    this.hoverRating.set(value);
  }

  clearHoverRating(): void {
    this.hoverRating.set(0);
  }

  imageBaseUrl = environment.apiUrl.replace('/api', '');
}