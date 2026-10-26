import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { ProductService } from '../../../../core/services/product';
import { CategoryService } from '../../../../core/services/category';
import { UploadService } from '../../../../core/services/upload';
import { CreateProductRequest, Product, UpdateProductRequest } from '../../../../core/models/product.model';
import { Category } from '../../../../core/models/category.model';
import { environment } from '../../../../../environments/environment';


@Component({
  selector: 'app-admin-product-list',
  imports: [FormsModule, ConfirmDialog],
  templateUrl: './admin-product-list.html',
  styleUrl: './admin-product-list.css'
})
export class AdminProductList implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private uploadService = inject(UploadService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  isLoading = signal(true);
  totalPages = signal(0);
  pageNumber = 1;
  searchTerm = '';

  showForm = signal(false);
  editingProductId = signal<number | null>(null);
  formError = signal('');

  uploadedImageUrls = signal<string[]>([]);
  isUploading = signal(false);
  imageBaseUrl = environment.apiUrl.replace('/api', '');

  formData = {
    name: '',
    description: '',
    price: 0,
    stockQuantity: 0,
    sku: '',
    categoryId: 0,
    isActive: true
  };

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories.set(data)
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.productService.getAll({
      pageNumber: this.pageNumber,
      pageSize: 10,
      sortBy: 'CreatedAt',
      sortDescending: true,
      search: this.searchTerm || undefined
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  onSearchChange(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber = page;
    this.loadProducts();
  }

  openCreateForm(): void {
    this.editingProductId.set(null);
    this.formError.set('');
    this.uploadedImageUrls.set([]);
    this.formData = {
      name: '', description: '', price: 0, stockQuantity: 0,
      sku: '', categoryId: this.categories()[0]?.id ?? 0, isActive: true
    };
    this.showForm.set(true);
  }

  openEditForm(product: Product): void {
    this.editingProductId.set(product.id);
    this.formError.set('');
    this.uploadedImageUrls.set([...product.imageUrls]);
    this.formData = {
      name: product.name,
      description: product.description,
      price: product.price,
      stockQuantity: product.stockQuantity,
      sku: product.sku,
      categoryId: product.categoryId,
      isActive: product.isActive
    };
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.isUploading.set(true);
    this.formError.set('');

    this.uploadService.uploadImage(file).subscribe({
      next: (result) => {
        this.uploadedImageUrls.update(current => [...current, result.url]);
        this.isUploading.set(false);
        input.value = '';
      },
      error: (err) => {
        this.formError.set(err.error?.message || 'Image upload failed.');
        this.isUploading.set(false);
      }
    });
  }

  removeImage(url: string): void {
    this.uploadedImageUrls.update(current => current.filter(u => u !== url));
  }

  saveProduct(): void {
    this.formError.set('');
    const editingId = this.editingProductId();

    if (editingId === null) {
      if (this.uploadedImageUrls().length === 0) {
        this.formError.set('Please upload at least one image.');
        return;
      }

      const request: CreateProductRequest = {
        name: this.formData.name,
        description: this.formData.description,
        price: this.formData.price,
        stockQuantity: this.formData.stockQuantity,
        sku: this.formData.sku,
        categoryId: this.formData.categoryId,
        imageUrls: this.uploadedImageUrls()
      };

      this.productService.create(request).subscribe({
        next: () => {
          this.showForm.set(false);
          this.loadProducts();
        },
        error: (err) => this.formError.set(err.error?.message || 'Could not create product.')
      });
    } else {
      const request: UpdateProductRequest = {
        name: this.formData.name,
        description: this.formData.description,
        price: this.formData.price,
        stockQuantity: this.formData.stockQuantity,
        sku: this.formData.sku,
        categoryId: this.formData.categoryId,
        isActive: this.formData.isActive
      };

      this.productService.update(editingId, request).subscribe({
        next: () => {
          this.showForm.set(false);
          this.loadProducts();
        },
        error: (err) => this.formError.set(err.error?.message || 'Could not update product.')
      });
    }
  }

  deleteProduct(product: Product): void {
    this.confirmDialog.open({
      title: 'Delete Product',
      message: `Are you sure you want to delete "${product.name}"? This cannot be undone.`,
      confirmLabel: 'Delete',
      onConfirm: () => {
        this.productService.delete(product.id).subscribe({
          next: () => this.loadProducts(),
          error: (err) => alert(err.error?.message || 'Could not delete product.')
        });
      }
    });
  }
}