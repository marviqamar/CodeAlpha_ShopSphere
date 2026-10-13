import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { ProductList } from './features/products/product-list/product-list';
import { ProductDetails } from './features/products/product-details/product-details';
import { CartPage } from './features/cart/cart-page/cart-page';
import { authGuard } from './core/guards/auth-guard';
import { CheckoutPage } from './features/checkout/checkout-page/checkout-page';
import { OrderList } from './features/orders/order-list/order-list';
import { OrderDetails } from './features/orders/order-details/order-details';
import { WishlistPage } from './features/wishlist/wishlist-page/wishlist-page';
import { AdminDashboard } from './features/admin/dashboard/admin-dashboard/admin-dashboard';
import { adminGuard } from './core/guards/admin-guard';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { AdminProductList } from './features/admin/products/admin-product-list/admin-product-list';
import { AdminCategoryList } from './features/admin/categories/admin-category-list/admin-category-list';
import { AdminOrderList } from './features/admin/orders/admin-order-list/admin-order-list';
import { AdminCouponList } from './features/admin/coupons/admin-coupon-list/admin-coupon-list';

export const routes: Routes = [
  { path: '', component: ProductList },
  { path: 'products/:id', component: ProductDetails },
  { path: 'cart', component: CartPage, canActivate: [authGuard] },
  { path: 'checkout', component: CheckoutPage, canActivate: [authGuard] },
  { path: 'orders', component: OrderList, canActivate: [authGuard] },
  { path: 'orders/:id', component: OrderDetails, canActivate: [authGuard] },
  { path: 'wishlist', component: WishlistPage, canActivate: [authGuard] },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [adminGuard],
    children: [
      { path: '', component: AdminDashboard },
      { path: 'products', component: AdminProductList },
      { path: 'categories', component: AdminCategoryList },
      { path: 'orders', component: AdminOrderList },
      { path: 'coupons', component: AdminCouponList }
    ]
  },
  { path: 'login', component: Login },
  { path: 'register', component: Register }
];