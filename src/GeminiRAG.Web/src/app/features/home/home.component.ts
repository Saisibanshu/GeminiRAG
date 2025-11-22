import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreSelectorComponent } from '../stores/store-selector/store-selector.component';
import { DocumentListComponent } from '../documents/document-list/document-list.component';
import { DocumentUploadComponent } from '../documents/document-upload/document-upload.component';
import { ChatInterfaceComponent } from '../query/chat-interface/chat-interface.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    StoreSelectorComponent,
    DocumentListComponent,
    DocumentUploadComponent,
    ChatInterfaceComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent { }
