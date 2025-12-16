import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/services/api.service';

@Component({
    selector: 'app-supported-types',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatChipsModule, MatIconModule, MatProgressSpinnerModule],
    template: `
    <div class="container mx-auto p-6 max-w-4xl">
      <mat-card class="mb-6">
        <mat-card-header>
          <mat-card-title class="text-2xl font-bold">
            <mat-icon class="align-middle mr-2">description</mat-icon>
            Supported File Types
          </mat-card-title>
          <mat-card-subtitle *ngIf="!loading">
            {{count}} file formats supported by Versnn RAG
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div *ngIf="loading" class="text-center p-8">
            <mat-spinner class="mx-auto"></mat-spinner>
            <p class="mt-4">Loading supported types...</p>
          </div>

          <div *ngIf="!loading">
            <div class="mb-6">
              <h3 class="text-lg font-semibold mb-3 flex items-center">
                <mat-icon class="mr-2 text-blue-600">article</mat-icon>
                Documents
              </h3>
              <mat-chip-set>
                <mat-chip *ngFor="let ext of documentTypes">{{ext}}</mat-chip>
              </mat-chip-set>
            </div>

            <div class="mb-6">
              <h3 class="text-lg font-semibold mb-3 flex items-center">
                <mat-icon class="mr-2 text-green-600">table_chart</mat-icon>
                Spreadsheets
              </h3>
              <mat-chip-set>
                <mat-chip *ngFor="let ext of spreadsheetTypes">{{ext}}</mat-chip>
              </mat-chip-set>
            </div>

            <div class="mb-6">
              <h3 class="text-lg font-semibold mb-3 flex items-center">
                <mat-icon class="mr-2 text-purple-600">code</mat-icon>
                Code Files
              </h3>
              <mat-chip-set>
                <mat-chip *ngFor="let ext of codeTypes">{{ext}}</mat-chip>
              </mat-chip-set>
            </div>

            <div class="mb-6">
              <h3 class="text-lg font-semibold mb-3 flex items-center">
                <mat-icon class="mr-2 text-orange-600">text_snippet</mat-icon>
                Text & Markup
              </h3>
              <mat-chip-set>
                <mat-chip *ngFor="let ext of textTypes">{{ext}}</mat-chip>
              </mat-chip-set>
            </div>

            <div class="bg-blue-50 p-4 rounded">
              <p class="text-sm text-gray-700">
                <mat-icon class="text-sm align-middle mr-1">info</mat-icon>
                <strong>Note:</strong> All uploaded files are validated for security.
                Executable files (.exe, .dll, .bat) and files with mismatched extensions are automatically rejected.
              </p>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
    styles: [`
    mat-chip {
      margin: 4px;
    }
  `]
})
export class SupportedTypesComponent implements OnInit {
    loading = true;
    count = 0;
    documentTypes: string[] = [];
    spreadsheetTypes: string[] = [];
    codeTypes: string[] = [];
    textTypes: string[] = [];

    constructor(private apiService: ApiService) { }

    ngOnInit() {
        this.apiService.getSupportedFileTypes().subscribe({
            next: (response) => {
                this.count = response.count;
                this.categorizeExtensions(response.extensions);
                this.loading = false;
            },
            error: (err) => {
                console.error('Failed to load supported types', err);
                //  Fallback categories
                this.documentTypes = ['.pdf', '.doc', '.docx', '.odt', '.rtf'];
                this.spreadsheetTypes = ['.xls', '.xlsx', '.csv', '.tsv'];
                this.codeTypes = ['.js', '.ts', '.py', '.java', '.c', '.cpp', '.cs', '.go', '.rs'];
                this.textTypes = ['.txt', '.md', '.html', '.css', '.json', '.xml'];
                this.count = 100;
                this.loading = false;
            }
        });
    }

    categorizeExtensions(extensions: string[]) {
        const docs = ['.pdf', '.doc', '.docx', '.docm', '.odt', '.rtf'];
        const sheets = ['.xls', '.xlsx', '.xlsm', '.csv', '.tsv'];
        const code = ['.js', '.jsx', '.ts', '.tsx', '.py', '.java', '.c', '.cpp', '.h', '.hpp',
            '.cs', '.go', '.rs', '.rb', '.php', '.swift', '.kt', '.scala', '.sh', '.ps1', '.sql'];
        const text = ['.txt', '.md', '.markdown', '.html', '.htm', '.css', '.scss', '.sass',
            '.json', '.xml', '.yaml', '.yml', '.rst', '.tex'];

        this.documentTypes = extensions.filter(e => docs.includes(e));
        this.spreadsheetTypes = extensions.filter(e => sheets.includes(e));
        this.codeTypes = extensions.filter(e => code.includes(e));
        this.textTypes = extensions.filter(e => text.includes(e));
    }
}
