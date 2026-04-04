import {Component, EventEmitter, inject, Input, OnInit, Output, TemplateRef} from '@angular/core';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {ToastrService} from 'ngx-toastr';
import {FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {AlbumDto, ErrorResponseDto, SongDto} from '../../../modules/lpshows-api';
import {NgClass} from '@angular/common';
import {AlbumsService} from '../../../services/music/albums.service';
import {AppleMusicService} from '../../../services/music/apple-music.service';

@Component({
  selector: 'app-song-form',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgClass
  ],
  templateUrl: './song-form.component.html',
  styleUrl: './song-form.component.css',
})
export class SongFormComponent implements OnInit {
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private formBuilder = inject(FormBuilder);
  private albumsService = inject(AlbumsService);
  private appleMusicService = inject(AppleMusicService);

  @Input("is-saving")
  isSaving$: boolean = false;

  @Output("saveClicked")
  saveClicked = new EventEmitter<SongFormContent>();

  songForm = this.formBuilder.group({
    title: new FormControl('', [Validators.required]),
    selectedAlbumId: new FormControl(0, []),
    isrc: new FormControl('', [Validators.pattern(/^(?<country>[A-Z]{2})(?<issuedBy>[A-Z0-9]{3})(?<year>\d{2})(?<number>\d{5})$/)]),
    linkinpediaUrl: new FormControl('', [Validators.pattern(/^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/=]*)/)]),
  });

  availableAlbums$: AlbumDto[] = [];

  ngOnInit() {
    this.albumsService.getAllAlbums(true)
      .subscribe({
        next: data => {
          this.availableAlbums$ = data;
        },
        error: err => {
          let errorResponse: ErrorResponseDto = err.error;
          this.toastr.error(errorResponse.message, "Could load albums");
        }
      });

    this.songForm.controls.isrc.valueChanges.subscribe(isrc => {
      this.onIsrcChanged();
    })
  }

  openModal(content: TemplateRef<any>) {
    return this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' });
  }


  openLinkinpediaUrlClicked() {
    let url = this.songForm.value.linkinpediaUrl?.valueOf();
    if (url?.length == 0) {
      return;
    }

    window.open(url, "_blank");
  }


  onSaveClicked() {
    let content = this.readFromForm();
    if (content) {
      this.saveClicked.emit(content!);
    }
  }


  private readFromForm(): SongFormContent | null {
    let title = this.songForm.value.title?.valueOf();
    let albumId = this.songForm.value.selectedAlbumId?.valueOf();
    let isrc = this.songForm.value.isrc?.valueOf();
    let linkinpediaUrl = this.songForm.value.linkinpediaUrl?.valueOf();

    if (title == undefined) {
      this.toastr.error("Title is required");
      return null;
    }

    return {
      title: title!,
      albumId: albumId ?? null,
      isrc: isrc ?? null,
      linkinpediaUrl: linkinpediaUrl ?? null
    };
  }


  public fillFormWith(song: SongDto) {
    console.debug("Fill form with data:", song);
    this.songForm.controls.title.setValue(song.title ?? null);
    this.songForm.controls.selectedAlbumId.setValue(song.album?.id ?? null);
    this.songForm.controls.isrc.setValue(song.isrc ?? null);
    this.songForm.controls.linkinpediaUrl.setValue(song.linkinpediaUrl ?? null);
  }


  onIsrcChanged() {
    let isrc = this.songForm.value.isrc?.valueOf();
    if (isrc) {
      this.appleMusicService.getSongsForIsrc(isrc ?? "").then(songs => {
        console.debug("Songs from Isrc:", songs);
        console.debug("Albums:", songs.map(song => song.attributes?.albumName));
      })
    }
  }
}

export class SongFormContent {
  title!: string;
  albumId: number | null = null;
  isrc: string | null = null;
  linkinpediaUrl: string | null = null;
}
