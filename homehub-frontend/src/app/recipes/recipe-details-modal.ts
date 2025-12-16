import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Recipe, RecipeStep, RecipeIngredient } from '../services/api.service';

@Component({
  selector: 'app-recipe-details-modal',
  imports: [CommonModule],
  templateUrl: './recipe-details-modal.html',
  standalone: true
})
export class RecipeDetailsModal implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() recipe: Recipe | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() edit = new EventEmitter<Recipe>();

  private readonly apiService = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  fullRecipe: Recipe | null = null;
  isLoading = false;
  imageExpanded = false;

  ngOnInit(): void {
    // If modal is already open with a recipe when component initializes, load details
    if (this.recipe && this.isOpen) {
      this.loadRecipeDetails();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    const recipeChanged = changes['recipe']?.currentValue !== changes['recipe']?.previousValue;
    const isOpening = changes['isOpen']?.currentValue === true &&
      (changes['isOpen']?.previousValue === false || changes['isOpen']?.previousValue === undefined);

    // When modal opens or recipe changes, load details
    if (this.isOpen && this.recipe) {
      if (isOpening || recipeChanged) {
        // Load immediately when modal opens
        this.loadRecipeDetails();
      }
    }

    if (changes['isOpen']?.currentValue === false) {
      this.imageExpanded = false;
      this.fullRecipe = null;
    }
  }

  loadRecipeDetails(): void {
    if (!this.recipe) {
      return;
    }

    // Always fetch full details to ensure we have steps and ingredients
    // Even if the recipe object has them, we want the latest data
    this.isLoading = true;
    this.fullRecipe = null; // Clear previous data while loading

    this.apiService.getRecipeById(this.recipe.id).subscribe({
      next: (recipe) => {
        this.fullRecipe = recipe;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading recipe details:', err);
        // Fallback to the recipe we have if fetch fails
        this.fullRecipe = this.recipe;
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  toggleImageExpand(): void {
    this.imageExpanded = !this.imageExpanded;
  }

  onEdit(): void {
    if (this.fullRecipe) {
      this.edit.emit(this.fullRecipe);
    }
  }

  onClose(): void {
    this.imageExpanded = false;
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }

  getSortedSteps(): RecipeStep[] {
    if (!this.fullRecipe?.steps) {
      return [];
    }
    return [...this.fullRecipe.steps].sort((a, b) => a.order - b.order);
  }

  getIngredientName(ingredient: RecipeIngredient): string {
    return ingredient.inventoryItem?.name || 'Unknown Item';
  }
}
