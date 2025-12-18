import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Recipe as ApiRecipe, CreateRecipeRequest, UpdateRecipeRequest, GeneratedRecipeResponse } from '../services/api.service';
import { ToastService } from '../services/toast.service';
import { RecipeModal } from './recipe-modal';
import { RecipeDetailsModal } from './recipe-details-modal';
import { GenerateRecipesModal, GenerateRecipesFormData } from './generate-recipes-modal';
import { GeneratedRecipesModal } from './generated-recipes-modal';

export interface Recipe {
  id: string;
  title: string;
  description: string;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-recipes',
  imports: [CommonModule, RouterLink, RecipeModal, RecipeDetailsModal, GenerateRecipesModal, GeneratedRecipesModal],
  templateUrl: './recipes.html',
})
export class Recipes implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly toastService = inject(ToastService);

  recipes: Recipe[] = [];
  isLoading = false;
  hasError = false;
  isModalOpen = false;
  isDetailsModalOpen = false;
  isEditModalOpen = false;
  isGenerateRecipesModalOpen = false;
  isGeneratedRecipesModalOpen = false;
  isGeneratingRecipes = false;
  selectedRecipe: ApiRecipe | null = null;
  recipeToEdit: ApiRecipe | null = null;
  generatedRecipes: GeneratedRecipeResponse[] = [];
  wasDetailsModalOpen = false;

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
    this.isModalOpen = true;
    this.cdr.detectChanges();
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.cdr.detectChanges();
  }

  saveRecipe(recipe: CreateRecipeRequest): void {
    this.apiService.createRecipe(recipe).subscribe({
      next: () => {
        this.toastService.success('Recipe created successfully!');
        this.closeModal();
        this.loadRecipes();
      },
      error: (err) => {
        this.toastService.error('Failed to create recipe. Please try again.');
        console.error('Error creating recipe:', err);
      }
    });
  }

  openRecipeDetails(recipe: Recipe): void {
    // Convert component Recipe to API Recipe
    const apiRecipe: ApiRecipe = {
      id: recipe.id,
      title: recipe.title,
      description: recipe.description,
      createdAt: recipe.createdAt.toISOString(),
      updatedAt: recipe.updatedAt?.toISOString()
    };
    this.selectedRecipe = apiRecipe;
    this.isDetailsModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDetailsModal(): void {
    this.isDetailsModalOpen = false;
    this.selectedRecipe = null;
    this.cdr.detectChanges();
  }

  openEditModal(recipe: ApiRecipe): void {
    // Remember if details modal was open
    this.wasDetailsModalOpen = this.isDetailsModalOpen;
    this.recipeToEdit = recipe;
    this.isEditModalOpen = true;
    // Keep details modal open in the background (it will be behind the edit modal)
    this.cdr.detectChanges();
  }

  closeEditModal(): void {
    this.isEditModalOpen = false;
    this.recipeToEdit = null;
    // Restore details modal if it was open before
    if (this.wasDetailsModalOpen) {
      this.isDetailsModalOpen = true;
      this.wasDetailsModalOpen = false;
    }
    this.cdr.detectChanges();
  }

  updateRecipe(recipe: UpdateRecipeRequest): void {
    if (!this.recipeToEdit) {
      return;
    }

    this.apiService.updateRecipe(this.recipeToEdit.id, recipe).subscribe({
      next: (updatedRecipe) => {
        this.toastService.success('Recipe updated successfully!');
        this.closeEditModal();
        this.loadRecipes();
        // Update the selected recipe and ensure details modal is open
        this.selectedRecipe = updatedRecipe;
        this.isDetailsModalOpen = true;
        this.wasDetailsModalOpen = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastService.error('Failed to update recipe. Please try again.');
        console.error('Error updating recipe:', err);
      }
    });
  }

  openGenerateRecipesModal(): void {
    this.isGenerateRecipesModalOpen = true;
    this.cdr.detectChanges();
  }

  closeGenerateRecipesModal(): void {
    this.isGenerateRecipesModalOpen = false;
    this.cdr.detectChanges();
  }

  generateRecipesFromInventory(formData: GenerateRecipesFormData): void {
    this.isGeneratingRecipes = true;
    this.cdr.detectChanges();

    this.apiService.generateRecipesFromInventory({
      inventoryItemIds: formData.inventoryItemIds,
      userDescription: formData.userDescription || undefined
    }).subscribe({
      next: (generatedRecipes: GeneratedRecipeResponse[]) => {
        this.isGeneratingRecipes = false;
        this.closeGenerateRecipesModal();

        if (generatedRecipes && generatedRecipes.length > 0) {
          this.generatedRecipes = generatedRecipes;
          this.isGeneratedRecipesModalOpen = true;
          this.toastService.success(`Successfully generated ${generatedRecipes.length} recipe(s)!`);
        } else {
          this.toastService.info('No recipes were generated. Try selecting different items or adding more information.');
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isGeneratingRecipes = false;
        this.toastService.error('Failed to generate recipes. Please try again.');
        console.error('Error generating recipes:', err);
        this.cdr.detectChanges();
      }
    });
  }

  closeGeneratedRecipesModal(): void {
    this.isGeneratedRecipesModalOpen = false;
    this.generatedRecipes = [];
    this.loadRecipes();
    this.cdr.detectChanges();
  }

  saveGeneratedRecipe(generatedRecipe: GeneratedRecipeResponse): void {
    const createRequest: CreateRecipeRequest = {
      title: generatedRecipe.title,
      description: generatedRecipe.description,
      steps: generatedRecipe.steps.map(step => ({
        order: step.order,
        description: step.description
      })),
      ingredients: generatedRecipe.ingredients.map(ingredient => ({
        inventoryItemId: ingredient.inventoryItemId,
        quantity: ingredient.quantity
      }))
    };

    this.apiService.createRecipe(createRequest).subscribe({
      next: () => {
        this.toastService.success('Recipe saved successfully!');
        // Remove the saved recipe from the generated recipes list
        this.generatedRecipes = this.generatedRecipes.filter(r => r !== generatedRecipe);
        if (this.generatedRecipes.length === 0) {
          this.closeGeneratedRecipesModal();
        } else {
          this.cdr.detectChanges();
        }
        this.loadRecipes();
      },
      error: (err) => {
        this.toastService.error('Failed to save recipe. Please try again.');
        console.error('Error saving recipe:', err);
      }
    });
  }
}
