import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../../../core/services/dashboard';
import { DashboardStats } from '../../../../core/models/dashboard.model';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, DatePipe],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  private dashboardService = inject(DashboardService);

  stats = signal<DashboardStats | null>(null);
  isLoading = signal(true);

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // Helper to loop over the ordersByStatus object in the template
  statusEntries(): { status: string; count: number }[] {
    const stats = this.stats();
    if (!stats) return [];

    return Object.entries(stats.ordersByStatus).map(([status, count]) => ({ status, count }));
  }

  maxDailyRevenue(): number {
    const stats = this.stats();
    if (!stats || stats.salesLast7Days.length === 0) return 1;
    return Math.max(...stats.salesLast7Days.map(d => d.revenue), 1);
  }
}