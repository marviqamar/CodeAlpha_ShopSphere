export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  sku: string;
  isActive: boolean;
  createdAt: string;
  categoryId: number;
  categoryName: string;
  imageUrls: string[];
  averageRating: number;
  reviewCount: number;
}

export interface ProductQueryParams {
  search?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  sku: string;
  categoryId: number;
  imageUrls: string[];
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  sku: string;
  categoryId: number;
  isActive: boolean;
}