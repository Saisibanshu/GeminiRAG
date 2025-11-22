import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ApiService } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule],
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.css'
})
export class DocumentUploadComponent {
  isUploading = false;
  uploadProgress = 0; // Not real progress, just indeterminate state for now
  selectedFile: File | null = null;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService
  ) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0] ?? null;
  }

  upload() {
    const currentStore = this.storeService.getCurrentStore();
    if (!this.selectedFile || !currentStore) return;

    this.isUploading = true;
    this.apiService.uploadDocument(currentStore.name, this.selectedFile).subscribe({
      next: () => {
        this.isUploading = false;
        this.selectedFile = null;
        // Trigger refresh of document list (could be done via a shared subject or just reloading page/component)
        // Ideally DocumentList listens to a refresh signal.
        // For now, we can just re-select the store to trigger reload in DocumentList if they share the same parent or service.
        // Or add a refresh method to StoreService.
        this.storeService.selectStore(currentStore); // Re-emit to trigger reload
        alert('Upload successful!');
      },
      error: (err) => {
        this.isUploading = false;
        console.error(err);
        alert('Upload failed.');
      }
    });
  }
}
