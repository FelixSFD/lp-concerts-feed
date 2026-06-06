import {Component, inject} from '@angular/core';
import {ToastrService} from 'ngx-toastr';
import {SongsService} from '../../../../../services/songs.service';
import {ErrorResponseDto, SongDto} from '../../../../../modules/lpshows-api';
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
  selector: 'app-manage-songs-page',
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
  templateUrl: './manage-songs-page.component.html',
  styleUrl: './manage-songs-page.component.css',
})
export class ManageSongsPageComponent {
  private toastr = inject(ToastrService);
  private confirmationService = inject(ConfirmationService);
  private songsService = inject(SongsService);


  songs$: SongDto[] = [];

  isDeletingSong$ = false;

  // true while data is being loaded
  isLoading$ = false;

  globalSearchText$: string = "";


  ngOnInit() {
    this.reloadList(false);
  }


  onDeleteSongClicked(event: Event, song: SongDto) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Do you want to delete the song "${song.title}"?`,
      header: 'Delete Song',
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
        this.onDeleteSongConfirm(song);
      }
    });
  }


  onDeleteSongConfirm(song: SongDto) {
    this.isDeletingSong$ = true;

    if (song) {
      this.songsService.deleteSong(song.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingSong$ = false;
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.toastr.error(errorResponse.message, "Could not delete song!");
            this.isDeletingSong$ = false;
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.songsService.getAllSongs(cache).subscribe({
      next: songs => {
        this.songs$ = songs;
      },
      error: err => {
        let errorResponse: ErrorResponseDto = err.error;
        this.toastr.error(errorResponse.message, "Could not load songs!");
      }
    })
  }
}
