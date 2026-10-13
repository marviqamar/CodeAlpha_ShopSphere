import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WishlistItem, AddToWishlistRequest } from '../models/wishlist.model';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrl}/wishlist`;

  items = signal<WishlistItem[]>([]);

  constructor(private http: HttpClient) {}

  getMyWishlist(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(this.apiUrl)
      .pipe(tap(items => this.items.set(items)));
  }

  add(request: AddToWishlistRequest): Observable<WishlistItem> {
    return this.http.post<WishlistItem>(this.apiUrl, request)
      .pipe(tap(item => this.items.update(current => [...current, item])));
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(tap(() => this.items.update(current => current.filter(i => i.id !== id))));
  }

  isInWishlist(productId: number): boolean {
    return this.items().some(i => i.productId === productId);
  }
}