import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../../core/services/wishlist';
import { CartService } from '../../../core/services/cart';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-wishlist-page',
  imports: [RouterLink, ConfirmDialog],
  templateUrl: './wishlist-page.html',
  styleUrl: './wishlist-page.css'
})
export class WishlistPage implements OnInit {
  private wishlistService = inject(WishlistService);
  private cartService = inject(CartService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  imageBaseUrl = environment.apiUrl.replace('/api', '');

  items = this.wishlistService.items;
  isLoading = signal(true);
  message = signal('');

  ngOnInit(): void {
    this.wishlistService.getMyWishlist().subscribe({
      next: () => this.isLoading.set(false),
      error: () => this.isLoading.set(false)
    });
  }

  remove(id: number, productName: string): void {
    this.confirmDialog.open({
      title: 'Remove from Wishlist',
      message: `Remove "${productName}" from your wishlist?`,
      confirmLabel: 'Remove',
      onConfirm: () => {
        this.wishlistService.remove(id).subscribe();
      }
    });
  }

  addToCart(productId: number): void {
    this.message.set('');
    this.cartService.addItem({ productId, quantity: 1 }).subscribe({
      next: () => this.message.set('Added to cart!'),
      error: (err) => this.message.set(err.error?.message || 'Could not add to cart.')
    });
  }
}