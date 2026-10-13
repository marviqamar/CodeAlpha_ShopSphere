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

  orders = signal<Order[]>([]);
  isLoading = signal(true);
  totalPages = signal(0);
  pageNumber = 1;

  statusOptions = ['Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'];
  updatingOrderId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.orderService.getAll(this.pageNumber, 10).subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber = page;
    this.loadOrders();
  }

  updateStatus(order: Order, newStatus: string): void {
    this.updatingOrderId.set(order.id);

    this.orderService.updateStatus(order.id, newStatus).subscribe({
      next: (updated) => {
        this.orders.update(current =>
          current.map(o => o.id === updated.id ? updated : o)
        );
        this.updatingOrderId.set(null);
      },
      error: () => {
        this.updatingOrderId.set(null);
        this.loadOrders(); // re-sync on failure
      }
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }
}