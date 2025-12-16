import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Recipe as ApiRecipe } from '../services/api.service';
import { ToastService } from '../services/toast.service';

export interface Recipe {
  id: string;
  title: string;
  description: string;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-recipes',
  imports: [CommonModule, RouterLink],
  templateUrl: './recipes.html',
})
export class Recipes implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly toastService = inject(ToastService);

  recipes: Recipe[] = [];
  isLoading = false;
  hasError = false;

  ngOnInit(): void {
    this.loadRecipes();
  }

  loadRecipes(): void {
    this.isLoading = true;
    this.hasError = false;

    this.apiService.getRecipes(1, 100).subscribe({
      next: (response) => {
        if (response.items && Array.isArray(response.items)) {
          this.recipes = response.items.map(item => this.mapApiRecipeToComponentRecipe(item));
        } else {
          this.recipes = [];
        }

        this.isLoading = false;
        this.hasError = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading recipes:', err);
        this.toastService.error('An error occurred while loading recipes. Please try again later.');
        this.isLoading = false;
        this.hasError = true;
        this.cdr.detectChanges();
      }
    });
  }

  private mapApiRecipeToComponentRecipe(apiRecipe: ApiRecipe): Recipe {
    return {
      id: apiRecipe.id,
      title: apiRecipe.title,
      description: apiRecipe.description,
      createdAt: new Date(apiRecipe.createdAt),
      updatedAt: apiRecipe.updatedAt ? new Date(apiRecipe.updatedAt) : undefined,
    };
  }

  formatDate(date: Date): string {
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }).format(date);
  }

  openAddRecipe(): void {
    // TODO: Implement add recipe modal/form
    console.log('Add new recipe clicked');
  }
}
