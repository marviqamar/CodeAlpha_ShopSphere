import { Component, OnInit, AfterViewInit, inject, signal, ViewChild, ElementRef, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { CategoryService } from '../../../core/services/category';
import { Product, ProductQueryParams } from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-list',
  imports: [FormsModule, RouterLink],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList implements OnInit, AfterViewInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  @ViewChild('categoryTiles') categoryTilesRef?: ElementRef<HTMLElement>;

  imageBaseUrl = environment.apiUrl.replace('/api', '');

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  isLoading = signal(false);
  totalPages = signal(0);
  totalCount = signal(0);
  canScrollLeft = signal(false);
  canScrollRight = signal(false);

  search = '';
  selectedCategoryId: number | null = null;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortBy = 'CreatedAt';
  sortDescending = true;
  pageNumber = 1;
  pageSize = 8;

  ngOnInit(): void {
    this.loadCategories();

    this.route.queryParamMap.subscribe(params => {
      const idParam = params.get('categoryId');
      this.selectedCategoryId = idParam ? Number(idParam) : null;
      this.pageNumber = 1;
      this.loadProducts();
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.checkScrollPosition(), 100);
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.checkScrollPosition();
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories.set(data);
        setTimeout(() => this.checkScrollPosition(), 100);
      }
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);

    const params: ProductQueryParams = {
      search: this.search || undefined,
      categoryId: this.selectedCategoryId ?? undefined,
      minPrice: this.minPrice ?? undefined,
      maxPrice: this.maxPrice ?? undefined,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.productService.getAll(params).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onCategoryDropdownChange(): void {
    this.pageNumber = 1;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { categoryId: this.selectedCategoryId ?? null },
      queryParamsHandling: 'merge'
    });
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  resetFilters(): void {
    this.search = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.sortBy = 'CreatedAt';
    this.sortDescending = true;
    this.pageNumber = 1;

    if (this.selectedCategoryId !== null) {
      this.router.navigate(['/']);
    } else {
      this.loadProducts();
    }
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber = page;
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  pagesArray(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'https://placehold.co/250x250?text=No+Image';
  }

  selectCategory(categoryId: number): void {
    this.router.navigate(['/'], { queryParams: { categoryId } });
  }

  checkScrollPosition(): void {
    const el = this.categoryTilesRef?.nativeElement;
    if (!el) {
      this.canScrollLeft.set(false);
      this.canScrollRight.set(false);
      return;
    }

    const maxScroll = el.scrollWidth - el.clientWidth;
    this.canScrollLeft.set(el.scrollLeft > 4);
    this.canScrollRight.set(maxScroll > 4 && el.scrollLeft < maxScroll - 4);
  }

  scrollCategoryTiles(direction: 'left' | 'right'): void {
    const el = this.categoryTilesRef?.nativeElement;
    if (!el) return;
    el.scrollBy({ left: direction === 'right' ? 400 : -400, behavior: 'smooth' });
  }
}