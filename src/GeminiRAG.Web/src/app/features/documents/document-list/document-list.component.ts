import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService, DocumentInfo, StoreInfo } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule, MatTooltipModule, MatMenuModule, MatSnackBarModule],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.css'
})
export class DocumentListComponent implements OnInit {
  documents: DocumentInfo[] = [];
  displayedColumns: string[] = ['displayName', 'uploadDate', 'status', 'actions'];
  currentStore: StoreInfo | null = null;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.storeService.selectedStore$.subscribe(store => {
      this.currentStore = store;
      if (store) {
        this.loadDocuments(store.name);
      } else {
        this.documents = [];
      }
    });
  }

  loadDocuments(storeName: string) {
    this.apiService.getDocuments(storeName).subscribe(docs => {
      this.documents = docs;
    });
  }

  deleteDocument(doc: DocumentInfo) {
    if (confirm(`Are you sure you want to delete "${doc.displayName}"?`)) {
      this.apiService.deleteDocument(doc.name).subscribe({
        next: () => {
          this.snackBar.open('File deleted successfully', 'Close', { duration: 3000 });
          if (this.currentStore) {
            this.loadDocuments(this.currentStore.name);
          }
        },
        error: (err) => {
          console.error(err);
          this.snackBar.open('Failed to delete file', 'Close', { duration: 3000 });
        }
      });
    }
  }
}
