import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatChipsModule, MatSnackBarModule],
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.css'
})
export class DocumentUploadComponent {
  isUploading = false;
  selectedFiles: File[] = [];
  isDragging = false;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) { }

  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    this.addFiles(files);
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.addFiles(files);
    }
  }

  addFiles(fileList: FileList) {
    const newFiles = Array.from(fileList).filter(f => f.type === 'application/pdf');
    this.selectedFiles.push(...newFiles);

    if (newFiles.length < fileList.length) {
      this.snackBar.open('Only PDF files are accepted', 'Close', { duration: 3000 });
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  upload() {
    const currentStore = this.storeService.getCurrentStore();
    if (this.selectedFiles.length === 0 || !currentStore) return;

    this.isUploading = true;
    let completed = 0;
    const total = this.selectedFiles.length;

    this.selectedFiles.forEach(file => {
      this.apiService.uploadDocument(currentStore.name, file).subscribe({
        next: () => {
          completed++;
          if (completed === total) {
            this.isUploading = false;
            this.selectedFiles = [];
            this.storeService.selectStore(currentStore);
            this.snackBar.open(`${total} file(s) uploaded successfully!`, 'Close', { duration: 3000 });
          }
        },
        error: (err) => {
          completed++;
          console.error(err);
          this.snackBar.open(`Failed to upload ${file.name}`, 'Close', { duration: 3000 });
          if (completed === total) {
            this.isUploading = false;
          }
        }
      });
    });
  }
}
