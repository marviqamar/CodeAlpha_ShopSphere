import { Component, OnInit, inject, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { CategoryService } from '../../../../core/services/category';
import { Category } from '../../../../core/models/category.model';
import { UploadService } from '../../../../core/services/upload';
import { environment } from '../../../../../environments/environment';


@Component({
  selector: 'app-admin-category-list',
  imports: [FormsModule, ConfirmDialog],
  templateUrl: './admin-category-list.html',
  styleUrl: './admin-category-list.css'
})
export class AdminCategoryList implements OnInit {
  private categoryService = inject(CategoryService);

  @ViewChild(ConfirmDialog) confirmDialog!: ConfirmDialog;

  allCategories: Category[] = [];
  categories = signal<Category[]>([]);
  isLoading = signal(true);
  searchTerm = '';

  showForm = signal(false);
  editingId = signal<number | null>(null);
  formError = signal('');

  formData = { name: '', description: '' };

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.allCategories = data;
        this.applyFilter();
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  applyFilter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.categories.set(
      term ? this.allCategories.filter(c => c.name.toLowerCase().includes(term)) : this.allCategories
    );
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.formError.set('');
    this.formData = { name: '', description: '' };
    this.uploadedImageUrl.set(null);
    this.showForm.set(true);
  }

  openEditForm(category: Category): void {
    this.editingId.set(category.id);
    this.formError.set('');
    this.formData = { name: category.name, description: category.description ?? '' };
    this.uploadedImageUrl.set(category.imageUrl ?? null);
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
  }

  saveCategory(): void {
    this.formError.set('');
    const editingId = this.editingId();

    const request = {
      name: this.formData.name,
      description: this.formData.description || undefined,
      imageUrl: this.uploadedImageUrl() || undefined
    };

    const call = editingId === null
      ? this.categoryService.create(request)
      : this.categoryService.update(editingId, request);

    call.subscribe({
      next: () => {
        this.showForm.set(false);
        this.loadCategories();
      },
      error: (err) => this.formError.set(err.error?.message || 'Could not save category.')
    });
  }

  deleteCategory(category: Category): void {
    this.confirmDialog.open({
      title: 'Delete Category',
      message: `Delete "${category.name}"? Products in this category will not be deleted, but this cannot be undone.`,
      confirmLabel: 'Delete',
      onConfirm: () => {
        this.categoryService.delete(category.id).subscribe({
          next: () => this.loadCategories(),
          error: (err) => alert(err.error?.message || 'Could not delete category.')
        });
      }
    });
  }

  private uploadService = inject(UploadService);
  imageBaseUrl = environment.apiUrl.replace('/api', '');
  uploadedImageUrl = signal<string | null>(null);
  isUploading = signal(false);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    this.isUploading.set(true);
    this.formError.set('');

    this.uploadService.uploadImage(input.files[0]).subscribe({
      next: (result) => {
        this.uploadedImageUrl.set(result.url);
        this.isUploading.set(false);
        input.value = '';
      },
      error: (err) => {
        this.formError.set(err.error?.message || 'Image upload failed.');
        this.isUploading.set(false);
      }
    });
  }
}