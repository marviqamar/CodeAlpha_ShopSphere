import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order';
import { Order } from '../../../core/models/order.model';
import { DatePipe } from '@angular/common';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-order-details',
  imports: [RouterLink, DatePipe, ConfirmDialog],
  templateUrl: './order-details.html',
  styleUrl: './order-details.css'
})
export class OrderDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  imageBaseUrl = environment.apiUrl.replace('/api', '');

  order = signal<Order | null>(null);
  isLoading = signal(true);
  justPlaced = signal(false);

  isCancelling = signal(false);
  cancelError = signal('');

  ngOnInit(): void {
    const orderId = Number(this.route.snapshot.paramMap.get('id'));
    this.justPlaced.set(this.route.snapshot.queryParamMap.get('success') === 'true');

    this.orderService.getById(orderId).subscribe({
      next: (data) => {
        this.order.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }

  cancelOrder(): void {
    const order = this.order();
    if (!order) return;

    this.confirmDialog.open({
      title: 'Cancel Order',
      message: `Cancel order #${order.id}? This cannot be undone, and the items will be restocked.`,
      confirmLabel: 'Cancel Order',
      onConfirm: () => {
        this.isCancelling.set(true);
        this.cancelError.set('');

        this.orderService.cancelOrder(order.id).subscribe({
          next: (updated) => {
            this.order.set(updated);
            this.isCancelling.set(false);
          },
          error: (err) => {
            this.cancelError.set(err.error?.message || 'Could not cancel order.');
            this.isCancelling.set(false);
          }
        });
      }
    });
  }
}