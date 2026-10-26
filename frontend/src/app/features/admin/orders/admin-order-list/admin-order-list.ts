import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../../../core/services/order';
import { Order } from '../../../../core/models/order.model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-admin-order-list',
  imports: [FormsModule, DatePipe],
  templateUrl: './admin-order-list.html',
  styleUrl: './admin-order-list.css'
})
export class AdminOrderList implements OnInit {
  private orderService = inject(OrderService);

  allOrders: Order[] = [];
  orders = signal<Order[]>([]);
  isLoading = signal(true);
  totalPages = signal(0);
  pageNumber = 1;

  searchTerm = '';
  statusFilter = '';

  statusOptions = ['Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];
  updatingOrderId = signal<number | null>(null);
  justUpdatedId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.orderService.getAll(this.pageNumber, 10).subscribe({
      next: (result) => {
        this.allOrders = result.items;
        this.applyFilters();
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  applyFilters(): void {
    let filtered = this.allOrders;

    if (this.statusFilter) {
      filtered = filtered.filter(o => o.status === this.statusFilter);
    }

    const term = this.searchTerm.trim().toLowerCase();
    if (term) {
      filtered = filtered.filter(o =>
        o.id.toString().includes(term) ||
        o.customerName.toLowerCase().includes(term) ||
        o.customerEmail.toLowerCase().includes(term)
      );
    }

    this.orders.set(filtered);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber = page;
    this.loadOrders();
  }

  updateStatus(order: Order, newStatus: string): void {
    this.updatingOrderId.set(order.id);
    this.justUpdatedId.set(null);

    this.orderService.updateStatus(order.id, newStatus).subscribe({
      next: (updated) => {
        this.allOrders = this.allOrders.map(o => o.id === updated.id ? updated : o);
        this.applyFilters();
        this.updatingOrderId.set(null);
        this.justUpdatedId.set(updated.id);
        setTimeout(() => this.justUpdatedId.set(null), 2000);
      },
      error: () => {
        this.updatingOrderId.set(null);
        this.loadOrders();
      }
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }
}