import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category } from '../../../../core/models/category.model';
import { CategoryService } from '../../../../core/services/category';


@Component({
  selector: 'app-admin-category-list',
  imports: [FormsModule],
  templateUrl: './admin-category-list.html',
  styleUrl: './admin-category-list.css'
})
export class AdminCategoryList implements OnInit {
  private categoryService = inject(CategoryService);

  categories = signal<Category[]>([]);
  isLoading = signal(true);

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
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  openCreateForm(): void {
    this.editingId.set(null);
    this.formError.set('');
    this.formData = { name: '', description: '' };
    this.showForm.set(true);
  }

  openEditForm(category: Category): void {
    this.editingId.set(category.id);
    this.formError.set('');
    this.formData = { name: category.name, description: category.description ?? '' };
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
      description: this.formData.description || undefined
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

  deleteCategory(id: number): void {
    if (!confirm('Delete this category? Products in it will not be deleted, but this action cannot be undone.')) return;

    this.categoryService.delete(id).subscribe({
      next: () => this.loadCategories(),
      error: (err) => alert(err.error?.message || 'Could not delete category.')
    });
  }
}