import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ApiService, DocumentInfo, StoreInfo } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.css'
})
export class DocumentListComponent implements OnInit {
  documents: DocumentInfo[] = [];
  displayedColumns: string[] = ['displayName', 'uploadDate', 'status'];
  currentStore: StoreInfo | null = null;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService
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
}
