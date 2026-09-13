import { Component, inject, signal } from '@angular/core';
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
import {ConfirmationService, MessageService} from 'primeng/api';

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
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private songsService = inject(SongsService);


  songs$ = signal<SongDto[]>([]);

  isDeletingSong$ = signal(false);

  // true while data is being loaded
  isLoading$ = signal(false);

  globalSearchText$ = signal("");


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
    this.isDeletingSong$.set(true);

    if (song) {
      this.songsService.deleteSong(song.id!)
        .subscribe({
          next: () => {
            this.reloadList(false);
            this.isDeletingSong$.set(false);
          },
          error: err => {
            let errorResponse: ErrorResponseDto = err.error;
            this.messageService.add({
              severity: "danger",
              summary: "Could not load delete song!",
              text: errorResponse.message,
            });
            this.isDeletingSong$.set(false);
          }
        });
    }
  }


  private reloadList(cache: boolean) {
    this.isLoading$.set(true);
    this.songsService.getAllSongs(cache).subscribe({
      next: songs => {
        this.songs$.set(songs);
        this.isLoading$.set(false);
      },
      error: err => {
        this.isLoading$.set(false);
        let errorResponse: ErrorResponseDto = err.error;
        this.messageService.add({
          severity: "danger",
          summary: "Could not load songs!",
          text: errorResponse.message,
        });
      }
    })
  }
}
