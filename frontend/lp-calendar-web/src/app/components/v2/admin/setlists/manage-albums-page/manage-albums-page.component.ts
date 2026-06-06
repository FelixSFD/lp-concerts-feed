import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
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
import {ConfirmationService} from 'primeng/api';

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
  private toastr = inject(ToastrService);
  private confirmationService = inject(ConfirmationService);
  private albumsService = inject(AlbumsService);


  albums$: AlbumDto[] = [];

  isDeletingAlbum$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


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
    this.isDeletingAlbum$ = true;

    if (album) {
      this.albumsService.deleteAlbum(album.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingAlbum$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not delete album!");
            this.isDeletingAlbum$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.isLoading$ = true;
    this.albumsService.getAllAlbums(cache).subscribe({
      next: albums => {
        this.albums$ = albums;
        this.isLoading$ = false;
      },
      error: err => {
        this.isLoading$ = false;
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load albums!");
      }
    })
  }
}
