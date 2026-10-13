export interface TopProduct {
  productId: number;
  productName: string;
  quantitySold: number;
  revenue: number;
}

export interface DailySales {
  date: string;
  orderCount: number;
  revenue: number;
}

export interface LowStockProduct {
  productId: number;
  productName: string;
  stockQuantity: number;
}

export interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
  ordersByStatus: { [key: string]: number };
  topProducts: TopProduct[];
  salesLast7Days: DailySales[];
  lowStockProducts: LowStockProduct[];
}