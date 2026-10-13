export interface Coupon {
  id: number;
  code: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  minOrderAmount: number;
  maxUses: number | null;
  usedCount: number;
  expiryDate: string | null;
  isActive: boolean;
}

export interface CreateCouponRequest {
  code: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  minOrderAmount: number;
  maxUses: number | null;
  expiryDate: string | null;
}

export interface CouponValidationResult {
  couponId: number;
  code: string;
  discountAmount: number;
  newTotal: number;
}