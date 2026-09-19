import { Component, inject, signal } from '@angular/core';
import {AlbumDto, ErrorResponseDto} from '../../../../../modules/lpshows-api';
import {AlbumsService} from '../../../../../services/music/albums.service';
import {RouterLink} from '@angular/router';
import {Button} from 'primeng/button';
import {ButtonGroup} from 'primeng/buttongroup';
import {Card} from 'primeng/card';
import {ConfirmDialog} from 'primeng/confirmdialog';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {TableModule} from 'primeng/table';
import {ConfirmationService, MessageService} from 'primeng/api';

@Component({
  selector: 'app-manage-albums-page',
  imports: [
    RouterLink,
    Button,
    ButtonGroup,
    Card,
    ConfirmDialog,
    IconField,
    InputIcon,
    InputText,
    ReactiveFormsModule,
    TableModule,
    FormsModule
  ],
  templateUrl: './manage-albums-page.component.html',
  styleUrl: './manage-albums-page.component.css',
})
export class ManageAlbumsPageComponent {
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private albumsService = inject(AlbumsService);


  albums$ = signal<AlbumDto[]>([]);

  isDeletingAlbum$ = signal(false);

  // true while data is being loaded
  isLoading$ = signal(false);

  globalSearchText$ = signal("");


  ngOnInit() {
    this.reloadList(false);
  }


  onDeleteAlbumClicked(event: Event, album: AlbumDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the album "${album.title}"?`,
      header: 'Delete Album',
      icon: 'pi pi-info-circle',
      rejectLabel: 'Cancel',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger'
      },

      accept: () => {
        this.onDeleteAlbumConfirm(album);
      }
    });
  }


  onDeleteAlbumConfirm(album: AlbumDto) {
    this.isDeletingAlbum$.set(true);

    if (album) {
      this.albumsService.deleteAlbum(album.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingAlbum$.set(false);
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not load delete album!",
              text: errorResponse.message,
            });
            this.isDeletingAlbum$.set(false);
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.isLoading$.set(true);
    this.albumsService.getAllAlbums(cache).subscribe({
      next: albums => {
        this.albums$.set(albums);
        this.isLoading$.set(false);
      },
      error: err => {
        this.isLoading$.set(false);
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "danger",
          summary: "Could not load albums!",
          text: errorResponse.message,
        });
      }
    })
  }
}
