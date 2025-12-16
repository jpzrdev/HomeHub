import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, InventoryItem, CreateRecipeRequest, UpdateRecipeRequest, Recipe } from '../services/api.service';

export interface RecipeFormData {
  title: string;
  description: string;
  steps: { order: number; description: string }[];
  ingredients: { inventoryItemId: string; quantity: number }[];
}

@Component({
  selector: 'app-recipe-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './recipe-modal.html',
  standalone: true
})
export class RecipeModal implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() recipe: Recipe | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<CreateRecipeRequest | UpdateRecipeRequest>();

  private readonly apiService = inject(ApiService);

  formData: RecipeFormData = {
    title: '',
    description: '',
    steps: [],
    ingredients: []
  };

  inventoryItems: InventoryItem[] = [];
  isLoadingInventory = false;
  errors: { [key: string]: string } = {};
  isEditMode = false;

  ngOnInit(): void {
    this.loadInventoryItems();
    this.initializeForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const isOpening = changes['isOpen']?.currentValue === true &&
      (changes['isOpen']?.previousValue === false || changes['isOpen']?.previousValue === undefined);

    const isClosing = changes['isOpen']?.currentValue === false &&
      changes['isOpen']?.previousValue === true;

    const recipeChanged = changes['recipe']?.currentValue !== changes['recipe']?.previousValue;

    if (isOpening || (recipeChanged && this.isOpen)) {
      this.initializeForm();
      if (this.inventoryItems.length === 0) {
        this.loadInventoryItems();
      }
    }

    if (isClosing) {
      this.resetForm();
    }
  }

  private initializeForm(): void {
    if (!this.isOpen) {
      return;
    }

    if (this.recipe) {
      this.isEditMode = true;
      // Load full recipe details if needed
      if (!this.recipe.steps || !this.recipe.ingredients) {
        this.apiService.getRecipeById(this.recipe.id).subscribe({
          next: (fullRecipe) => {
            this.populateFormFromRecipe(fullRecipe);
          },
          error: (err) => {
            console.error('Error loading recipe for edit:', err);
            this.resetForm();
          }
        });
      } else {
        this.populateFormFromRecipe(this.recipe);
      }
    } else {
      this.isEditMode = false;
      this.resetForm();
    }
  }

  private populateFormFromRecipe(recipe: Recipe): void {
    this.formData = {
      title: recipe.title || '',
      description: recipe.description || '',
      steps: recipe.steps
        ? recipe.steps.map(step => ({
          order: step.order,
          description: step.description
        }))
        : [],
      ingredients: recipe.ingredients
        ? recipe.ingredients.map(ingredient => ({
          inventoryItemId: ingredient.inventoryItemId,
          quantity: ingredient.quantity
        }))
        : []
    };
    this.errors = {};
  }

  private resetForm(): void {
    this.formData = {
      title: '',
      description: '',
      steps: [],
      ingredients: []
    };
    this.errors = {};
  }

  loadInventoryItems(): void {
    this.isLoadingInventory = true;
    this.apiService.getInventoryItems(1, 1000).subscribe({
      next: (response) => {
        this.inventoryItems = response.items || [];
        this.isLoadingInventory = false;
      },
      error: (err) => {
        console.error('Error loading inventory items:', err);
        this.inventoryItems = [];
        this.isLoadingInventory = false;
      }
    });
  }

  addStep(): void {
    const nextOrder = this.formData.steps.length + 1;
    this.formData.steps.push({
      order: nextOrder,
      description: ''
    });
  }

  removeStep(index: number): void {
    this.formData.steps.splice(index, 1);
    // Reorder remaining steps
    this.formData.steps.forEach((step, idx) => {
      step.order = idx + 1;
    });
  }

  addIngredient(): void {
    this.formData.ingredients.push({
      inventoryItemId: '',
      quantity: 0
    });
  }

  removeIngredient(index: number): void {
    this.formData.ingredients.splice(index, 1);
  }

  getInventoryItemName(id: string): string {
    const item = this.inventoryItems.find(i => i.id === id);
    return item ? item.name : '';
  }

  validateForm(): boolean {
    this.errors = {};

    if (!this.formData.title || this.formData.title.trim().length === 0) {
      this.errors['title'] = 'Title is required';
    }

    // Validate steps
    this.formData.steps.forEach((step, index) => {
      if (!step.description || step.description.trim().length === 0) {
        this.errors[`step_${index}`] = 'Step description is required';
      }
    });

    // Validate ingredients
    this.formData.ingredients.forEach((ingredient, index) => {
      if (!ingredient.inventoryItemId) {
        this.errors[`ingredient_item_${index}`] = 'Inventory item is required';
      }
      if (ingredient.quantity <= 0) {
        this.errors[`ingredient_quantity_${index}`] = 'Quantity must be greater than 0';
      }
    });

    return Object.keys(this.errors).length === 0;
  }

  onSave(): void {
    if (!this.validateForm()) {
      return;
    }

    const request: CreateRecipeRequest | UpdateRecipeRequest = {
      title: this.formData.title.trim(),
      description: this.formData.description.trim(),
      steps: this.formData.steps.map(step => ({
        order: step.order,
        description: step.description.trim()
      })),
      ingredients: this.formData.ingredients.map(ingredient => ({
        inventoryItemId: ingredient.inventoryItemId,
        quantity: ingredient.quantity
      }))
    };

    this.save.emit(request);
  }

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.onClose();
    }
  }
}
