import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../../core/services/api.service';
import { StoreService } from '../../../core/services/store.service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.css'
})
export class DocumentUploadComponent {
  isUploading = false;
  selectedFiles: File[] = [];
  isDragging = false;
  supportedExtensions: string[] = [];
  supportedExtensionsLoaded = false;

  constructor(
    private apiService: ApiService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) {
    this.loadSupportedTypes();
  }

  loadSupportedTypes() {
    this.apiService.getSupportedFileTypes().subscribe({
      next: (response) => {
        this.supportedExtensions = response.extensions;
        this.supportedExtensionsLoaded = true;
      },
      error: (err) => {
        console.error('Failed to load supported file types:', err);
        // Fallback to common types
        this.supportedExtensions = ['.pdf', '.doc', '.docx', '.txt', '.md'];
        this.supportedExtensionsLoaded = true;
      }
    });
  }

  onFileSelected(event: any) {
    const files: FileList = event.target.files;
    this.addFiles(files);
    // Reset input so same file can be selected again
    event.target.value = '';
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
    const filesArray = Array.from(fileList);
    const validFiles: File[] = [];
    const invalidFiles: string[] = [];

    filesArray.forEach(file => {
      const extension = this.getFileExtension(file.name);

      if (this.isFileSupported(extension)) {
        validFiles.push(file);
      } else {
        invalidFiles.push(file.name);
      }
    });

    this.selectedFiles.push(...validFiles);

    if (invalidFiles.length > 0) {
      const extensions = this.supportedExtensions.slice(0, 5).join(', ');
      this.snackBar.open(
        `${invalidFiles.length} file(s) rejected. Supported types include: ${extensions}...`,
        'View All',
        {
          duration: 5000
        }
      ).onAction().subscribe(() => {
        this.showSupportedTypes();
      });
    }
  }

  getFileExtension(fileName: string): string {
    const lastDot = fileName.lastIndexOf('.');
    return lastDot === -1 ? '' : fileName.substring(lastDot).toLowerCase();
  }

  isFileSupported(extension: string): boolean {
    if (!this.supportedExtensionsLoaded) {
      // While loading, accept common types
      return ['.pdf', '.doc', '.docx', '.txt', '.md', '.csv', '.json'].includes(extension);
    }
    return this.supportedExtensions.includes(extension);
  }

  showSupportedTypes() {
    const message = `Supported file types (${this.supportedExtensions.length}):\n\n` +
      `Documents: PDF, DOC, DOCX, ODT, RTF\n` +
      `Spreadsheets: XLS, XLSX, CSV, TSV\n` +
      `Code: JS, TS, PY, JAVA, C, CPP, CS, GO, RS, PHP\n` +
      `Text: TXT, MD, HTML, CSS, JSON, XML, YAML\n` +
      `And many more!`;

    alert(message);
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  upload() {
    const currentStore = this.storeService.getCurrentStore();
    if (this.selectedFiles.length === 0 || !currentStore) return;

    this.isUploading = true;
    let completed = 0;
    let successful = 0;
    const total = this.selectedFiles.length;

    this.selectedFiles.forEach(file => {
      this.apiService.uploadDocument(currentStore.name, file).subscribe({
        next: (response) => {
          completed++;
          successful++;

          if (completed === total) {
            this.isUploading = false;
            this.selectedFiles = [];
            this.storeService.selectStore(currentStore);
            this.snackBar.open(
              `${successful}/${total} file(s) uploaded successfully!`,
              'Close',
              { duration: 3000 }
            );
          }
        },
        error: (err) => {
          completed++;
          console.error('Upload error:', err);

          const errorMsg = err.error?.error || err.error?.message || 'Upload failed';
          this.snackBar.open(
            `${file.name}: ${errorMsg}`,
            'Close',
            { duration: 5000 }
          );

          if (completed === total) {
            this.isUploading = false;
            if (successful > 0) {
              this.selectedFiles = [];
              this.storeService.selectStore(currentStore);
            }
          }
        }
      });
    });
  }

  get acceptedTypes(): string {
    // Return accept attribute for file input
    return this.supportedExtensions.join(',');
  }

  get uploadButtonDisabled(): boolean {
    return this.selectedFiles.length === 0 || this.isUploading;
  }
}
