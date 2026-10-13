import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Cart, AddToCartRequest, UpdateCartItemRequest } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;

  // Shared cart state — any component can read this to show live totals (e.g. navbar cart count)
  cart = signal<Cart | null>(null);

  constructor(private http: HttpClient) {}

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.apiUrl)
      .pipe(tap(cart => this.cart.set(cart)));
  }

  addItem(request: AddToCartRequest): Observable<Cart> {
    return this.http.post<Cart>(`${this.apiUrl}/items`, request)
      .pipe(tap(cart => this.cart.set(cart)));
  }

  updateItem(cartItemId: number, request: UpdateCartItemRequest): Observable<Cart> {
    return this.http.put<Cart>(`${this.apiUrl}/items/${cartItemId}`, request)
      .pipe(tap(cart => this.cart.set(cart)));
  }

  removeItem(cartItemId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.apiUrl}/items/${cartItemId}`)
      .pipe(tap(cart => this.cart.set(cart)));
  }

  clearCart(): Observable<void> {
    return this.http.delete<void>(this.apiUrl)
      .pipe(tap(() => {
        this.getCart().subscribe();
      }));
  }
}