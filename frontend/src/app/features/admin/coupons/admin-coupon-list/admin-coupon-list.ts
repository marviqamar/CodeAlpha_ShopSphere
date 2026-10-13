import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CouponService } from '../../../../core/services/coupon';
import { Coupon } from '../../../../core/models/coupon.model';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-admin-coupon-list',
  imports: [FormsModule, DatePipe],
  templateUrl: './admin-coupon-list.html',
  styleUrl: './admin-coupon-list.css'
})
export class AdminCouponList implements OnInit {
  private couponService = inject(CouponService);

  coupons = signal<Coupon[]>([]);
  isLoading = signal(true);

  showForm = signal(false);
  formError = signal('');

  formData = {
    code: '',
    discountType: 'Percentage' as 'Percentage' | 'FixedAmount',
    discountValue: 0,
    minOrderAmount: 0,
    maxUses: null as number | null,
    expiryDate: ''
  };

  ngOnInit(): void {
    this.loadCoupons();
  }

  loadCoupons(): void {
    this.isLoading.set(true);
    this.couponService.getAll().subscribe({
      next: (data) => {
        this.coupons.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openCreateForm(): void {
    this.formError.set('');
    this.formData = {
      code: '', discountType: 'Percentage', discountValue: 0,
      minOrderAmount: 0, maxUses: null, expiryDate: ''
    };
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
  }

  saveCoupon(): void {
    this.formError.set('');

    this.couponService.create({
      code: this.formData.code,
      discountType: this.formData.discountType,
      discountValue: this.formData.discountValue,
      minOrderAmount: this.formData.minOrderAmount,
      maxUses: this.formData.maxUses,
      expiryDate: this.formData.expiryDate ? new Date(this.formData.expiryDate).toISOString() : null
    }).subscribe({
      next: () => {
        this.showForm.set(false);
        this.loadCoupons();
      },
      error: (err) => this.formError.set(err.error?.message || 'Could not create coupon.')
    });
  }

  deleteCoupon(id: number): void {
    if (!confirm('Delete this coupon?')) return;

    this.couponService.delete(id).subscribe({
      next: () => this.loadCoupons(),
      error: (err) => alert(err.error?.message || 'Could not delete coupon.')
    });
  }

  isExpired(coupon: Coupon): boolean {
    return coupon.expiryDate !== null && new Date(coupon.expiryDate) < new Date();
  }
}