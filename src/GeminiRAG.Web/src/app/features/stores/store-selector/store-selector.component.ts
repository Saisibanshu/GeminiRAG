import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { ApiService, StoreInfo } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-store-selector',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatSnackBarModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './store-selector.component.html',
  styleUrl: './store-selector.component.css'
})
export class StoreSelectorComponent implements OnInit {
  stores: StoreInfo[] = [];
  selectedStore: StoreInfo | null = null;
  newStoreName: string = '';
  isCreating = false;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadStores();
    this.storeService.selectedStore$.subscribe(store => {
      this.selectedStore = store;
    });
  }

  loadStores() {
    this.apiService.getStores().subscribe(stores => {
      this.stores = stores;
      // Auto-select if only one? Or keep previous selection?
      if (this.selectedStore) {
        const found = this.stores.find(s => s.name === this.selectedStore?.name);
        if (found) {
          this.selectedStore = found;
          this.storeService.selectStore(found);
        } else {
          this.storeService.clearSelection();
        }
      }
    });
  }

  onStoreSelected(store: StoreInfo) {
    this.storeService.selectStore(store);
  }

  toggleCreate() {
    this.isCreating = !this.isCreating;
  }

  createStore() {
    const nameToUse = this.newStoreName.trim() || `Store ${new Date().toLocaleString()}`;

    this.apiService.createStore(nameToUse).subscribe({
      next: (response) => {
        this.newStoreName = '';
        this.isCreating = false;
        this.loadStores();
        this.snackBar.open(`Store "${nameToUse}" created successfully`, 'Close', { duration: 3000 });
      },
      error: (err) => {
        console.error(err);
        this.snackBar.open('Failed to create store', 'Close', { duration: 3000 });
      }
    });
  }

  deleteStore(store: StoreInfo, event: Event) {
    event.stopPropagation();
    if (confirm(`Are you sure you want to delete store "${store.displayName}"?`)) {
      this.apiService.deleteStore(store.name).subscribe({
        next: () => {
          this.snackBar.open('Store deleted successfully', 'Close', { duration: 3000 });
          this.loadStores();
          if (this.selectedStore?.name === store.name) {
            this.storeService.clearSelection();
            this.selectedStore = null;
          }
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || 'Failed to delete store';
          this.snackBar.open(msg, 'Close', { duration: 5000 });
        }
      });
    }
  }
}
