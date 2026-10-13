export interface WishlistItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl?: string;
  price: number;
  inStock: boolean;
  addedAt: string;
}

export interface AddToWishlistRequest {
  productId: number;
}