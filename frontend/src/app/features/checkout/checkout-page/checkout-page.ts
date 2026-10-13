import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart';
import { OrderService } from '../../../core/services/order';
import { CouponService } from '../../../core/services/coupon';
import { Cart } from '../../../core/models/cart.model';
import { CouponValidationResult } from '../../../core/models/coupon.model';

@Component({
  selector: 'app-checkout-page',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './checkout-page.html',
  styleUrl: './checkout-page.css'
})
export class CheckoutPage implements OnInit {
  private fb = inject(FormBuilder);
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private couponService = inject(CouponService);
  private router = inject(Router);

  cart = signal<Cart | null>(null);
  isLoading = signal(true);

  shippingForm: FormGroup = this.fb.group({
    shippingFullName: ['', Validators.required],
    shippingPhone: ['', Validators.required],
    shippingStreet: ['', Validators.required],
    shippingCity: ['', Validators.required],
    shippingState: ['', Validators.required],
    shippingPostalCode: ['', Validators.required],
    shippingCountry: ['', Validators.required]
  });

  couponCode = '';
  appliedCoupon = signal<CouponValidationResult | null>(null);
  couponError = signal('');
  isValidatingCoupon = signal(false);

  isPlacingOrder = signal(false);
  orderError = signal('');

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      next: (cart) => {
        this.cart.set(cart);
        this.isLoading.set(false);

        if (cart.items.length === 0) {
          this.router.navigate(['/cart']);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  get finalTotal(): number {
    const cart = this.cart();
    if (!cart) return 0;

    const coupon = this.appliedCoupon();
    return coupon ? coupon.newTotal : cart.totalAmount;
  }

  applyCoupon(): void {
    const cart = this.cart();
    if (!cart || !this.couponCode.trim()) return;

    this.isValidatingCoupon.set(true);
    this.couponError.set('');

    this.couponService.validate(this.couponCode.trim(), cart.totalAmount).subscribe({
      next: (result) => {
        this.appliedCoupon.set(result);
        this.isValidatingCoupon.set(false);
      },
      error: (err) => {
        this.appliedCoupon.set(null);
        this.couponError.set(err.error?.message || 'Invalid coupon.');
        this.isValidatingCoupon.set(false);
      }
    });
  }

  removeCoupon(): void {
    this.appliedCoupon.set(null);
    this.couponCode = '';
    this.couponError.set('');
  }

  placeOrder(): void {
    if (this.shippingForm.invalid) {
      this.shippingForm.markAllAsTouched();
      return;
    }

    this.isPlacingOrder.set(true);
    this.orderError.set('');

    const request = {
      ...this.shippingForm.value,
      couponCode: this.appliedCoupon()?.code
    };

    this.orderService.placeOrder(request).subscribe({
      next: (order) => {
        this.isPlacingOrder.set(false);
        this.router.navigate(['/orders', order.id], { queryParams: { success: 'true' } });
      },
      error: (err) => {
        this.isPlacingOrder.set(false);
        this.orderError.set(err.error?.message || 'Could not place order. Please try again.');
      }
    });
  }
}