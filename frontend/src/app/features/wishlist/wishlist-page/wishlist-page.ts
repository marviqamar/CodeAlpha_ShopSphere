import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../../core/services/wishlist';
import { CartService } from '../../../core/services/cart';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-wishlist-page',
  imports: [RouterLink],
  templateUrl: './wishlist-page.html',
  styleUrl: './wishlist-page.css'
})
export class WishlistPage implements OnInit {
  private wishlistService = inject(WishlistService);
  private cartService = inject(CartService);

  items = this.wishlistService.items;
  isLoading = signal(true);
  message = signal('');

  ngOnInit(): void {
    this.wishlistService.getMyWishlist().subscribe({
      next: () => this.isLoading.set(false),
      error: () => this.isLoading.set(false)
    });
  }

  remove(id: number): void {
    this.wishlistService.remove(id).subscribe();
  }

  addToCart(productId: number): void {
    this.message.set('');
    this.cartService.addItem({ productId, quantity: 1 }).subscribe({
      next: () => this.message.set('Added to cart!'),
      error: (err) => this.message.set(err.error?.message || 'Could not add to cart.')
    });
  }

  imageBaseUrl = environment.apiUrl.replace('/api', '');
}