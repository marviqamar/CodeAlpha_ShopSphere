import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet, NavigationEnd } from '@angular/router';
import { Auth } from '../../core/services/auth';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css'
})
export class AdminLayout implements OnInit {
  private router = inject(Router);
  authService = inject(Auth);

  sidebarOpen = signal(false);
  pageTitle = signal('Dashboard');

  private titleMap: Record<string, string> = {
    '/admin': 'Dashboard',
    '/admin/products': 'Products',
    '/admin/categories': 'Categories',
    '/admin/orders': 'Orders',
    '/admin/coupons': 'Coupons'
  };

  ngOnInit(): void {
    this.updateTitle(this.router.url);
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.updateTitle(event.urlAfterRedirects);
        this.closeSidebar();
      }
    });
  }

  private updateTitle(url: string): void {
    this.pageTitle.set(this.titleMap[url] ?? 'Admin');
  }

  toggleSidebar(): void {
    this.sidebarOpen.update(open => !open);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}