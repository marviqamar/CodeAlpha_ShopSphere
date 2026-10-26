import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { CouponService } from '../../../../core/services/coupon';
import { Coupon } from '../../../../core/models/coupon.model';

@Component({
  selector: 'app-admin-coupon-list',
  imports: [FormsModule, DatePipe, ConfirmDialog],
  templateUrl: './admin-coupon-list.html',
  styleUrl: './admin-coupon-list.css'
})
export class AdminCouponList implements OnInit {
  private couponService = inject(CouponService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  allCoupons: Coupon[] = [];
  coupons = signal<Coupon[]>([]);
  isLoading = signal(true);
  searchTerm = '';

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
        this.allCoupons = data;
        this.applyFilter();
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  applyFilter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.coupons.set(
      term ? this.allCoupons.filter(c => c.code.toLowerCase().includes(term)) : this.allCoupons
    );
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

  deleteCoupon(coupon: Coupon): void {
    this.confirmDialog.open({
      title: 'Delete Coupon',
      message: `Delete coupon "${coupon.code}"? This cannot be undone.`,
      confirmLabel: 'Delete',
      onConfirm: () => {
        this.couponService.delete(coupon.id).subscribe({
          next: () => this.loadCoupons(),
          error: (err) => alert(err.error?.message || 'Could not delete coupon.')
        });
      }
    });
  }

  isExpired(coupon: Coupon): boolean {
    return coupon.expiryDate !== null && new Date(coupon.expiryDate) < new Date();
  }
}