import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../core/services/cart';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cart-page',
  imports: [RouterLink, FormsModule],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css'
})
export class CartPage implements OnInit {
  private cartService = inject(CartService);

  // Reuse the shared signal from CartService directly — no need for a separate local copy
  cart = this.cartService.cart;
  isLoading = signal(true);
  errorMessage = signal('');

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.isLoading.set(true);
    this.cartService.getCart().subscribe({
      next: () => this.isLoading.set(false),
      error: () => this.isLoading.set(false)
    });
  }

  updateQuantity(cartItemId: number, newQuantity: number): void {
    if (newQuantity < 1) return;

    this.errorMessage.set('');
    this.cartService.updateItem(cartItemId, { quantity: newQuantity }).subscribe({
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Could not update quantity.');
        this.loadCart(); // reload to reset the UI to the real server state
      }
    });
  }

  removeItem(cartItemId: number): void {
    this.cartService.removeItem(cartItemId).subscribe();
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe();
  }

  imageBaseUrl = environment.apiUrl.replace('/api', '');
}