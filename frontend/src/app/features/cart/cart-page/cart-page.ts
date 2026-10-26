import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../core/services/cart';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cart-page',
  imports: [RouterLink, FormsModule, ConfirmDialog],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css'
})
export class CartPage implements OnInit {
  private cartService = inject(CartService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  imageBaseUrl = environment.apiUrl.replace('/api', '');

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
        this.loadCart();
      }
    });
  }

  removeItem(cartItemId: number, productName: string): void {
    this.confirmDialog.open({
      title: 'Remove Item',
      message: `Remove "${productName}" from your cart?`,
      confirmLabel: 'Remove',
      onConfirm: () => {
        this.cartService.removeItem(cartItemId).subscribe();
      }
    });
  }

  clearCart(): void {
    this.confirmDialog.open({
      title: 'Clear Cart',
      message: 'Remove all items from your cart? This cannot be undone.',
      confirmLabel: 'Clear Cart',
      onConfirm: () => {
        this.cartService.clearCart().subscribe();
      }
    });
  }
}